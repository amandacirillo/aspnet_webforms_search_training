using System;
using System.Collections.Generic;

namespace SkillsSearch.Core
{
    /// <summary>Well-known technical skill categories, matching the original
    /// TechSkills table's Category column values (one renamed for genericness --
    /// see README).</summary>
    public static class TechSkillCategories
    {
        public const string ProgrammingTools = "Programming Tools";
        public const string InternallyDeveloped = "Internally Developed Software";
        public const string ExternalSoftware = "External Software";

        public static IReadOnlyList<string> All { get; } = new[]
        {
            ProgrammingTools,
            InternallyDeveloped,
            ExternalSoftware,
        };
    }
}
