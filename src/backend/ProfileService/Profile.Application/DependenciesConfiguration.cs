using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profile.Application.ApplicationServices;
using Profile.Application.Services;
using Sqids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profile.Application
{
    public static class DependenciesConfiguration
    {
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            AddSqids(services, configuration);
            AddMapper(services);
            AddServices(services);
        }

        static void AddMapper(IServiceCollection services)
        {
            services.AddScoped(mapper =>
            {
                var sqids = mapper.GetRequiredService<SqidsEncoder<long>>();
                var config = new MapperConfiguration(d => d.AddProfile(new AutoMapperConfiguration(sqids)));

                return config.CreateMapper();
            });
        }

        static void AddServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IConnectionService, ConnectionService>();
        }

        static void AddSqids(IServiceCollection services, IConfiguration configuration)
        {
            var alphabet = configuration.GetValue<string>("services:sqids:alphabet");
            var minLength = configuration.GetValue<int>("services:sqids:minLength");
            services.AddSingleton<SqidsEncoder<long>>(d => new SqidsEncoder<long>(new SqidsOptions()
            {
                Alphabet = alphabet!,
                MinLength = minLength
            }));
        }
    }
}
