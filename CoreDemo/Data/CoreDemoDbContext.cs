using System;
using Microsoft.EntityFrameworkCore;


namespace CoreDemo.Data
{
    public partial class CoreDemoDbContext : DbContext
    {
        public CoreDemoDbContext(DbContextOptions<CoreDemoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public virtual DbSet<Composers> Composers { get; set; }
        public virtual DbSet<Works> Works { get; set; }

    }
}