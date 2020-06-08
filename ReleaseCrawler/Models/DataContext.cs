using Microsoft.EntityFrameworkCore;
using ReleaseCrowler.Models;

namespace ReleaseCrawler.Models
{
    public class DataContext : DbContext
    {
        //public DataContext()
        //    : base() { }

        public DbSet<Release> Releases { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<TopRelease> TopReleases { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("******************");
        }
    }
}