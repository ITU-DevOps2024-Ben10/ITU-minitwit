using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Minitwit;
using Minitwit.Core.Entities;
using Minitwit.Core.Repository;
using Minitwit.Infrastructure;
using Minitwit.Infrastructure.Repository;
using ITU_minitwit.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerSideBlazor();

string currentDirectory = Directory.GetCurrentDirectory();
string dbPath;

if (Directory.Exists(Path.Combine(currentDirectory, "..", "Minitwit.Infrastructure", "data")))
{
    dbPath = Path.Combine(currentDirectory, "..", "Minitwit.Infrastructure", "data", "Minitwit.db"); //Build directory
}
else 
{
    dbPath = Path.Combine(currentDirectory, "data", "Minitwit.db"); //Publish directory
}

builder.Services.AddRazorComponents();

//Github OAuth:
builder.Services.AddAuthentication()
    .AddCookie();

builder.Services.AddDbContext<MinitwitDbContext>(options => 
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddIdentity<Author, IdentityRole<Guid>>()
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
		options.Cookie.Name = ".Chirp.Session";
    	options.IdleTimeout = TimeSpan.FromMinutes(10);
    	options.Cookie.HttpOnly = false;
    	options.Cookie.IsEssential = true;
	});

builder.Services.AddDistributedMemoryCache();

var app = builder.Build();


// Get a scope to manage the liftime of the context
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

app.MapRazorComponents<App>();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.Run();