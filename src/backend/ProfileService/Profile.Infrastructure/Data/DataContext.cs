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
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Role>().HasData(new Role() { Name = "normal", NormalizedName = "NORMAL", Id = 1 });
            builder.Entity<Role>().HasData(new Role() {  Name = "admin", NormalizedName = "ADMIN", Id = 2 });
            
            builder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
            builder.ApplyConfiguration(new UserSkills.Mapping());
            builder.ApplyConfiguration(new SocialLink.Mapping());
            builder.ApplyConfiguration(new Connection.Mapping());
            builder.ApplyConfiguration(new ProfileEntity.Mapping());

            builder.Entity<Skill>().HasData(PrimarySkills);
        }

        public static readonly List<Skill> PrimarySkills = new()
        {

            new Skill { Id = 1, Name = "C#" },
            new Skill { Id = 2, Name = "Java" },
            new Skill { Id = 3, Name = "Python" },
            new Skill { Id = 4, Name = "JavaScript" },
            new Skill { Id = 5, Name = "TypeScript" },
            new Skill { Id = 6, Name = "Go" },
            new Skill { Id = 7, Name = "Rust" },
            new Skill { Id = 8, Name = "Kotlin" },
            new Skill { Id = 9, Name = "Swift" },
            new Skill { Id = 10, Name = "PHP" },

            new Skill { Id = 11, Name = "React" },
            new Skill { Id = 12, Name = "Angular" },
            new Skill { Id = 13, Name = "Vue.js" },
            new Skill { Id = 14, Name = "Svelte" },
            new Skill { Id = 15, Name = "Blazor" },

            new Skill { Id = 16, Name = ".NET" },
            new Skill { Id = 17, Name = "Spring Boot" },
            new Skill { Id = 18, Name = "Django" },
            new Skill { Id = 19, Name = "Flask" },
            new Skill { Id = 20, Name = "Express.js" },
            new Skill { Id = 21, Name = "Laravel" },

            new Skill { Id = 22, Name = "SQL Server" },
            new Skill { Id = 23, Name = "MySQL" },
            new Skill { Id = 24, Name = "PostgreSQL" },
            new Skill { Id = 25, Name = "MongoDB" },
            new Skill { Id = 26, Name = "Redis" },
            new Skill { Id = 27, Name = "Elasticsearch" },

            new Skill { Id = 28, Name = "Docker" },
            new Skill { Id = 29, Name = "Kubernete" },
            new Skill { Id = 30, Name = "Azure" },
            new Skill { Id = 31, Name = "AWS" },
            new Skill { Id = 32, Name = "Terraform" },
            new Skill { Id = 33, Name = "CI/CD" },

            new Skill { Id = 34, Name = "Scrum" },
            new Skill { Id = 35, Name = "Kanban" },
            new Skill { Id = 36, Name = "DDD" },
            new Skill { Id = 37, Name = "TDD" },
            new Skill { Id = 38, Name = "Clean Architecture" }
        };
    }
}
