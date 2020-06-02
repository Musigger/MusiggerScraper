using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ReleaseCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ReleaseCrawler.CustomClasses
{
    public static class ReleaseParser
    {
        public static void Run()
        {
            LogStatus("update started");

            using (WebClient webClient = new WebClient())
            {
                DataContext db = new DataContext();

                webClient.Encoding = Encoding.UTF8;
                int pageNumber = 30;
                int doubles = 0;
                while (pageNumber < 200)
                {
                    var response = webClient.DownloadString("http://freake.ru/?p=" + pageNumber);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(response);
                    //Getting the page
                    var page = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "music-small"));

                    //Retrieveing existing releases to exclude duplicates
                    var allExistingIds = db.Releases.Select(m => m.ReleaseId).ToList();

                    //iterating through page
                    foreach (var item in page)
                    {
                        try
                        {
                            if (doubles > 0)
                            {
                                Console.WriteLine("Doubles: " + doubles);
                            }
                            if (doubles >= 100)
                            {
                                Console.WriteLine("Saving");
                                db.SaveChanges();

                                RemoveDuplicates();

                                LogStatus("updated");

                                return;
                            }

                            var msimage = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-image").First();
                            var a = msimage.Descendants("a").First();

                            //Parsing release ID
                            string releaseId = a.Attributes["href"].Value.Remove(0, 1);

                            if (allExistingIds != null && allExistingIds.Contains(int.Parse(releaseId)))
                            {
                                doubles++;
                                continue;
                            }

                            doubles = 0;

                            var msinfo = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-info").First();
                            var elps = msinfo.Descendants("h3").First();                                                                        //Release name

                            var rateclass = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-rate")).First();
                            //var infoClass = rateclass.Descendants("div").Where(m => m.Attributes["class"].Value == "info").First();             //Vote count

                            var table = msinfo.Descendants("table").First();
                            var date = table.Descendants("tr").First().Descendants("a").First().InnerText;                                      //Release date
                            var type = table.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;                              //Release type

                            //Retrieving the release page
                            var releaseResponse = webClient.DownloadString("http://freake.ru/" + releaseId);
                            HtmlDocument releaseDoc = new HtmlDocument();
                            releaseDoc.LoadHtml(releaseResponse);
                            var releasePage = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "post")).First();

                            string rateId = "rate-r-" + releaseId;
                            var rate = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@id,'{0}')]", rateId)).First().InnerHtml; //rating
                            string voteId = "rate-v-" + releaseId;
                            var vote = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@id,'{0}')]", voteId)).First().InnerHtml; //votes

                            string artists = ""; //string for collecting artists

                            var tablerel = releasePage.Descendants("table").First();

                            foreach (var artist in tablerel.Descendants("tr").First().Descendants("a"))
                            {
                                artists += artist.InnerHtml;
                                artists += ", ";
                            }
                            artists = artists.Substring(0, artists.Length - 2);                                                                 //артисты

                            var label = "";

                            var labelMark = tablerel.SelectNodes("tr").Skip(1).First().Descendants("td").First().InnerText;

                            if (labelMark == "Label:")
                            {
                                label = tablerel.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;                          //лейбл
                            }

                            if ((ReleaseType)Enum.Parse(typeof(ReleaseType), type) == ReleaseType.Radioshow)
                            {
                                label = "";
                            }
                            var info = releasePage.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("unreset")).First().InnerHtml;//инфо (треклист, прослушка, итц)

                            info = info.Replace("http://embed.beatport.com", "https://embed.beatport.com");

                            var downloads = releasePage.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("link-numm")).First().InnerHtml.Replace("Скачиваний: ", ""); //загрузки, точнее их количесто

                            string Cover = "";
                            try
                            {
                                Cover = releasePage.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "fancybox")).First().Attributes["href"].Value;//обложка
                            }
                            catch { }

                            //Getting hidden links with POST
                            var res = HttpInvoker.Post("http://freake.ru/engine/modules/ajax/music.link.php", new NameValueCollection()
                            {
                                {
                                    "id", releaseId
                                }
                            });
                            var str = Encoding.Default.GetString(res);
                            JObject json = JObject.Parse(str);
                            string links = "";
                            if (json["answer"].ToString() == "ok")
                            {
                                links = json["link"].ToString().Replace("Ссылки на скачивание", "Download links");
                            }
                            ////////////////////////////////////////////////////////////////

                            string ReleaseName = elps.Descendants("a").First().InnerText;
                            int Votes = int.Parse(vote);
                            DateTime ReleaseDate = DateTime.ParseExact(date, "dd.MM.yyyy", null);
                            ReleaseType relType = (ReleaseType)Enum.Parse(typeof(ReleaseType), type);
                            int ReleaseId = int.Parse(releaseId);
                            string MiniCover = a.Descendants("img").First().Attributes["src"].Value;
                            string Genres = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-style")).First().InnerText;
                            decimal Rating = decimal.Parse(rate, NumberStyles.Any, new CultureInfo("en-US"));

                            //Creating a release to insert
                            Release release = new Release
                            {
                                Name = ReleaseName,
                                Votes = Votes,
                                Date = ReleaseDate,
                                Type = relType,
                                Artists = artists,
                                Label = label,
                                Info = info,
                                Links = links,
                                Cover = Cover,
                                ReleaseId = ReleaseId,
                                MiniCover = MiniCover,
                                Genres = Genres,
                                Rating = Rating,
                                VoteRateUpdated = DateTime.Now,
                                Downloads = int.Parse(downloads)
                            };

                            Console.WriteLine("Adding " + release.Name + " " + release.ReleaseId + " : " + pageNumber);
                            db.Releases.Add(release);

                        }
                        catch (Exception e)
                        {
                            string inner = "";
                            if (e.InnerException != null)
                            {
                                inner = e.InnerException.Message;
                            }
                            Console.WriteLine("Crawler error: " + e.Message + " Inner exception: " + inner);

                            continue;
                        }
                    }
                    pageNumber++;
                }
                Console.WriteLine("Saving");
                db.SaveChanges();
                db.Dispose();
                RemoveDuplicates();

                LogStatus("updated");

            }



        }

        private static void RemoveDuplicates()
        {
            DataContext db = new DataContext();

            Console.WriteLine("Removing doubles");

            var releasesToDelete = db.Releases.AsEnumerable().GroupBy(m => m.ReleaseId).SelectMany(grp => grp.Skip(1)).ToList();

            db.Releases.RemoveRange(releasesToDelete);

            db.SaveChanges();
            db.Dispose();

        }

        private static void LogStatus(string status)
        {
            try
            {
                using (var writer = File.CreateText("C:\\Musigger\\Backend\\bin\\date.txt"))
                {
                    writer.WriteLine("DB " + status + " at: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                }
            }
            catch { }
        }

        public static void GetGenres()
        {
            DataContext db = new DataContext();

            using (WebClient webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                var response = webClient.DownloadString("http://freake.ru/?p=1");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                var page = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "block-style")).First();
                List<string> existing = db.Genres.Select(m => m.Name).ToList();
                foreach (var item in page.Descendants("a"))
                {
                    string newGenre = item.InnerText;
                    Genre genre = new Genre(newGenre);

                    if (!existing.Contains(newGenre))
                    {
                        db.Genres.Add(genre);
                    }
                }
                db.SaveChanges();
            }
            return;
        }


    }
}