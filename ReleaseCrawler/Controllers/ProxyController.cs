using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace ReleaseCrowler.Controllers
{
    public class ProxyController : ApiController
    {
        public HttpResponseMessage Get(string url)
        {
            using (var client = new WebClient())
            {
                var fullUrl = "http://freake.ru" + url;

                var path = HttpContext.Current.Server.MapPath("~/images") + url.Replace('/', '\\');

                if (!File.Exists(path))
                {
                    var directoryName = Path.GetDirectoryName(path);

                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);

                    client.DownloadFile(fullUrl, path);
                }

                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StreamContent(new FileStream(path, FileMode.Open)); // this file stream will be closed by lower layers of web api for you once the response is completed.
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return response;
            }

        }
    }
}
