using ReleaseCrowler.Models;
using System.Data.Entity;

namespace ReleaseCrawler.Models
{
    public class DataContext : DbContext
    {
        public DataContext()
            : base("DefaultConnection") { }

        public DbSet<Release> Releases { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<TopRelease> TopReleases { get; set; }
    }
}