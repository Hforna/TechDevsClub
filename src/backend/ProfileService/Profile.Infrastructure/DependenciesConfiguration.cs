using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profile.Domain.Aggregates;
using Profile.Domain.Entities;
using Profile.Infrastructure.Data;
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
        }

        static void AddData(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("sqlserver");
            services.AddDbContext<DataContext>(cfg => cfg.UseSqlServer(connectionString));

            services.AddIdentityCore<User>(d =>
            {
                d.Password.RequiredLength = 8;
                d.User.RequireUniqueEmail = true;
                d.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<DataContext>();
        }
    }
}
