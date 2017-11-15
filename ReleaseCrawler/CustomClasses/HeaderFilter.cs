using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace ReleaseCrawler.CustomClasses
{
    public class AddCustomHeaderFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var count = actionExecutedContext.Request.Properties["Count"];
            actionExecutedContext.Response.Content.Headers.Add("X-Total", count.ToString());
        }
    }
    
}