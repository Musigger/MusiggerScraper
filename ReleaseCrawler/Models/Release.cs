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
}
