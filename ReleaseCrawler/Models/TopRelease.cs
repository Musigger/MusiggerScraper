using ReleaseCrawler;
using ReleaseCrowler.RocketScience;
using System;

namespace ReleaseCrowler.Models
{
    public class TopRelease
    {
        public int Id { get; set; }

        public int Weeks { get; set; }
        public int Count { get; set; }

        public decimal Goodness { get; set; }

        public Release Release { get; set; }

        public DateTime Updated { get; set; }

        public TopRelease() { }

        public TopRelease(int weeks, int count, Release release)
        {
            Weeks = weeks;
            Count = count;
            Goodness = Ranger.GetReleaseGoodness(release);
            Release = release;
            Updated = DateTime.UtcNow;
        }
    }
}