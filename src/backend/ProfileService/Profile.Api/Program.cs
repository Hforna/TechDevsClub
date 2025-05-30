using Microsoft.Data.SqlClient;
using Profile.Api.Endpoints;
using Profile.Api.Middlewares;
using Profile.Application;
using Profile.Infrastructure;
using Profile.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SmptSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddTransient<CultureInfoMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<CultureInfoMiddleware>();

app.UseAuthorization();

app.MapUserEndpoints();

app.Run();

public partial class Program { }
