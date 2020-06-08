using ReleaseCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReleaseCrowler.Models
{
    public class TopReleaseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Cover { get; set; }

        public TopReleaseItem(Release release)
        {
            Id = release.Id;
            Name = release.Name;
            Label = release.Label;
            Cover = release.Cover;
        }
        public TopReleaseItem()
        {

        }
    }
}