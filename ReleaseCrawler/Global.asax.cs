using ReleaseCrowler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace ReleaseCrawler
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //Database.SetInitializer(new DropCreateDatabaseAlways<DataContext>());


            //DataContext db = new DataContext();

            //Release rel = new Release();

            //rel.Name = "[asdfasdf";
            //rel.VoteCount = 2;
            //rel.ReleaseDate = DateTime.Now;

            //db.Releases.Add(rel);
            //db.SaveChanges();

        }
    }
}
