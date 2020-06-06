using ReleaseCrawler.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ReleaseCrawler.Controllers
{
    public class LabelsController : ApiController
    {
        DataContext db = new DataContext();

        public HttpResponseMessage Get()
        {
            var labels = db.Releases.Select(m => m.Label).Distinct();

            return Request.CreateResponse(HttpStatusCode.OK, labels.ToList(), MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}