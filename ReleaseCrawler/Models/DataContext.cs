using ReleaseCrawler;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ReleaseCrawler.Models
{
    public class DataContext : DbContext
    {
        public DataContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Release> Releases { get; set; }

        public DbSet<Genre> Genres { get; set; }

    }
}