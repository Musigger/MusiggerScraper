using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseCrawler
{
    public class Release
    {
        [Key]
        public int Id { get; set; }

        public int ReleaseId { get; set; }

        public string Name { get; set; }

        public ReleaseType ReleaseType { get; set; }

        public int VoteCount { get; set; }

        public string Genres { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string MiniCover { get; set; }
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
