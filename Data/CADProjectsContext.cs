using CADProjectsHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CADProjectsHub.Data
{
    public class CADProjectsContext : DbContext
    {
        public CADProjectsContext(DbContextOptions<CADProjectsContext> options) : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<CADModel> CADModels { get; set; }
        public DbSet<CADFile> CADFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().ToTable("Project");
            modelBuilder.Entity<Assignment>().ToTable("Assignment");
            modelBuilder.Entity<CADModel>().ToTable("CADModel");
            modelBuilder.Entity<CADFile>().ToTable("CADFile");
        }
    }
}
