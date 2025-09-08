using Career.Infrastructure;
using Career.Application;
using Career.Domain;
using Career.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Career.Api.Hubs;
using Career.Domain.Services;
using Microsoft.AspNetCore.SignalR;
using Career.Domain.Dtos;
using Career.Infrastructure.Messaging.Rabbitmq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRouting(d => d.LowercaseUrls = true);

builder.Services.Configure<SmptSettings>(builder.Configuration.GetSection("services:SmtpSettings"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddDomain();

builder.Services.AddScoped<IRealTimeNotifier, NotificationHubService>();

builder.Services.AddAuthorization(d =>
{
    d.AddPolicy("OnlyOwner", d => d.RequireRole("owner"));
    d.AddPolicy("ManageJobs", d => d.RequireRole("owner"));
});

builder.Services.Configure<BaseRabbitMqConnectionDto>(builder.Configuration.GetSection("rabbitMq"));

builder.Services.AddSingleton<IUserIdProvider, CustomUserProvider>();

builder.Services.AddSignalR();

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("profile.api", cfg =>
{
    cfg.BaseAddress = new Uri("https://profile.api:8081");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<NotificationHub>("/hub/notification");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
