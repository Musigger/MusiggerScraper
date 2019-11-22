using ReleaseCrawler;
using ReleaseCrowler.RocketScience;

namespace ReleaseCrowler.Models
{
    public class TopItem
    {
        public Release Release { get; set; }
        public decimal Goodness { get; set; }

        public TopItem(Release release)
        {
            Release = release;
            Goodness = Ranger.GetReleaseGoodness(release);
        }
    }
}