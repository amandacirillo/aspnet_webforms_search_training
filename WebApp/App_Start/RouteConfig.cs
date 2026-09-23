using System.Web.Routing;

namespace SkillsSearchWeb
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.RouteExistingFiles = false;
            routes.MapPageRoute("Search", "search", "~/SkillsSearch.aspx");
        }
    }
}
