using Microsoft.EntityFrameworkCore;
using ReleaseCrawler;
using ReleaseCrawler.Models;
using ReleaseCrowler.Models;
using System;
using System.Collections;
using System.Collections.Generic;
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
            var dayAgo = DateTime.UtcNow.AddDays(-1);

            var top = db.TopReleases
                .Include(m=>m.Release)
                .Where(m => m.Updated > dayAgo)
                .Where(m => m.Weeks == weeks && m.Count == count)
                .OrderByDescending(m => m.Goodness)
                .AsQueryable();

            if (!top.Any() || top.Count() != count)
            {
                var topToDelete = db.TopReleases
                    .Where(m => m.Weeks == weeks && m.Count == count)
                    .ToList();

                db.TopReleases.RemoveRange(topToDelete);

                var startDate = DateTime.UtcNow.AddDays(-(weeks+1) * 7);
                var releasesToRate = db.Releases.Where(m => m.Date > startDate).ToList();

                var maxVotes = releasesToRate.Max(m => m.Votes);

                foreach (var release in releasesToRate)
                    release.Votes = 100 * release.Votes / maxVotes;

                top = releasesToRate
                    .Select(s => new TopRelease(weeks, count, s))
                    .OrderByDescending(m=>m.Goodness)
                    .Take(count).AsQueryable();

                db.TopReleases.AddRange(top);
                db.SaveChanges();
            }

            var result = top.ToList()
                .Select(s => new ReleaseDetails(s.Release));

            return Request.CreateResponse(HttpStatusCode.OK, result, MediaTypeHeaderValue.Parse("application/json"));
        }
    }
}
