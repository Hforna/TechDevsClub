using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure.Data
{
    public class DataContext : IdentityDbContext<User, Role, long>
    {
        public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext) { }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkills> UserSkills { get; set; }
        public DbSet<ProfileEntity> Profiles { get; set; }
        public DbSet<SocialLink> SocialLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Role>().HasData(new Role() { Name = "normal", NormalizedName = "NORMAL", Id = 1 });
            builder.Entity<Role>().HasData(new Role() {  Name = "admin", NormalizedName = "ADMIN", Id = 2 });
            
            builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
            builder.ApplyConfiguration(new UserSkills.Mapping());
            builder.ApplyConfiguration(new SocialLink.Mapping());
            builder.ApplyConfiguration(new ProfileEntity.Mapping());
        }
    }
}
