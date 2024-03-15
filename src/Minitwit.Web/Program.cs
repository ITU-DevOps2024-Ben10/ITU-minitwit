using Minitwit.Core.Entities;
using Minitwit.Infrastructure;
using Minitwit.Web;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure.Repository;

/// <summary>
/// This file is the entry point of the application. 
/// It is responsible for setting up the application and starting it.
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// Set the environment
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

// Set up the database path
if (environmentName.Equals("Development"))
{
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
    builder.Services.AddDbContext<MinitwitDbContext>(options =>
    {
        options.UseSqlite($"Data Source={dbPath}");
    });
}
else
{
    string username = Environment.GetEnvironmentVariable("MYSQL_USERNAME");
    string password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
    string host = Environment.GetEnvironmentVariable("MYSQL_HOST");
    string port = Environment.GetEnvironmentVariable("MYSQL_PORT");
    string database = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
    string sslmode = Environment.GetEnvironmentVariable("MYSQL_SSL_MODE");
    
    var connectionString = $"Server={host};Port={port};Database={database};User={username};Password={password};SslMode={sslmode}";
    
    builder.Services.AddDbContext<MinitwitDbContext>(options =>
    {
        options.UseMySQL(connectionString);
    });

    Console.Write($"Connection string: server={host}; port={port}; database={database}; user={username}; password={password}; sslmode={sslmode}");
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


builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.AllowedUserNameCharacters = "zxcvbnmasdfghjklæøqwertyuiopåZXCVBNMASDFGHJKLÆØQWERTYUIOPÅ1234567890 @";
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
    
    // Apply any pending migrations
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllers();
app.MapRazorPages();
app.Run();