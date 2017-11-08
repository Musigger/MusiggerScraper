using ReleaseCrowler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ReleaseCrowler.Controllers
{
    public class LabelsController : ApiController
    {
        DataContext db = new DataContext();

        public HttpResponseMessage Get()
        {
            var labels = db.Releases.Select(m => m.Label).Distinct();
            Request.Properties["Count"] = labels.Count();

            return Request.CreateResponse(HttpStatusCode.OK, labels.ToList(), MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}