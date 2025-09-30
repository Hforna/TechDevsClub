using Career.Api.Hubs;
using Career.Application;
using Career.Domain;
using Career.Domain.Dtos;
using Career.Domain.Services;
using Career.Infrastructure;
using Career.Infrastructure.Messaging.Rabbitmq;
using Career.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text;

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
    d.AddPolicy("ManageJobs", d => d.RequireRole("staff"));
});

var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("services:jwt:signKey")!)),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = false,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt => {
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = tokenValidationParams;
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

//app.MapPrometheusScrapingEndpoint();

app.MapHub<NotificationHub>("/hubs/notification");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
