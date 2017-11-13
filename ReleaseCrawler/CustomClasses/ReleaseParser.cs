using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ReleaseCrowler.CustomClasses;
using ReleaseCrowler.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using static System.Net.WebRequestMethods;

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
                //начинаем с 30 страницы, чтоб получать настоявшиеся релизы
                int pageNumber = 50;
                while (pageNumber < 60)
                {
                    var response = webClient.DownloadString("http://freake.ru/?p=" + pageNumber);
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(response);
                    //Получаем страницу
                    var page = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "music-small"));

                    //Получаем из базы список существующих айдишников релизов, для контроля уникальности
                    List<int> existing = db.Releases.Select(m => m.ReleaseId).ToList();
                    
                    //Идем по странице
                    foreach (var item in page)
                    {
                        var msimage = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-image").First();
                        var msinfo = item.Descendants("div").Where(m => m.Attributes["class"].Value == "ms-info").First();
                        
                        var elps = msinfo.Descendants("h3").First();                                                                        //Название релиза
                        var a = msimage.Descendants("a").First();
                        
                        var rateclass = msinfo.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("ms-rate")).First();
                        //var infoClass = rateclass.Descendants("div").Where(m => m.Attributes["class"].Value == "info").First();             //Количество голосов

                        var table = msinfo.Descendants("table").First();
                        var date = table.Descendants("tr").First().Descendants("a").First().InnerText;                                      //Дата релиза
                        var type = table.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;                              //Тип релиза
                        
                        //Берем айди релиза, на который будем заходить
                        string releaseId = a.Attributes["href"].Value.Remove(0, 1);
                        //Получаем страницу релиза
                        var releaseResponse = webClient.DownloadString("http://freake.ru/" + releaseId);
                        HtmlDocument releaseDoc = new HtmlDocument();
                        releaseDoc.LoadHtml(releaseResponse);
                        var releasePage = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "post")).First();

                        string rateId = "rate-r-" + releaseId;
                        var rate = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@id,'{0}')]", rateId)).First().InnerHtml; //рейтинг
                        string voteId = "rate-v-" + releaseId;
                        var vote = releaseDoc.DocumentNode.SelectNodes(string.Format("//*[contains(@id,'{0}')]", voteId)).First().InnerHtml; //голоса

                        string artists = ""; //сюда будем складывать артистов
                        
                        var tablerel = releasePage.Descendants("table").First();    //они лежат в таблице

                        foreach (var artist in tablerel.Descendants("tr").First().Descendants("a"))
                        {
                            artists += artist.InnerHtml;
                            artists += ", ";
                        }
                        artists = artists.Substring(0, artists.Length - 2);                                                                 //артисты

                        var label = tablerel.SelectNodes("tr").Skip(1).First().Descendants("a").First().InnerText;                          //лейбл
                        var info = releasePage.Descendants("div").Where(m => m.Attributes["class"].Value.Contains("unreset")).First().InnerHtml;//инфо (треклист, прослушка, итц)
                        var Cover = releasePage.SelectNodes(string.Format("//*[contains(@class,'{0}')]", "fancybox")).First().Attributes["href"].Value;//обложка

                        //пост запросом получаем хитро спрятанный ссылки на скачивание
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

                        //Формируем модель релиза для сохранения
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
                            Rating = decimal.Parse(rate.Replace('.',','))
                        };

                        Console.WriteLine(release.Name + ": " + pageNumber + Environment.NewLine);

                        //если такого нет, то добавляем
                        if (!existing.Contains(release.ReleaseId))
                        {
                            db.Releases.Add(release);
                        }
                        else
                        {
                            //если такой уже есть, все сохраняем и выходим. Значит достигли ранее загруженных
                            db.SaveChanges();
                            return;
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