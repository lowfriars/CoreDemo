using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CoreDemo.Logging;

namespace CoreDemo.Data
{
    public partial class CoreDemoEvtContext : DbContext
    {
        public CoreDemoEvtContext(DbContextOptions<CoreDemoEvtContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public virtual DbSet<DatabaseLog> Logs { get; set; }
    }
}