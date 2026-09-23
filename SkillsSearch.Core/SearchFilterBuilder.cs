using System;
using System.Collections.Generic;
using System.Linq;
using SkillsSearch.Core.Models;

namespace SkillsSearch.Core
{
    /// <summary>
    /// Replaces the original code-behind's `UpdateQuery()` method, which built
    /// a SQL-like filter expression by string-concatenating unescaped user
    /// input (checkbox values and free-text search terms) and applied it via
    /// <c>DataView.RowFilter</c>. That's the same class of bug as string-built
    /// SQL: <c>DataView.RowFilter</c> has its own expression syntax (it
    /// supports things like sub-selects and functions), so unescaped input
    /// containing a stray quote or expression operator can change what the
    /// filter does, not just what it matches.
    ///
    /// This version filters an in-memory collection directly with LINQ and
    /// plain string comparisons -- no expression string is ever built from
    /// user input, so there's nothing to inject into.
    /// </summary>
    public static class SearchFilterBuilder
    {
        public static IEnumerable<StaffMember> Search(IEnumerable<StaffMember> staff, SearchCriteria criteria)
        {
            if (staff == null) throw new ArgumentNullException(nameof(staff));
            if (criteria == null) throw new ArgumentNullException(nameof(criteria));

            return staff.Where(member => Matches(member, criteria));
        }

        private static bool Matches(StaffMember member, SearchCriteria criteria)
        {
            var effectiveSkills = EffectiveSkillSet(member, criteria.SkillLevel);

            foreach (var skill in criteria.SelectedSkills)
            {
                if (!Contains(effectiveSkills, skill))
                {
                    return false;
                }
            }

            if (criteria.FreeTextKeywords.Count > 0)
            {
                var generalInfo = GeneralInfo(member);
                foreach (var keyword in criteria.FreeTextKeywords)
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                    {
                        continue;
                    }
                    if (generalInfo.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return false;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(criteria.TestingProgram) && criteria.TestingProgram != "None")
            {
                bool inPrimary = string.Equals(member.TestingPrograms, criteria.TestingProgram, StringComparison.OrdinalIgnoreCase);
                bool inOther = member.OtherTestingPrograms.IndexOf(criteria.TestingProgram, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!inPrimary && !inOther)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Which skill lists count toward a match, based on the requested
        /// skill level -- mirrors the original Mast_Tech_Skills /
        /// Best_Tech_Skills / All_Tech_Skills computed columns.
        /// </summary>
        private static IEnumerable<string> EffectiveSkillSet(StaffMember member, SkillLevelFilter level)
        {
            IEnumerable<string> skills = level switch
            {
                SkillLevelFilter.MasteredOnly => member.MasteredTechSkills.Concat(member.MasteredPsychSkills),
                SkillLevelFilter.ExperiencedOrMastered => member.ExperiencedTechSkills
                    .Concat(member.MasteredTechSkills)
                    .Concat(member.ExperiencedPsychSkills)
                    .Concat(member.MasteredPsychSkills),
                _ => member.SomeTechSkills
                    .Concat(member.ExperiencedTechSkills)
                    .Concat(member.MasteredTechSkills)
                    .Concat(member.SomePsychSkills)
                    .Concat(member.ExperiencedPsychSkills)
                    .Concat(member.MasteredPsychSkills),
            };
            return skills.Concat(member.OtherSkills);
        }

        private static bool Contains(IEnumerable<string> skills, string target) =>
            skills.Any(skill => string.Equals(skill, target, StringComparison.OrdinalIgnoreCase));

        private static string GeneralInfo(StaffMember member) => string.Join(
            ",",
            member.EmployeeName,
            member.TestingPrograms,
            member.OtherTestingPrograms,
            string.Join(",", EffectiveSkillSet(member, SkillLevelFilter.Any)));
    }
}
