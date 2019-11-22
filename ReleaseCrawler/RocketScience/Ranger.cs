using ReleaseCrawler;
using System;

namespace ReleaseCrowler.RocketScience
{
    public static class Ranger
    {
        public static decimal GetReleaseGoodness(Release release)
        {
            var voteGoodness = (decimal)GoodnessByVotes(release);
            var ratingGoodness = (decimal)GoodnessByRating(release);

            return (voteGoodness + ratingGoodness) / 2;
        }

        private static double GoodnessByVotes(Release release)
        {
            var x = release.Votes;

            var e = Math.E;

            var y = 1.06777 - 1.314133 * Math.Pow(e, -0.03345669 * x);

            return Normalize(y);
        }

        private static double GoodnessByRating(Release release)
        {
            double x = (double)release.Rating;

            var y = -637.6923 + 838.6223 * x
                - 436.6759 * x * x
                + 112.471 * Math.Pow(x, 3)
                - 14.32438 * Math.Pow(x, 4)
                + 0.7220866 * Math.Pow(x, 5);

            return Normalize(y);
        }

        private static double Normalize(double coef)
        {
            if (coef > 1) return 1;
            if (coef < 0) return 0;
            return coef;
        }
    }
}