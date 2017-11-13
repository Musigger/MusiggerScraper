using System.Collections.Specialized;
using System.Net;

namespace ReleaseCrowler.CustomClasses
{
    public static class HttpInvoker
    {
        public static byte[] Post(string uri, NameValueCollection pairs)
        {
            byte[] response = null;
            using (WebClient client = new WebClient())
            {
                response = client.UploadValues(uri, pairs);
            }
            return response;
        }
    }
}