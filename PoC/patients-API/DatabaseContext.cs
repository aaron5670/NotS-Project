using Microsoft.EntityFrameworkCore;
using patients_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace patients_API
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Medicine> Medicine { get; set; }
        public DbSet<Care> Care { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Medicines)
                .WithOne();

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Care)
                .WithOne();
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
    }
}
