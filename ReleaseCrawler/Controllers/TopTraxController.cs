using ReleaseCrawler.Models;
using ReleaseCrowler.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace ReleaseCrowler.Controllers
{
    public class TopTraxController : ApiController
    {
        private DataContext db = new DataContext();

        public HttpResponseMessage Get(int weeks, int count)
        {
            weeks++;
            var startDate = DateTime.UtcNow.AddDays(-weeks * 7);
            var releasesToRate = db.Releases.Where(m => m.Date > startDate).ToList();

            var maxVotes = releasesToRate.Max(m => m.Votes);

            foreach (var release in releasesToRate)
                release.Votes = 100 * release.Votes / maxVotes;
            
            var top = releasesToRate
                .Select(s => new TopItem(s))
                .OrderByDescending(m => m.Goodness)
                .Take(count);

            var result = top.Select(s => s.Release);

            return Request.CreateResponse(HttpStatusCode.OK, result, MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}
