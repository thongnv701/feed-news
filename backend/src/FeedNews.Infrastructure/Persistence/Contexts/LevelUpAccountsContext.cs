using FeedNews.Domain.Entities;
using FeedNews.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FeedNews.Infrastructure.Persistence.Contexts;

public class FeedNewsContext : DbContext
{
    public FeedNewsContext(DbContextOptions<FeedNewsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<News> NewsFeeds { get; set; }

    public async Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        foreach (EntityEntry entry in ChangeTracker.Entries()
                     .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            entry.Property("UpdatedDate").CurrentValue = now;
            if (entry.State == EntityState.Modified) entry.Property("CreatedDate").IsModified = false;

            if (entry.State == EntityState.Added) entry.Property("CreatedDate").CurrentValue = now;
        }

        int numberChange = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        ChangeTracker.Clear();
        return numberChange;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure News entity
        modelBuilder.ApplyConfiguration(new NewsEFConfiguration());

        // Account - Role relationship
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .HasConstraintName("fk_account_role");
    }
}