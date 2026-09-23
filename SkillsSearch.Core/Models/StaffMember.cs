using System.Collections.Generic;

namespace SkillsSearch.Core.Models
{
    /// <summary>
    /// One row of the staff skills directory. Field names mirror the original
    /// "tmp_Staff_Skills" table's columns, just in plain C# so the search
    /// logic can be unit tested without a database.
    /// </summary>
    public sealed class StaffMember
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string TestingPrograms { get; set; } = string.Empty;
        public string OtherTestingPrograms { get; set; } = string.Empty;
        public IReadOnlyList<string> SomeTechSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> ExperiencedTechSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> MasteredTechSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> SomePsychSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> ExperiencedPsychSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> MasteredPsychSkills { get; set; } = new List<string>();
        public IReadOnlyList<string> OtherSkills { get; set; } = new List<string>();
        public string LastUpdated { get; set; } = string.Empty;
    }
}
