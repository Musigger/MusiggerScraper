using ReleaseCrawler;
using ReleaseCrowler.CustomClasses;
using ReleaseCrowler.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ReleaseCrowler.Controllers
{
    public class ReleasesController : ApiController
    {
        private DataContext db = new DataContext();

        public HttpResponseMessage Get(int p = 1, int votes = 0, string label = "", string genres ="", string type="")
        {
            var releases = db.Releases.Where(m => m.Votes > votes && m.Genres.Contains(genres));

            var parseResult = Enum.TryParse<ReleaseType>(type, out var relType);

            if (parseResult)
            {
                releases = releases.Where(m => m.Type == relType);
            }

            Paginator<Release> paginator = new Paginator<Release>(releases.OrderByDescending(n=>n.ReleaseId).ToList());
            
            return Request.CreateResponse(HttpStatusCode.OK, paginator.GetPage(p), MediaTypeHeaderValue.Parse("application/json"));
        }

        public HttpResponseMessage Get(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, db.Releases.Find(id), MediaTypeHeaderValue.Parse("application/json"));
        }


        //// GET: api/Releases  
        //public IQueryable<Release> GetReleases()
        //{
        //    return db.Releases;
        //}

        //// GET: api/Releases/5
        //[ResponseType(typeof(Release))]
        //public async Task<IHttpActionResult> GetRelease(int id)
        //{
        //    Release release = await db.Releases.FindAsync(id);
        //    if (release == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(release);
        //}

        // PUT: api/Releases/5

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ReleaseExists(int id)
        {
            return db.Releases.Count(e => e.Id == id) > 0;
        }
    }
}