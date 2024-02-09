using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Minitwit.Infrastructure;

namespace Test_Utilities;

public class SqliteInMemoryBuilder
{   
    //This class is used to create an in memory database for testing purposes
    //Create a new instance of this class in your test class,
    //then call the GetContext() method to get a new instance of the ChirpDbContext

    public static MinitwitDbContext GetContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        var builder = new DbContextOptionsBuilder<MinitwitDbContext>().UseSqlite(connection);
        var context = new MinitwitDbContext(builder.Options);
        context.Database.EnsureCreated();
        
        return context;
    }
	
    
}