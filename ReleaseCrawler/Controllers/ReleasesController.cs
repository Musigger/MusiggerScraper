using ReleaseCrawler;
using ReleaseCrowler.CustomClasses;
using ReleaseCrowler.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ReleaseCrowler.Controllers
{
    public class ReleasesController : ApiController
    {
        private DataContext db = new DataContext();

        public HttpResponseMessage Get(int p = 1, int votes = 0, int perPage = 24, string labels = "", string genres ="", string types="", string artists = "")
        {
            var releases = db.Releases.Where(m => m.Votes > votes);

            try
            {
                if (artists != "")
                {
                    string[] artistsList = artists.Split(',');

                    if (artistsList.Count() > 0)
                    {
                        releases = releases.Where(m => artistsList.Any(n => m.Artists.Contains(n)));
                    }
                }
            }
            catch { }

            try
            {
                if (labels != "")
                {
                    string[] labelsList = labels.Split(',');

                    if (labelsList.Count() > 0)
                    {
                        releases = releases.Where(m => labelsList.Any(n => m.Label.Contains(n)));
                    }
                }
            }
            catch { }

            try
            {
                if (genres != "")
                {
                    string[] genreList = genres.Split(',');

                    if (genreList.Count() > 0)
                    {
                        releases = releases.Where(m => genreList.Any(n => m.Genres.Contains(n)));
                    }
                }
            }
            catch { }

            try
            {
                if (types != "")
                {
                    string[] typeList = types.Split(',');

                    if (typeList.Count() > 0)
                    {
                        releases = releases.Where(m => typeList.Contains(m.Type.ToString()));
                    }
                }
            }
            catch { }
           
            Paginator<ReleaseItem> paginator = new Paginator<ReleaseItem>(releases.OrderByDescending(n=>n.ReleaseId).AsEnumerable().Select(s=> new ReleaseItem(s)), perPage);

            Request.Properties["Count"] = releases.Count();

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