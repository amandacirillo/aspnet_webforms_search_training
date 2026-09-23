using System.Collections.Generic;
using SkillsSearch.Core.Models;
using Xunit;

namespace SkillsSearch.Core.Tests
{
    public class SearchFilterBuilderTests
    {
        private static StaffMember Alice() => new StaffMember
        {
            EmployeeName = "Alice Example",
            Email = "alice@example.com",
            JobTitle = "Data Analyst",
            TestingPrograms = "MATH101",
            OtherTestingPrograms = "SCI200",
            SomeTechSkills = new List<string> { "SAS" },
            ExperiencedTechSkills = new List<string> { "SQL", "Python" },
            MasteredTechSkills = new List<string> { "R" },
            SomePsychSkills = new List<string> { "IRT" },
            ExperiencedPsychSkills = new List<string>(),
            MasteredPsychSkills = new List<string> { "Equating" },
            OtherSkills = new List<string>(),
        };

        private static StaffMember Bob() => new StaffMember
        {
            EmployeeName = "Bob Example",
            Email = "bob@example.com",
            JobTitle = "Psychometrician",
            TestingPrograms = "SCI200",
            OtherTestingPrograms = "",
            SomeTechSkills = new List<string>(),
            ExperiencedTechSkills = new List<string> { "SAS" },
            MasteredTechSkills = new List<string> { "SQL" },
            SomePsychSkills = new List<string>(),
            ExperiencedPsychSkills = new List<string> { "IRT" },
            MasteredPsychSkills = new List<string>(),
            OtherSkills = new List<string>(),
        };

        private static IReadOnlyList<StaffMember> Directory() => new List<StaffMember> { Alice(), Bob() };

        [Fact]
        public void NoCriteria_ReturnsEveryone()
        {
            var result = SearchFilterBuilder.Search(Directory(), new SearchCriteria());
            Assert.Equal(2, System.Linq.Enumerable.Count(result));
        }

        [Fact]
        public void SelectedSkill_FiltersToMembersWithThatSkill_AnySkillLevel()
        {
            var criteria = new SearchCriteria { SelectedSkills = new List<string> { "R" } };
            var result = SearchFilterBuilder.Search(Directory(), criteria);
            Assert.Single(result, m => m.EmployeeName == "Alice Example");
        }

        [Fact]
        public void MasteredOnly_ExcludesSkillsHeldAtLowerLevels()
        {
            // Bob has SQL at Mastered level and SAS at Experienced level.
            var mastered = new SearchCriteria
            {
                SelectedSkills = new List<string> { "SAS" },
                SkillLevel = SkillLevelFilter.MasteredOnly,
            };
            var result = SearchFilterBuilder.Search(Directory(), mastered);
            Assert.Empty(result); // nobody has SAS at Mastered level

            var experiencedOrMastered = new SearchCriteria
            {
                SelectedSkills = new List<string> { "SAS" },
                SkillLevel = SkillLevelFilter.ExperiencedOrMastered,
            };
            var result2 = SearchFilterBuilder.Search(Directory(), experiencedOrMastered);
            Assert.Single(result2, m => m.EmployeeName == "Bob Example");
        }

        [Fact]
        public void FreeTextKeyword_MatchesNameOrTestingProgramOrSkill()
        {
            var byName = new SearchCriteria { FreeTextKeywords = new List<string> { "alice" } };
            Assert.Single(SearchFilterBuilder.Search(Directory(), byName));

            var bySkill = new SearchCriteria { FreeTextKeywords = new List<string> { "IRT" } };
            var bySkillResult = SearchFilterBuilder.Search(Directory(), bySkill);
            Assert.Equal(2, System.Linq.Enumerable.Count(bySkillResult)); // both have IRT at some level
        }

        [Fact]
        public void TestingProgram_MatchesPrimaryOrOtherProgram()
        {
            var criteria = new SearchCriteria { TestingProgram = "SCI200" };
            var result = SearchFilterBuilder.Search(Directory(), criteria);
            Assert.Equal(2, System.Linq.Enumerable.Count(result)); // Alice via "other", Bob via primary
        }

        [Fact]
        public void TestingProgramNoneOrEmpty_DoesNotFilter()
        {
            var criteria = new SearchCriteria { TestingProgram = "None" };
            var result = SearchFilterBuilder.Search(Directory(), criteria);
            Assert.Equal(2, System.Linq.Enumerable.Count(result));
        }

        [Fact]
        public void MaliciousLookingKeyword_IsTreatedAsLiteralText_NotInjected()
        {
            // This is the whole point of not building a filter-expression
            // string from user input: a value that would have been dangerous
            // in the original DataView.RowFilter approach is just an ordinary
            // (non-matching) substring here.
            var criteria = new SearchCriteria
            {
                FreeTextKeywords = new List<string> { "' OR '1'='1" },
            };
            var result = SearchFilterBuilder.Search(Directory(), criteria);
            Assert.Empty(result);
        }

        [Fact]
        public void CombiningSkillAndKeyword_RequiresBothToMatch()
        {
            var criteria = new SearchCriteria
            {
                SelectedSkills = new List<string> { "SQL" },
                FreeTextKeywords = new List<string> { "bob" },
            };
            var result = SearchFilterBuilder.Search(Directory(), criteria);
            Assert.Single(result, m => m.EmployeeName == "Bob Example");
        }
    }
}
