using Career.Domain.Repositories;
using Career.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Infrastructure
{
    public static class DependenciesConfiguration
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddDbContext(services, configuration);
            AddRepositories(services);
        }

        static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("sqlserver");

            services.AddDbContext<DataContext>(opt => opt.UseSqlServer(connectionString));
        }

        static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<CompanyRepository, CompanyRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
