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
using Profile.Domain.Aggregates;
using Microsoft.AspNetCore.Identity;
using Profile.Domain.Entities;
using Profile.Domain.Repositories;
using CommonUtilities.Fakers.Entities;
using Profile.Domain.Services.Security;
using System.Security.Claims;

namespace Profile.WebTests
{
    public class ConfigureWebApiTests : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public DataContext DbContext;
        public string UserEmail;

        public async Task InitializeAsync()
        {
            var scope = this.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            DbContext = db;

            await SeedData();
        }

        async Task SeedData()
        {
            var scope = this.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            await roleManager.CreateAsync(new Role("normal"));
            await roleManager.CreateAsync(new Role("admin"));
        }

        public async Task<User> GetConfirmedUser()
        {
            var user = UserFaker.Build(true, true);

            var scope = this.Services.CreateScope();
            var passEncrypt = scope.ServiceProvider.GetRequiredService<IPasswordEncrypt>();
            var uof = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var userMng = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            user.PasswordHash = passEncrypt.Encrypt(user.PasswordHash);
            user.SecurityStamp = Guid.NewGuid().ToString();

            await uof.GenericRepository.Add<User>(user);
            await uof.Commit();

            await userMng.AddToRoleAsync(user, "normal");

            return user;
        }

        public async Task<HttpClient> GenerateClientWithToken()
        {
            var user = await GetConfirmedUser();

            using var scope = this.Services.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
            var userMng = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roles = await userMng.GetRolesAsync(user);
            var claims = roles.Select(d => { return new Claim(ClaimTypes.Role, d); }).ToList();

            var token = tokenService.GenerateToken(claims, user.UserIdentifier);
            var client = this.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", 
                token);

            return client;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DataContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                var serviceProvider = services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

                services.AddDbContext<DataContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                    options.UseInternalServiceProvider(serviceProvider);
                });
            });

            base.ConfigureWebHost(builder);
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }

        public async Task DeleteTables()
        {
            //var entites = DbContext.Model.GetEntityTypes();
            //foreach(var entity in entites)
            //{
            //    await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM {entity.GetTableName()}");
            //}
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }
    }
}
