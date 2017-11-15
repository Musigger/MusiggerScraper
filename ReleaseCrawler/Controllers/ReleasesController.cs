using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ReleaseCrawler;
using ReleaseCrawler.CustomClasses;
using ReleaseCrawler.Models;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using static System.Net.WebRequestMethods;
using ReleaseCrawler.Controllers;
using System;

namespace ReleaseCrawler.Controllers
{
    public class ReleasesController : ApiController
    {
        private DataContext db = new DataContext();

        public HttpResponseMessage Get(int p = 1, int votes = 0, int perPage = 24, string labels = "", string genres ="", string types="", string artists = "")
        {
            var releases = db.Releases.Where(m => m.Votes >= votes);

            try
            {
                if (artists != null && artists != "")
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
                if (labels != null && labels != "")
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
                if (genres != null && genres != "")
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
                if (types != null && types != "")
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
            Request.Properties["Count"] = "1";

            return Request.CreateResponse(HttpStatusCode.OK, new ReleaseDetails(db.Releases.Find(id)), MediaTypeHeaderValue.Parse("application/json"));
        }
        
        public HttpResponseMessage Get (int id, bool update)
        {
            var release = db.Releases.Find(id);
            var expDate = DateTime.Now.AddDays(-1);
            Request.Properties["Count"] = "1";

            if (release.VoteRateUpdated > expDate)
            {
                return Request.CreateResponse(HttpStatusCode.NotModified, release, MediaTypeHeaderValue.Parse("application/json"));
            }

            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            var releaseResponse = webClient.DownloadString("http://freake.ru/" + release.ReleaseId);
            HtmlDocument releaseDoc = new HtmlDocument();
            releaseDoc.LoadHtml(releaseResponse);
            var releasePage = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "post")).First();

            string rateId = "rate-r-" + release.ReleaseId;
            var rate = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@id,'{0}')]", rateId)).First().InnerHtml; //рейтинг
            string voteId = "rate-v-" + release.ReleaseId;
            var vote = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@id,'{0}')]", voteId)).First().InnerHtml; //голоса
            var info = releasePage.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("unreset")).First().InnerHtml;//инфо (треклист, прослушка, итц)
            
            //пост запросом получаем хитро спрятанный ссылки на скачивание
            var res = HttpInvoker.Post("http://freake.ru/engine/modules/ajax/music.link.php", new NameValueCollection() {
                            { "id", release.ReleaseId.ToString() }
                        });
            var str = Encoding.Default.GetString(res);
            JObject json = JObject.Parse(str);
            string links = "";
            if (json["answer"].ToString() == "ok")
            {
                links = json["link"].ToString().Replace("Ссылки на скачивание", "Download links");
            }
            ////////////////////////////////////////////////////////////////

            release.VoteRateUpdated = DateTime.Now;
            release.Rating = decimal.Parse(rate.Replace('.', ','));
            release.Votes = int.Parse(vote);
            release.Info = info;
            release.Links = links;

            db.Entry(release).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();


            return Request.CreateResponse(HttpStatusCode.OK, new ReleaseDetails(release), MediaTypeHeaderValue.Parse("application/json"));
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