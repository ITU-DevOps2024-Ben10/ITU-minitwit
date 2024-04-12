using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;
using Minitwit.Infrastructure;

namespace Minitwit.Web;

public class ProgramOptions
{
    public static void AddProgramOptions(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.All;
            logging.MediaTypeOptions.AddText("application/javascript");
            logging.RequestBodyLogLimit = 0;
            logging.ResponseBodyLogLimit = 0;
            logging.CombineLogs = true;
        });
    }

    public static void AddIdendity(WebApplicationBuilder builder)
    {
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 1;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.AllowedUserNameCharacters = "zxcvbnmasdfghjklqwertyuiopZXCVBNMASDFGHJKLQWERTYUIOP1234567890 @";
        });
        
        builder.Services.AddDefaultIdentity<Author>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<MinitwitDbContext>();
        
        builder.Services.AddAuthentication()
            .AddCookie();
    }

    public static void AddDatabase(WebApplicationBuilder builder)
    {
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

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
            var connectionstringList = ConnectionStringList();
    
            var connectionString = $"Server={connectionstringList[0]};Port={connectionstringList[1]};" +
                                   $"Database={connectionstringList[2]};User={connectionstringList[3]};" +
                                   $"Password={connectionstringList[4]};SslMode={connectionstringList[5]}";
    
            builder.Services.AddDbContext<MinitwitDbContext>(options => {
                options.UseMySQL(connectionString);
            });

            Console.Write("Connection string: " + connectionString);
        }
    }
    

    public static List<String> ConnectionStringList()
    {
        var outputList = new List<String>();
        outputList.Add(Environment.GetEnvironmentVariable("MYSQL_HOST")); //host
        outputList.Add(Environment.GetEnvironmentVariable("MYSQL_PORT")); //port
        outputList.Add(Environment.GetEnvironmentVariable("MYSQL_DATABASE")); //database
        outputList.Add(Environment.GetEnvironmentVariable("MYSQL_USERNAME")); //username
        outputList.Add(Environment.GetEnvironmentVariable("MYSQL_PASSWORD")); //password
        outputList.Add( Environment.GetEnvironmentVariable("MYSQL_SSL_MODE")); //sslmode
        return outputList;
    }
}