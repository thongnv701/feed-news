using FeedNews.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Attribute = FeedNews.Domain.Entities.Attribute;

namespace FeedNews.Infrastructure.Persistence.Contexts;

public class FeedNewsContext : DbContext
{
    public FeedNewsContext(DbContextOptions<FeedNewsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Favourite> Favourites { get; set; }
    public virtual DbSet<Wallet> Wallets { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }
    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }
    public virtual DbSet<Game> Games { get; set; }
    public virtual DbSet<Attribute> Attributes { get; set; }
    public virtual DbSet<GameAccount> GameAccounts { get; set; }
    public virtual DbSet<GAccountAttribute> GAccountAttributes { get; set; }
    public virtual DbSet<WithdrawRequest> WithdrawRequests { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }

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

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .HasConstraintName("fk_account_role");

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Account)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.AccountId)
            .HasConstraintName("fk_notification_account");

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.GameAccount)
            .WithMany(ga => ga.Favourites)
            .HasForeignKey(f => f.GAccountId)
            .HasConstraintName("fk_favourite_gameaccount");

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Customer)
            .WithMany(c => c.Favourites)
            .HasForeignKey(f => f.CustomerId)
            .HasConstraintName("fk_favourite_customer");

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Wallet)
            .WithMany(w => w.Customers)
            .HasForeignKey(c => c.WalletId)
            .HasConstraintName("fk_customer_wallet");

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.Wallet)
            .WithMany(w => w.WalletTransactions)
            .HasForeignKey(wt => wt.WalletId)
            .HasConstraintName("fk_wallettransaction_wallet");

        modelBuilder.Entity<WithdrawRequest>()
            .HasOne(wr => wr.Wallet)
            .WithMany(w => w.WithdrawRequests)
            .HasForeignKey(wr => wr.WalletId)
            .HasConstraintName("fk_withdrawrequest_wallet");

        modelBuilder.Entity<Attribute>()
            .HasOne(a => a.Game)
            .WithMany(g => g.Attributes)
            .HasForeignKey(a => a.GameId)
            .HasConstraintName("fk_attribute_game");

        modelBuilder.Entity<GameAccount>()
            .HasOne(ga => ga.Game)
            .WithMany(g => g.GameAccounts)
            .HasForeignKey(ga => ga.GameId)
            .HasConstraintName("fk_gameaccount_game");

        modelBuilder.Entity<GAccountAttribute>()
            .HasOne(gaa => gaa.Attribute)
            .WithMany(a => a.GAccountAttributes)
            .HasForeignKey(gaa => gaa.AttributeId)
            .HasConstraintName("fk_gaccountattribute_attribute");

        modelBuilder.Entity<GAccountAttribute>()
            .HasOne(gaa => gaa.GameAccount)
            .WithMany(ga => ga.GAccountAttributes)
            .HasForeignKey(gaa => gaa.GAccountId)
            .HasConstraintName("fk_gaccountattribute_gameaccount");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .HasConstraintName("fk_order_customer");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.GameAccount)
            .WithMany(ga => ga.Orders)
            .HasForeignKey(o => o.GAccountId)
            .HasConstraintName("fk_order_gameaccount");

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .HasConstraintName("fk_payment_order");
    }
}