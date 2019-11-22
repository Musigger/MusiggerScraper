using ReleaseCrawler;
using ReleaseCrowler.RocketScience;

namespace ReleaseCrowler.Models
{
    public class TopItem
    {
        public ReleaseItem Release { get; set; }
        public decimal Goodness { get; set; }

        public TopItem(Release release)
        {
            Release = new ReleaseItem(release);
            Goodness = Ranger.GetReleaseGoodness(release);
        }
    }
}