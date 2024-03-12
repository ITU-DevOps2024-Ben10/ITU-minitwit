using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Minitwit.Core.Entities;

namespace Minitwit.Infrastructure;

/// <summary>
///  EF Core will use the properties of the entities to create and control
///  the database, without having the application directly interact with the database.
/// </summary>

public sealed class MinitwitDbContext : IdentityDbContext<Author, IdentityRole<Guid>, Guid>
{
    public DbSet<Twit> Cheeps {get; set;} = null!;
    
    public DbSet<Follow> Follows { get; set; } = null!;

    public DbSet<Reaction> Reactions { get; set; } = null!;

    public MinitwitDbContext(DbContextOptions<MinitwitDbContext> dbContextOptions) : base(dbContextOptions)
    {
        ChangeTracker.LazyLoadingEnabled = false;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        
        // Author entity
        modelBuilder.Entity<Author>(entity =>
        {
            
            modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(p => new { p.LoginProvider, p.ProviderKey });
            modelBuilder.Entity<IdentityUserLogin<Guid>>().HasIndex(p => new { p.LoginProvider, p.ProviderKey }).IsUnique();
            modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(p => new { p.UserId, p.RoleId });
            modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(p => new { p.UserId, p.LoginProvider, p.Name });
            
            entity.Property(e => e.Id);

        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(f => new { f.FollowingAuthorId, f.FollowedAuthorId });
        });
        
        // Cheep entity
        modelBuilder.Entity<Twit>(entity =>
        {
            entity.HasKey(e => e.CheepId);
            entity.Property(e => e.AuthorId).IsRequired();
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.TimeStamp).IsRequired();

        });

        modelBuilder.Entity<Reaction>().Property(m => m.ReactionType)
            .HasConversion<string>();

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(r => new { r.CheepId, r.AuthorId });

        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(e => e.UserId);
        modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(e => e.RoleId);
        modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(e => e.UserId);
    }

    public async Task RemoveDuplicateUserLogins()
    {
        // Fetch all user logins
        var userLogins = await Set<IdentityUserLogin<Guid>>().ToListAsync();

        // Group by LoginProvider and ProviderKey
        var groupedUserLogins = userLogins.GroupBy(l => new { l.LoginProvider, l.ProviderKey });

        // For each group, keep only one record and mark the others for deletion
        foreach (var group in groupedUserLogins)
        {
            var userLoginsToDelete = group.Skip(1).ToList();
            Set<IdentityUserLogin<Guid>>().RemoveRange(userLoginsToDelete);
        }

        // Save changes to the database
        await SaveChangesAsync();
    }
}