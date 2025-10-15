using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Clients;
using Career.Domain.Services.Messaging;
using Career.Infrastructure.Messaging.Rabbitmq.Consumers;
using Career.Infrastructure.Messaging.Rabbitmq.Producers;
using Career.Infrastructure.Persistence;
using Career.Infrastructure.Services;
using Career.Infrastructure.Services.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
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
            AddServices(services, configuration);
            AddStorage(services, configuration);
            WebSocketsConnection(services);
            AddMessagingService(services);
        }

        static void AddDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("sqlserver");

            services.AddDbContext<DataContext>(opt => opt.UseSqlServer(connectionString));
        }

        static void AddMessagingService(IServiceCollection services)
        {
            services.AddScoped<IStaffServiceProducer, StaffServiceProducer>();
            services.AddScoped<IJobServiceProducer, JobServiceProducer>();

            services.AddHostedService<AnalyzeJobProducer>();
            services.AddHostedService<UsersMatchedConsumer>();
        }

        static void WebSocketsConnection(IServiceCollection services)
        {
            services.AddSignalRCore();
        }

        static void AddRepositories(IServiceCollection services)
        {
            services.AddScoped<IGenericRepository, GenericRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IProfileServiceClient, ProfileServiceClient>();

            services.AddScoped<IRequestService, RequestService>();

            var sendGridKey = configuration.GetValue<string>("services:twilio:sendGridKey");
            services.AddSingleton<ISendGridClient, SendGridClient>(d => new SendGridClient(sendGridKey));

            services.AddSingleton<IEmailService, EmailService>();

            services.Configure<SmptSettings>(configuration.GetSection("services:smtpSettings"));
        }

        static void AddStorage(IServiceCollection services, IConfiguration configuration)
        {
            //services.AddSingleton<IStorageService>(d =>
            //new AzureStorageService(new Azure.Storage.Blobs.BlobServiceClient(configuration.GetValue<string>("services:azure:blobClient"))));
        }
    }
}
