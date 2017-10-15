using HtmlAgilityPack;
using ReleaseCrowler.Models;
using System;
using System.Collections.Generic;
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
                while (pageNumber < 250)
                {
                    var response = webClient.DownloadString("http://freake.ru/?p=" + pageNumber);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(response);

                    var page = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "music-small"));

                    List<int> existing = db.Releases.Select(m => m.ReleaseId).ToList();
                    
                    foreach (var item in page)
                    {
                        var msimage = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-image").First();
                        var msinfo = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-info").First();
                        
                        var elps = msinfo.Descendants("h3").First();
                        var a = msimage.Descendants("a").First();
                        
                        var rateclass = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-rate")).First();
                        var infoClass = rateclass.Descendants("div").Where(m => m.Attributes["class"].Value == "info").First();

                        var table = msinfo.Descendants("table").First();
                        var date = table.Descendants("tr").First().Descendants("a").First().InnerText;
                        var type = table.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;

                        Release release = new Release
                        {
                            ReleaseId = int.Parse(a.Attributes["href"].Value.Remove(0, 1)),
                            Name = elps.Descendants("a").First().InnerText,
                            MiniCover = a.Descendants("img").First().Attributes["src"].Value,
                            VoteCount = int.Parse(infoClass.InnerText.Remove(0, 9)),
                            ReleaseDate = DateTime.ParseExact(date, "dd.MM.yyyy", null),
                            Genres = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-style")).First().InnerText,
                            ReleaseType = (ReleaseType)Enum.Parse(typeof(ReleaseType), type)
                        };

                        if (!existing.Contains(release.ReleaseId))
                        {
                            db.Releases.Add(release);
                        }
                        else
                        {
                            db.SaveChanges();
                            return;
                        }

                        Console.WriteLine(release.ReleaseId + " " + release.Name);
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