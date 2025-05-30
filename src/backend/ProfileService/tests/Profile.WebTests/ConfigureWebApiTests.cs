using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Profile.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Profile.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Profile.WebTests
{
    public class ConfigureWebApiTests : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(cfg =>
            {
                cfg.RemoveAll(typeof(DataContext));

                cfg.AddDbContext<DataContext>(opts =>
                {
                    opts.UseInMemoryDatabase("TestDatabase");
                });
            });

            base.ConfigureWebHost(builder);
        }
    }
}
