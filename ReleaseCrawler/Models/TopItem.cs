using ReleaseCrawler;
using ReleaseCrowler.RocketScience;

namespace ReleaseCrowler.Models
{
    public class TopItem
    {
        public TopReleaseItem Release { get; set; }
        public decimal Goodness { get; set; }

        public TopItem(Release release)
        {
            Release = new TopReleaseItem(release);
            Goodness = Ranger.GetReleaseGoodness(release);
        }
    }
}