using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Crypto.Engines;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using Profile.Domain.Services;
using Profile.Domain.Services.External;
using Profile.Domain.Services.Security;
using Profile.Infrastructure.Data;
using Profile.Infrastructure.Repositories;
using Profile.Infrastructure.Services;
using Profile.Infrastructure.Services.External;
using Profile.Infrastructure.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Infrastructure
{
    public static class DependenciesConfiguration
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddData(services, configuration);
            AddRepositories(services);
            AddSecurity(services, configuration);
            AddServices(services);
        }

        static void AddData(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("sqlserver");
            services.AddDbContext<DataContext>(cfg => cfg.UseSqlServer(connectionString));

            services.AddDataProtection();

            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<User>>();
        }

        static void AddSecurity(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPasswordEncrypt, BCryptService>();

            services.AddScoped<ITokenService, TokenService>(d => new TokenService(
                int.Parse(configuration.GetSection("services:security:jwt:expiresOn").Value!), 
                configuration.GetSection("services:security:jwt:signKey").Value!, 
                int.Parse(configuration.GetSection("services:security:jwt:refreshTokenExpiration").Value!)));
        }

        static void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IEmailService, EmailService>();

            services.AddScoped<IGitHubService, GitHubService>();
        }

        static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<ISkillRespository, SkillRepository>();
            services.AddScoped<IConnectionRepository, ConnectionRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
