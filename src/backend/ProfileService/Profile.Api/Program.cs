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
using Profile.Domain.Repositories;
using Profile.Application.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

builder.Services.AddAuthorization(cfg =>
{
    cfg.AddPolicy("NormalUser", p => p.RequireRole("normal", "admin", "recruiter", "hiring_manager"));
});

builder.Services.AddCors(cfg =>
{
    cfg.AddPolicy("ServicesOnly", plc => plc.WithOrigins("https://career.api:8081"));
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
}).AddCookie("Cookies")
.AddOAuth("GitHub", opt =>
{
    
    opt.SignInScheme = "Cookies";
    opt.ClientId = builder.Configuration.GetValue<string>("services:gitHub:clientId")!;
    opt.ClientSecret = builder.Configuration.GetValue<string>("services:gitHub:clientSecret")!;

    opt.CallbackPath = "/oauth/github-cb";

    opt.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    opt.TokenEndpoint = "https://github.com/login/oauth/access_token";
    opt.UserInformationEndpoint = "https://api.github.com/user";
    opt.SaveTokens = true;

    opt.ClaimActions.MapJsonKey("sub", "id");
    opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
    opt.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

    opt.Scope.Add("user:email");

    opt.Events.OnCreatingTicket = async ctx =>
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);
        using var result = await ctx.Backchannel.SendAsync(request);
        var user = await result.Content.ReadFromJsonAsync<JsonElement>();
        if(user.TryGetProperty("email", out var value) && string.IsNullOrEmpty(value.GetString()))
        {
            var resultToString = await result.Content.ReadAsStringAsync();
            var deserialize = JsonConvert.DeserializeObject(resultToString) as JObject;

            using var requestEmail = new HttpRequestMessage(HttpMethod.Get, $"{ctx.Options.UserInformationEndpoint}/emails");
            requestEmail.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);
            using var emailResult = await ctx.Backchannel.SendAsync(requestEmail);
            var resultAsJson = await emailResult.Content.ReadFromJsonAsync<JsonElement>();

            var firstEmail = resultAsJson
            .EnumerateArray()
            .FirstOrDefault(d => d.GetProperty("primary").GetBoolean() == true)
            .GetProperty("email").GetString();

            deserialize!.SelectToken("email").Replace(firstEmail);
            var JobjToString = deserialize.ToString();
            user = JsonDocument.Parse(JobjToString).RootElement;
        }

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
    
    var result = await context.AuthenticateAsync("Cookies");
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

app.UseRateLimiter();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { IPAddress.Parse("172.25.0.1") }
});

app.UseCors();

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
