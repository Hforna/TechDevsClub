using Career.Domain.DomainServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Career.Domain
{
    public static class DependenciesConfiguration
    {
        public static void AddDomain(this IServiceCollection services)
        {
            AddServices(services);
        }

        static void AddServices(IServiceCollection services)
        {
            services.AddScoped<ICompanyDomainService, CompanyDomainService>();
        }
    }
}
