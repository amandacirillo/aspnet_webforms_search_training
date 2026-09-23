using System.Collections.Generic;
using SkillsSearch.Core.Models;

namespace SkillsSearch.Core
{
    /// <summary>
    /// In-memory <see cref="ISkillsRepository"/> useful for local development,
    /// demos, and UI-layer tests that don't need a real database.
    /// </summary>
    public sealed class InMemorySkillsRepository : ISkillsRepository
    {
        private readonly List<StaffMember> _staff;
        private readonly Dictionary<string, List<string>> _techSkillsByCategory;
        private readonly List<string> _psychSkills;
        private readonly List<string> _testingProgramNames;
        private readonly List<TestingProgramInfo> _testingProgramInfo;

        public InMemorySkillsRepository(
            List<StaffMember> staff,
            Dictionary<string, List<string>> techSkillsByCategory,
            List<string> psychSkills,
            List<string> testingProgramNames,
            List<TestingProgramInfo> testingProgramInfo)
        {
            _staff = staff;
            _techSkillsByCategory = techSkillsByCategory;
            _psychSkills = psychSkills;
            _testingProgramNames = testingProgramNames;
            _testingProgramInfo = testingProgramInfo;
        }

        public IReadOnlyList<StaffMember> GetStaffSkills() => _staff;

        public IReadOnlyList<string> GetTechSkillCategory(string category) =>
            _techSkillsByCategory.TryGetValue(category, out var skills) ? skills : new List<string>();

        public IReadOnlyList<string> GetPsychometricSkills() => _psychSkills;

        public IReadOnlyList<string> GetTestingProgramNames() => _testingProgramNames;

        public IReadOnlyList<TestingProgramInfo> GetTestingProgramInfo() => _testingProgramInfo;
    }
}
