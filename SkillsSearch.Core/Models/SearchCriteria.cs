using System.Collections.Generic;

namespace SkillsSearch.Core.Models
{
    /// <summary>Which skill levels count as a "match" for a given search.</summary>
    public enum SkillLevelFilter
    {
        Any,
        ExperiencedOrMastered,
        MasteredOnly,
    }

    /// <summary>
    /// Everything the UI collected from the user, in a plain data object --
    /// the whole point being that nothing here is a string of SQL or a
    /// DataView.RowFilter expression. Building the actual filter predicate is
    /// SearchFilterBuilder's job, and it's the only place that needs testing.
    /// </summary>
    public sealed class SearchCriteria
    {
        public IReadOnlyList<string> SelectedSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> FreeTextKeywords { get; set; } = new List<string>();
        public string? TestingProgram { get; set; }
        public SkillLevelFilter SkillLevel { get; set; } = SkillLevelFilter.Any;
    }
}
