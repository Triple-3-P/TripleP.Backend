using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using TripleP.Backend.Database.Context;
using TripleP.Backend.Database.Identity;
using TripleP.Backend.Web.Infrastructure.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.secrets.json", optional: true);

builder.Configuration
    .AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddTriplePLogic();
builder.Services.AddTriplePDatabase(builder.Configuration);

// Registers Identity together with the token/cookie authentication schemes the
// standard Identity API endpoints (MapIdentityApi) need.
builder.Services
    .AddIdentityApiEndpoints<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TriplePDbContext>();

// UseAuthorization in the pipeline requires these services; Identity does not register them.
builder.Services.AddAuthorization();

builder.Services.AddTransient<IEmailSender<ApplicationUser>, NoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTriplePEndpoints();

// Standard ASP.NET Identity endpoints (register, login, refresh, email confirmation,
// password reset, 2FA and account info) under the same /api prefix as the rest of the API.
app.MapGroup("api")
    .MapIdentityApi<ApplicationUser>()
    .WithTags("Identity");

app.Run();