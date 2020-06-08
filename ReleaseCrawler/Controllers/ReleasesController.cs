using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ReleaseCrawler.CustomClasses;
using ReleaseCrawler.Models;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ReleaseCrawler.Controllers
{
    public class ReleasesController : ApiController
    {
        private DataContext db = new DataContext();

        public async System.Threading.Tasks.Task<HttpResponseMessage> GetAsync(int p = 1, int votes = 0, int perPage = 24, string labels = "", string genres = "", string types = "", string artists = "", string title = "")
        {
            var releases = db.Releases.AsQueryable();

            if (votes > 0)
            {
                releases = releases.Where(m => m.Votes >= votes);
            }

            try
            {
                if (title != null && title != "")
                {
                    title = title.Trim();

                    releases = releases
                        .Where(m => EF.Functions.FreeText(m.Name, title));
                }
            }
            catch { }

            try
            {
                if (artists != null && artists != "")
                {
                    artists = artists.Trim();

                    releases = releases
                     .Where(m => EF.Functions.FreeText(m.Artists, artists));
                }
            }
            catch { }

            try
            {
                if (labels != null && labels != "")
                {
                    string[] labelsList = labels.Split(',');

                    if (labelsList.Count() > 0)
                        releases = releases.Where(m => labelsList.Contains(m.Label));

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
                        var sourceReleases = releases;
                        releases = Enumerable.Empty<Release>().AsQueryable();

                        foreach (var genre in genreList)
                            releases = releases.Union(sourceReleases.Where(m => EF.Functions.FreeText(m.Genres, genre)));

                    }
                }
            }
            catch { }

            try
            {
                if (types != null && types != "")
                {
                    string[] typeList = types.Split(',');

                    var parsedTypes = new List<ReleaseType>();

                    foreach (var type in typeList)
                        parsedTypes.Add((ReleaseType)Enum.Parse(typeof(ReleaseType), type, true));

                    if (parsedTypes.Count() > 0)
                        releases = releases.Where(m => parsedTypes.Contains(m.Type));

                }
            }
            catch { }



            if (perPage == 0 || perPage >= 100)
                perPage = 24;


            int startIndex = perPage * (p - 1);

            var page = releases.OrderByDescending(m => m.ReleaseId).Skip(startIndex).Take(perPage).AsEnumerable();

            var result = page.Select(s => new ReleaseDetails(s)).ToList();

            HttpContext.Current.Response.AppendHeader("X-Total", releases.Count().ToString());

            return Request.CreateResponse(HttpStatusCode.OK, result, MediaTypeHeaderValue.Parse("application/json"));
        }

        public HttpResponseMessage Get(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new ReleaseDetails(db.Releases.Find(id)), MediaTypeHeaderValue.Parse("application/json"));
        }

        public HttpResponseMessage Get(int id, bool update)
        {
            var release = db.Releases.Find(id);
            var expDate = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.AppendHeader("X-Total", "1");

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

            string cover = "";
            string miniCover = "";
            try
            {
                var node = releasePage.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "fancybox")).First();
                cover = node.Attributes["href"].Value;//обложка
                var miniCoverNode = node.ChildNodes.First();
                miniCover = miniCoverNode.Attributes["src"].Value;

            }

            catch { }

            var downloads = releasePage.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("link-numm")).First().InnerHtml.Replace("Скачиваний: ", ""); //загрузки, точнее их количесто

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
            release.Rating = decimal.Parse(rate, NumberStyles.Any, new CultureInfo("en-US"));
            release.Votes = int.Parse(vote);
            release.Info = info;
            release.Links = links;
            release.Downloads = int.Parse(downloads);
            release.Cover = cover;
            release.MiniCover = miniCover;

            db.Entry(release).State = EntityState.Modified;
            db.SaveChanges();


            return Request.CreateResponse(HttpStatusCode.OK, new ReleaseDetails(release), MediaTypeHeaderValue.Parse("application/json"));
        }



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