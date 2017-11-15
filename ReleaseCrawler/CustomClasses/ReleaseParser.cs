using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ReleaseCrawler.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace ReleaseCrawler.CustomClasses
{
    public static class ReleaseParser
    {
        public static void Run()
        {
            using (WebClient webClient = new WebClient())
            {
                DataContext db = new DataContext();
                webClient.Encoding = Encoding.UTF8;
                int pageNumber = 30;
                while (pageNumber < 400)
                {
                    var response = webClient.DownloadString("http://freake.ru/?p=" + pageNumber);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(response);
                    //Getting the page
                    var page = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "music-small"));

                    //Retrieveing existing releases to exclude duplicates
                    var existing = db.Releases;

                    int doubles = 0;
                    
                    //iterating through page
                    foreach (var item in page)
                    {
                        var msimage = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-image").First();
                        var msinfo = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-info").First();
                        
                        var elps = msinfo.Descendants("h3").First();                                                                        //Release name
                        var a = msimage.Descendants("a").First();
                        
                        var rateclass = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-rate")).First();
                        //var infoClass = rateclass.Descendants("div").Where(m => m.Attributes["class"].Value == "info").First();             //Vote count

                        var table = msinfo.Descendants("table").First();
                        var date = table.Descendants("tr").First().Descendants("a").First().InnerText;                                      //Release date
                        var type = table.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;                              //Release type
                        
                        //Parsing release ID
                        string releaseId = a.Attributes["href"].Value.Remove(0, 1);
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

                        var label = tablerel.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;                          //лейбл

                        if ((ReleaseType)Enum.Parse(typeof(ReleaseType), type) == ReleaseType.Radioshow)
                        {
                            label = "";
                        }
                        var info = releasePage.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("unreset")).First().InnerHtml;//инфо (треклист, прослушка, итц)
                        var Cover = releasePage.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "fancybox")).First().Attributes["href"].Value;//обложка

                        //Getting hidden links with POST
                        var res = HttpInvoker.Post("http://freake.ru/engine/modules/ajax/music.link.php", new NameValueCollection() {
                            { "id", releaseId }
                        });
                        var str = Encoding.Default.GetString(res);
                        JObject json = JObject.Parse(str);
                        string links = "";
                        if (json["answer"].ToString() == "ok")
                        {
                            links = json["link"].ToString().Replace("Ссылки на скачивание", "Download links");
                        }
                        ////////////////////////////////////////////////////////////////

                        //Creating a release to insert
                        Release release = new Release
                        {
                            Name = elps.Descendants("a").First().InnerText,
                            Votes = int.Parse(vote),
                            Date = DateTime.ParseExact(date, "dd.MM.yyyy", null),
                            Type = (ReleaseType)Enum.Parse(typeof(ReleaseType), type),
                            Artists = artists,
                            Label = label,
                            Info = info,
                            Links = links,
                            Cover = Cover,
                            ReleaseId = int.Parse(releaseId),
                            MiniCover = a.Descendants("img").First().Attributes["src"].Value,
                            Genres = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-style")).First().InnerText,
                            Rating = decimal.Parse(rate),
                            VoteRateUpdated = DateTime.Now
                        };

                        Console.WriteLine(release.Name + " " + release.ReleaseId + " : " + pageNumber + Environment.NewLine);

                        bool releaseExists = existing.Select(m => m.ReleaseId).ToList().Contains(release.ReleaseId);

                        //if there's no releases with this ID adding as new
                        if (existing == null || existing.Count() == 0 || !releaseExists)
                        {
                            db.Releases.Add(release); 
                        }
                        else if(releaseExists)
                        {
                            doubles++;
                            //if there were less than 10 duplicates. (one or two may be added to the source during running)
                            if (doubles <= 10)
                            {
                                continue;
                            }
                            else //if threr are more duplicates just saving changes and return
                            {
                                db.SaveChanges();
                                return;
                            }
                        }
                    }
                    pageNumber++;
                }
                db.SaveChanges();
            }
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