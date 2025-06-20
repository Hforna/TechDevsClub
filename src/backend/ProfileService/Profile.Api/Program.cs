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
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

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
builder.Services.AddTransient<DeviceTrackerMiddleware>();

builder.Services.AddAuthentication(cfg =>
{
    cfg.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    cfg.DefaultChallengeScheme = "GitHub";
}).AddCookie("cookie")
.AddOAuth("GitHub", opt =>
{
    opt.SignInScheme = "cookie";
    opt.ClientId = builder.Configuration.GetValue<string>("services:gitHub:clientId")!;
    opt.ClientSecret = builder.Configuration.GetValue<string>("services:gitHub:clientSecret")!;

    opt.CallbackPath = "/oauth/github-cb";

    opt.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    opt.TokenEndpoint = "https://github.com/login/oauth/access_token";
    opt.UserInformationEndpoint = "https://api.github.com/user";
    opt.SaveTokens = true;

    opt.ClaimActions.MapJsonKey("sub", "id");
    opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");

    opt.Events.OnCreatingTicket = async ctx =>
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);
        using var result = await ctx.Backchannel.SendAsync(request);
        var user = await result.Content.ReadFromJsonAsync<JsonElement>();

        ctx.RunClaimActions(user);
    };
});

builder.Services.AddExceptionHandler<ExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

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
    var result = await context.AuthenticateAsync("cookie");
    if (result.Succeeded)
    {
        Console.WriteLine("User not authenticated by cookie");
        foreach (var claim in result.Principal.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        context.User = result.Principal;
    }
    else
    {
        Console.WriteLine("User not authenticated by cookie");
    }

    await next();
});

app.UseMiddleware<DeviceTrackerMiddleware>();

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

app.MapGet("/", (HttpContext ctx) =>
{
    return ctx.User.Claims.Select(d => new { d.Type, d.Value }).ToList();
});

app.UseRateLimiter();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { IPAddress.Parse("172.25.0.1") }
});

app.UseAuthorization();

app.UseExceptionHandler(d => { });

app.MapUserEndpoints();
app.MapProfileEndpoint();
app.MapLoginEndpoints();
app.MapConnectionEndpoints();
app.MapTokenEndpoints();
app.MapSkillEndpoints();

app.Run();

public partial class Program { }
