using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ReleaseCrowler.Controllers
{
    public class InfoController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var info = File.ReadAllText("C:\\Musigger\\Backend\\bin");

            return Request.CreateResponse(HttpStatusCode.OK, info, MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}
