using Career.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Application
{
    public static class DependenciesConfiguration
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            AddServices(services);
            AddAutoMapper(services);
        }

        static void AddServices(IServiceCollection services)
        {
            services.AddScoped<ICompanyService, CompanyService>();
        }

        static void AddAutoMapper(IServiceCollection services)
        {
            services.AddAutoMapper(d => d.AddProfile(new MapperConfiguration()));
        }
    }
}
