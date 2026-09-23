using System.Collections.Generic;
using SkillsSearch.Core.Models;

namespace SkillsSearch.Core
{
    /// <summary>
    /// Abstraction over the data source so the web layer (and tests) don't
    /// depend on a live SQL Server connection.
    /// </summary>
    public interface ISkillsRepository
    {
        IReadOnlyList<StaffMember> GetStaffSkills();
        IReadOnlyList<string> GetTechSkillCategory(string category);
        IReadOnlyList<string> GetPsychometricSkills();
        IReadOnlyList<string> GetTestingProgramNames();
        IReadOnlyList<TestingProgramInfo> GetTestingProgramInfo();
    }
}
