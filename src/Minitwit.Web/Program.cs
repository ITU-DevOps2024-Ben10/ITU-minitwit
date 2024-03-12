using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Web;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure.Repository;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Prometheus;

/// <summary>
/// This file is the entry point of the application. 
/// It is responsible for setting up the application and starting it.
/// </summary>


// using var meterProvider = Sdk.CreateMeterProviderBuilder().AddMeter("myMeter").AddPrometheusExporter().Build();


var builder = WebApplication.CreateBuilder(args);

string currentDirectory = Directory.GetCurrentDirectory();
string dbPath;

if (Directory.Exists(Path.Combine(currentDirectory, "..", "Minitwit.Infrastructure", "data")))
{
    dbPath = Path.Combine(currentDirectory, "..", "Minitwit.Infrastructure", "data", "MinitwitDBContext.db"); //Build directory
}
else 
{
    dbPath = Path.Combine(currentDirectory, "data", "MinitwitDBContext.db"); //Publish directory
}

// Add services to the container.
builder.Services.AddRazorPages();

//API Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.IgnoreNullValues = true;
});

//Client that prometheus uses to report metric
//Src: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Prometheus.AspNetCore/README.md
builder.Services.AddOpenTelemetry()
    .WithMetrics(providerBuilder =>
    {
        providerBuilder.AddPrometheusExporter();

        providerBuilder.AddMeter("Dotnet.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
    });
        


string database = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
string databasePassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD");

builder.Services.AddDbContext<MinitwitDbContext>(options =>
{
    var connectionString = $"server=minitwit_database;port=3306;database={database};user=root;password={databasePassword};";
    options.UseMySQL(connectionString);
});


builder.Services.AddDefaultIdentity<Author>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<MinitwitDbContext>();

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

//Github OAuth:
builder.Services.AddAuthentication()
    .AddCookie();

var app = builder.Build();

// Get a scope to manage the lifetime of the context
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Get an instance of the DbContext
    var context = services.GetRequiredService<MinitwitDbContext>();
    
    try
    {
        context.Database.Migrate();
    } 
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
    
    // Call the method to remove duplicate user Logins
    //await context.RemoveDuplicateUserLogins();
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

app.UseOpenTelemetryPrometheusScrapingEndpoint();


app.Run();