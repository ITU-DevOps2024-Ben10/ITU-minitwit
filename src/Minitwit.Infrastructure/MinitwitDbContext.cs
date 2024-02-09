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
    public DbSet<Cheep> Cheeps {get; set;} = null!;
    
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

             entity.HasMany(a => a.Following)
            .WithOne(f => f.FollowingAuthor)
            .HasForeignKey(f => f.FollowingAuthorId);

            entity.HasMany(a => a.Followers)
            .WithOne(f => f.FollowedAuthor)
            .HasForeignKey(f => f.FollowedAuthorId);

            entity.HasMany(a => a.Cheeps)
                .WithOne(c => c.Author)
                .HasForeignKey(c => c.AuthorId)
                .IsRequired(); // Cascade delete for Cheeps

            entity.HasMany(a => a.Reactions)
                .WithOne(r => r.Author)
                .HasForeignKey(r => r.AuthorId)
                .IsRequired();
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(f => new { f.FollowingAuthorId, f.FollowedAuthorId });

            entity.HasOne(f => f.FollowingAuthor)
            .WithMany(a => a.Following)
            .HasForeignKey(f => f.FollowingAuthorId);

            entity.HasOne(f => f.FollowedAuthor)
            .WithMany(a => a.Followers)
            .HasForeignKey(f => f.FollowedAuthorId);
        });
        
        // Cheep entity
        modelBuilder.Entity<Cheep>(entity =>
        {
            entity.HasKey(e => e.CheepId);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.TimeStamp).IsRequired();

            entity.HasOne(c => c.Author)
                .WithMany(a => a.Cheeps)
                .HasForeignKey(c => c.AuthorId); // Cascade delete for Cheeps

            entity.HasMany(c => c.Reactions)
                .WithOne(r => r.Cheep)
                .HasForeignKey(r => r.CheepId)
                .IsRequired(); // Cascade delete for Reactions
        });

        modelBuilder.Entity<Reaction>().Property(m => m.ReactionType)
            .HasConversion<string>();

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(r => new { r.CheepId, r.AuthorId });

            entity.HasOne(r => r.Author)
                .WithMany(a => a.Reactions)
                .HasForeignKey(r => r.AuthorId)
                .IsRequired(); // Restrict delete for Reactions
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