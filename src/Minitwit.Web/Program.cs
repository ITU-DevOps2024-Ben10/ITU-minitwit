using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Web;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure.Repository;
using Prometheus;

/// <summary>
/// This file is the entry point of the application. 
/// It is responsible for setting up the application and starting it.
/// </summary>


// using var meterProvider = Sdk.CreateMeterProviderBuilder().AddMeter("myMeter").AddPrometheusExporter().Build();


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

ProgramOptions.AddProgramOptions(builder);
ProgramOptions.AddIdendity(builder);
ProgramOptions.AddDatabase(builder);

//API Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.IgnoreNullValues = true;
});


// TODO put do this in ProgramOptions
//Client that prometheus uses to report metric
//Src: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.AspNetCore/README.md

// Dependency Injection
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IValidator<CreateCheep>, CheepCreateValidator>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<ICheepService, MinitwitService>();
builder.Services.AddScoped<IReactionRepository, ReactionRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();

builder.Services.AddSession(
    options =>
    {
        options.Cookie.Name = ".Minitwit.Web.Session";
        options.IdleTimeout = TimeSpan.FromMinutes(10);
        options.Cookie.HttpOnly = false;
        options.Cookie.IsEssential = true;
    });

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    MinitwitDbContext context = services.GetRequiredService<MinitwitDbContext>();
    
    try
    {
        context.Database.Migrate();
    } 
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

/* ----------- Middleware ----------- */

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMetricServer(); // Prometheus metrics endpoint
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllers();
app.MapRazorPages();


app.Run();