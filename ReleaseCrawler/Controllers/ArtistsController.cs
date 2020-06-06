using ReleaseCrawler.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ReleaseCrawler.Controllers
{
    public class ArtistsController : ApiController
    {
        DataContext db = new DataContext();

        public HttpResponseMessage Get()
        {
            var artists = db.Releases.Select(m => m.Artists.Trim()).Distinct();
            
            return Request.CreateResponse(HttpStatusCode.OK, artists.ToList(), MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}