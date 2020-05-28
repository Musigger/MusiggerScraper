using System;
using System.ComponentModel.DataAnnotations;

namespace ReleaseCrawler
{
    public class Release
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public int Votes { get; set; }
        public DateTime Date { get; set; }
        public ReleaseType Type { get; set; }
        public string Artists { get; set; }
        public string Label { get; set; }
        public string Info { get; set; }
        public string Links { get; set; }
        public string Cover { get; set; }
        public int ReleaseId { get; set; }
        public string MiniCover { get; set; }
        public string Genres { get; set; }
        public decimal Rating { get; set; }
        public DateTime? VoteRateUpdated { get; set; }
        public int? Downloads { get; set; }
        public int DownloadsFromMusigger { get; set; }
    }

    public enum ReleaseType
    {
        Album,
        Single,
        EP,
        LP,
        Compilation,
        Radioshow
    }

    public class ReleaseItem
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Votes { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string MiniCover { get; set; }
        public string Genres { get; set; }

        public ReleaseItem()
        {

        }
        public ReleaseItem(Release release)
        {
            Id = release.Id;
            Name = release.Name;
            Votes = release.Votes;
            Type = release.Type.ToString();
            Label = release.Label;
            MiniCover = release.MiniCover;
            Genres = release.Genres;
        }
    }

    public class ReleaseDetails
    {
        public string Name { get; set; }
        public int Votes { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Genres { get; set; }

        public string Cover { get; set; }
        public decimal Rating { get; set; }
        public string Info { get; set; }
        public int ReleaseId { get; set; }
        public string Links { get; set; }
        public string Date { get; set; }
        public string Artists { get; set; }

        public ReleaseDetails()
        {

        }
        public ReleaseDetails(Release release)
        {
            Name = release.Name;
            Votes = release.Votes;
            Type = release.Type.ToString();
            Label = release.Label;
            Genres = release.Genres;

            Cover = release.Cover;
            Rating = release.Rating;
            Info = release.Info;
            ReleaseId = release.ReleaseId;
            Links = release.Links;
            Date = release.Date.ToShortDateString();
            Artists = release.Artists;
        }
    }
}
