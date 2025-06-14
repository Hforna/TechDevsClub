using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Profile.Api.BackgroundJobs;
using Profile.Api.Binders;
using Profile.Api.Endpoints;
using Profile.Api.Filters;
using Profile.Api.Middlewares;
using Profile.Application;
using Profile.Domain.Services.Security;
using Profile.Infrastructure;
using Profile.Infrastructure.Services;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<CountGithubCommitsBackground>();

builder.Services.Configure<SmptSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddScoped<IRequestToken, RequestToken>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("PerfilPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2; 
    });
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<BinderId>();

builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication();

builder.Services.AddTransient<CultureInfoMiddleware>();

builder.Services.AddExceptionHandler<ExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseMiddleware<CultureInfoMiddleware>();

//app.Use(async (context, next) =>
//{
//    Console.WriteLine($"Session available: {context.Session?.IsAvailable ?? false}");
//    await next();
//});
//
app.Use(async (context, next) =>
{
    await context.Session.LoadAsync();
    if (string.IsNullOrEmpty(context.Session.Id))
    {
        context.Session.SetString("__init__", "1");
        await context.Session.CommitAsync();
    }
    await next();
});

app.UseRateLimiter();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
    ForwardedHeaders.XForwardedProto
});

app.UseAuthorization();

app.UseExceptionHandler(d => { });

app.MapUserEndpoints();
app.MapProfileEndpoint();
app.MapLoginEndpoints();
app.MapConnectionEndpoints();

app.Run();

public partial class Program { }
