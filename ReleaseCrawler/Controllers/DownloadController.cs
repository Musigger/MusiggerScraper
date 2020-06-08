using Microsoft.EntityFrameworkCore;
using ReleaseCrawler.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ReleaseCrowler.Controllers
{
    public class DownloadController : ApiController
    {
        private DataContext db = new DataContext();

        public HttpResponseMessage Get(int id)
        {
            var release = db.Releases.Find(id);

            if (release == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "release not found", MediaTypeHeaderValue.Parse("application/json"));

            release.DownloadsFromMusigger = release.DownloadsFromMusigger + 1;

            db.Entry(release).State = EntityState.Modified;

            db.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK, release.DownloadsFromMusigger, MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}
