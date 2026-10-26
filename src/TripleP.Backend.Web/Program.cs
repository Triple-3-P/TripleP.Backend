using Scalar.AspNetCore;
using TripleP.Backend.Database.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
}

builder.Configuration
    // The framework reads host settings such as URLS, HTTP_PORTS and HTTPS_PORTS under
    // their unprefixed names, so ASPNETCORE_* variables are also exposed without the prefix.
    .AddEnvironmentVariables(prefix: "ASPNETCORE_")
    .AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddTriplePLogic();
builder.Services.AddTriplePDatabase(builder.Configuration);

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
app.MapGroup("api").MapIdentityApi<ApplicationUser>();

app.Run();