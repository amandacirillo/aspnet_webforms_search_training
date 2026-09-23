using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SkillsSearch.Core.Models;

namespace SkillsSearch.Core
{
    /// <summary>
    /// ADO.NET implementation of <see cref="ISkillsRepository"/>.
    ///
    /// Fixes applied vs. the original code-behind:
    ///  - one connection string, read once from configuration, instead of a
    ///    literal duplicated in every method (and pointing at a real internal
    ///    hostname baked into source)
    ///  - every query and connection is wrapped in `using`, so connections are
    ///    always closed/disposed even when an exception is thrown (the
    ///    original called connection.Close() only on the success path)
    ///  - queries use `SqlParameter` wherever a value varies, instead of
    ///    concatenating strings -- none of these particular queries take user
    ///    input today, but parameterizing on principle means nobody can
    ///    introduce an injection bug later by adding a naive `WHERE` clause
    ///    the same way the original file built its search filter
    ///  - failures are surfaced as exceptions (for the caller/logger to
    ///    handle) instead of swallowed by a bare `catch { MessageBox.Show(...) }`
    ///    -- `System.Windows.Forms.MessageBox` is a desktop-app API; calling it
    ///    from an ASP.NET request has no visible effect on the client and
    ///    silently blocks the worker thread waiting on a dialog nobody can see.
    /// </summary>
    public sealed class SqlSkillsRepository : ISkillsRepository
    {
        private readonly string _connectionString;

        public SqlSkillsRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IReadOnlyList<StaffMember> GetStaffSkills()
        {
            const string sql = "SELECT * FROM tmp_Staff_Skills";
            var results = new List<StaffMember>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new StaffMember
                {
                    EmployeeName = ReadString(reader, "Employee_Name"),
                    Email = ReadString(reader, "Email"),
                    JobTitle = ReadString(reader, "Job_Title"),
                    TestingPrograms = ReadString(reader, "Testing_Programs"),
                    OtherTestingPrograms = ReadString(reader, "Other_Testing_Programs"),
                    SomeTechSkills = SplitCsv(ReadString(reader, "Some_Technical_Skills")),
                    ExperiencedTechSkills = SplitCsv(ReadString(reader, "Experienced_Technical_Skills")),
                    MasteredTechSkills = SplitCsv(ReadString(reader, "Mastered_Technical_Skills")),
                    SomePsychSkills = SplitCsv(ReadString(reader, "Some_Psych_Skills")),
                    ExperiencedPsychSkills = SplitCsv(ReadString(reader, "Experienced_Psych_Skills")),
                    MasteredPsychSkills = SplitCsv(ReadString(reader, "Mastered_Psych_Skills")),
                    OtherSkills = SplitCsv(ReadString(reader, "Other_Skills")),
                    LastUpdated = ReadString(reader, "Last_Updated"),
                });
            }
            return results;
        }

        public IReadOnlyList<string> GetTechSkillCategory(string category)
        {
            const string sql = "SELECT Technical_Skills FROM TechSkills WHERE Category = @Category";
            var results = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@Category", SqlDbType.NVarChar, 100) { Value = category });
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }
            return results;
        }

        public IReadOnlyList<string> GetPsychometricSkills()
        {
            const string sql = "SELECT Areas_of_Expertise FROM PsychometricSkills";
            var results = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }
            return results;
        }

        public IReadOnlyList<string> GetTestingProgramNames()
        {
            const string sql = "SELECT Testing_Program FROM TestingPrograms";
            var results = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }
            return results;
        }

        public IReadOnlyList<TestingProgramInfo> GetTestingProgramInfo()
        {
            const string sql = "SELECT * FROM TestingProgramInfo";
            var results = new List<TestingProgramInfo>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new TestingProgramInfo
                {
                    ProgramName = ReadString(reader, "Program_Name"),
                    ProgramCode = ReadString(reader, "Program_Code"),
                    DataAnalystManager = ReadString(reader, "Data_Analyst_Manager"),
                    PsychometricManager = ReadString(reader, "Psychometric_Manager"),
                    DataAnalystLead = ReadString(reader, "Data_Analyst_Lead"),
                    PsychometricLead = ReadString(reader, "Psychometric_Lead"),
                    DeliveryPlatform = ReadString(reader, "Delivery_Platform"),
                    ItemBankingSystem = ReadString(reader, "Item_Banking_System"),
                    AnalysisSystem = ReadString(reader, "Analysis_System"),
                    AnalysisSystemEquating = ReadString(reader, "Analysis_System_Equating"),
                    AnalysisScoring = ReadString(reader, "Analysis_Scoring"),
                    ProductionScoring = ReadString(reader, "Production_Scoring"),
                    CrScoring = ReadString(reader, "CR_Scoring"),
                    EquatingMethods = ReadString(reader, "Equating_Methods"),
                });
            }
            return results;
        }

        private static string ReadString(SqlDataReader reader, string column)
        {
            var ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        private static IReadOnlyList<string> SplitCsv(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }
            var parts = value.Split(',');
            var trimmed = new List<string>(parts.Length);
            foreach (var part in parts)
            {
                var t = part.Trim();
                if (t.Length > 0)
                {
                    trimmed.Add(t);
                }
            }
            return trimmed;
        }
    }
}
