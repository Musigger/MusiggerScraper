using System;
using System.Net;
using System.Text;

namespace ReleaseUpdater
{
    class Program
    {
        static void Main(string[] args)
        {

            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            for (int i = 105324; i > 95000; i--)
            {
                Console.WriteLine(i);
                try
                {
                    var releaseResponse = webClient.DownloadString("http://www.musigger.com/api/releases?ID=" + i + "&update=true");
                    Console.WriteLine(releaseResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }


            Console.WriteLine("Complete");
        }
    }
}
