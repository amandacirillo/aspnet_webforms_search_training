using System.Web.Optimization;

namespace SkillsSearchWeb
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/site.js"));
        }
    }
}
