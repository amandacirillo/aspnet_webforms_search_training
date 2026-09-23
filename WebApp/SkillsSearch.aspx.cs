using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI.WebControls;
using SkillsSearch.Core;
using SkillsSearch.Core.Models;

namespace SkillsSearchWeb
{
    /// <summary>
    /// Rewritten version of the original PARsearchSearch page. All data
    /// access and filtering logic now live in SkillsSearch.Core (which is
    /// unit tested); this code-behind only wires the UI controls to that
    /// library. See the repo README for a full list of what changed and why.
    /// </summary>
    public partial class SkillsSearchPage : System.Web.UI.Page
    {
        // In a real deployment, wire this up via your DI container of choice
        // (e.g. Unity, Autofac, or plain constructor injection through a
        // custom PageBase). Constructing it directly here keeps the example
        // focused on the search logic itself.
        private ISkillsRepository Repository => new SqlSkillsRepository(
            ConfigurationManager.ConnectionStrings["SkillsDb"].ConnectionString);

        private List<string> _allPsychSkills = new List<string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadReferenceData();
            }
            RunSearch();
        }

        private void LoadReferenceData()
        {
            try
            {
                var repo = Repository;

                ckList_ProgTools.DataSource = repo.GetTechSkillCategory(TechSkillCategories.ProgrammingTools);
                ckList_ProgTools.DataBind();

                ckList_Internal.DataSource = repo.GetTechSkillCategory(TechSkillCategories.InternallyDeveloped);
                ckList_Internal.DataBind();

                ckList_Extsoft.DataSource = repo.GetTechSkillCategory(TechSkillCategories.ExternalSoftware);
                ckList_Extsoft.DataBind();

                _allPsychSkills = repo.GetPsychometricSkills().ToList();
                ckList_Psych.DataSource = _allPsychSkills;
                ckList_Psych.DataBind();

                foreach (var programName in repo.GetTestingProgramNames())
                {
                    cb_TPs.Items.Add(new ListItem(programName, programName));
                }

                dgv_TP.DataSource = repo.GetTestingProgramInfo();
                dgv_TP.DataBind();
            }
            catch (Exception ex)
            {
                // Fix vs. original: surface a visible error in the page and
                // let the exception be logged by whatever the hosting
                // pipeline's error logging is (Application_Error, App
                // Insights, etc.) -- instead of calling
                // System.Windows.Forms.MessageBox.Show(), a desktop API that
                // has no effect when called from a web request and blocks
                // the worker thread on an invisible dialog.
                ShowError("Could not load reference data: " + ex.Message);
            }
        }

        private void RunSearch()
        {
            try
            {
                var repo = Repository;
                var criteria = BuildCriteriaFromControls();
                var results = SearchFilterBuilder.Search(repo.GetStaffSkills(), criteria).ToList();
                dgv_Staff.DataSource = results;
                dgv_Staff.DataBind();
            }
            catch (Exception ex)
            {
                ShowError("Search failed: " + ex.Message);
            }
        }

        private SearchCriteria BuildCriteriaFromControls()
        {
            var selectedSkills = new List<string>();
            AddSelected(ckList_ProgTools, selectedSkills);
            AddSelected(ckList_Internal, selectedSkills);
            AddSelected(ckList_Extsoft, selectedSkills);
            AddSelected(ckList_Psych, selectedSkills);

            var keywords = (tb_Search.Text ?? string.Empty)
                .Split(',')
                .Select(k => k.Trim())
                .Where(k => k.Length > 0)
                .ToList();

            var level = SkillLevelFilter.Any;
            if (radioBtn_Master.Checked) level = SkillLevelFilter.MasteredOnly;
            else if (radioBtn_Exp_or_Master.Checked) level = SkillLevelFilter.ExperiencedOrMastered;

            return new SearchCriteria
            {
                SelectedSkills = selectedSkills,
                FreeTextKeywords = keywords,
                TestingProgram = cb_TPs.SelectedValue,
                SkillLevel = level,
            };
        }

        private static void AddSelected(CheckBoxList list, List<string> into)
        {
            foreach (ListItem item in list.Items)
            {
                if (item.Selected)
                {
                    into.Add(item.Value);
                }
            }
        }

        protected void tb_Search_TextChanged(object sender, EventArgs e) => RunSearch();

        protected void SelectedSkills_Changed(object sender, EventArgs e) => RunSearch();

        protected void SkillLevel_CheckedChanged(object sender, EventArgs e) => RunSearch();

        protected void cb_TPs_SelectedIndexChanged(object sender, EventArgs e) => RunSearch();

        protected void tb_PsychSearch_TextChanged(object sender, EventArgs e)
        {
            var filter = tb_PsychSearch.Text ?? string.Empty;
            ckList_Psych.DataSource = string.IsNullOrEmpty(filter)
                ? _allPsychSkills
                : _allPsychSkills.Where(s => s.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            ckList_Psych.DataBind();
            RunSearch();
        }

        protected void btn_clear_Click(object sender, EventArgs e)
        {
            tb_Search.Text = string.Empty;
            tb_PsychSearch.Text = string.Empty;
            cb_TPs.ClearSelection();
            radioBtn_Any.Checked = true;
            radioBtn_Exp_or_Master.Checked = false;
            radioBtn_Master.Checked = false;
            ClearSelections(ckList_ProgTools);
            ClearSelections(ckList_Internal);
            ClearSelections(ckList_Extsoft);
            ClearSelections(ckList_Psych);
            RunSearch();
        }

        private static void ClearSelections(CheckBoxList list)
        {
            foreach (ListItem item in list.Items)
            {
                item.Selected = false;
            }
        }

        private void ShowError(string message)
        {
            lbl_Error.Text = message;
            lbl_Error.Visible = true;
        }
    }
}
