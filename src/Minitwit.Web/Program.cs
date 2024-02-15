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


builder.Services.AddDbContext<MinitwitDbContext>(options => 
    options.UseSqlite($"Data Source={dbPath}"));

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

    // Call the method to remove duplicate user Logins
    await context.RemoveDuplicateUserLogins();

    // Call the method to seed the database
    try {
        DbInitializer.SeedDatabase(context);
    } catch (Exception ex) {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
    
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
app.MapRazorPages();
app.Run();