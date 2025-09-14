using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Career.Domain;
using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Aggregates.JobRoot;
using Career.Domain.ValueObjects;
using Career.Domain.Entities;

namespace Career.Infrastructure.Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<StaffRole> StaffRoles { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<JobRequirement> JobRequirements { get; set; }
        public DbSet<RequestStaff> RequestsStaffs { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<CompanyConfiguration> CompanyConfigurations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>()
                .HasMany(d => d.Staffs)
                .WithOne(d => d.Company).HasForeignKey(d => d.CompanyId);

            modelBuilder.Entity<Review>().HasOne(d => d.Company)
                .WithMany(d => d.Reviews)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Job>().HasMany(d => d.JobRequirements)
                .WithOne(d => d.Job)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Company>()
                .HasOne(d => d.CompanyConfiguration)
                .WithOne(d => d.Company)
                .HasForeignKey<CompanyConfiguration>(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Owned<Location>();
            modelBuilder.Owned<Salary>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
        }
    }
}
