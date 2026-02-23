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
    public virtual DbSet<KnowledgeEntry> KnowledgeEntries { get; set; }
    public virtual DbSet<AnalysisQuestion> AnalysisQuestions { get; set; }
    public virtual DbSet<ArticleAnalysisResult> ArticleAnalysisResults { get; set; }
    public virtual DbSet<KnowledgeDispute> KnowledgeDisputes { get; set; }

    public async Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        foreach (EntityEntry entry in ChangeTracker.Entries()
                     .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
        {
            var now = DateTime.UtcNow;
            
            // Set UpdatedAt timestamp - check which property name exists
            if (entry.Metadata.FindProperty("UpdatedAt") != null)
            {
                entry.Property("UpdatedAt").CurrentValue = now;
            }
            else if (entry.Metadata.FindProperty("UpdatedDate") != null)
            {
                entry.Property("UpdatedDate").CurrentValue = now;
            }
            
            // Handle CreatedAt/CreatedDate
            if (entry.State == EntityState.Modified)
            {
                // Don't modify creation timestamp on updates
                if (entry.Metadata.FindProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").IsModified = false;
                }
                else if (entry.Metadata.FindProperty("CreatedDate") != null)
                {
                    entry.Property("CreatedDate").IsModified = false;
                }
            }

            if (entry.State == EntityState.Added)
            {
                // Set creation timestamp on new entries
                if (entry.Metadata.FindProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                }
                else if (entry.Metadata.FindProperty("CreatedDate") != null)
                {
                    entry.Property("CreatedDate").CurrentValue = now;
                }
            }
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

        // Configure KnowledgeEntry entity
        modelBuilder.Entity<KnowledgeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.SourceUrl).HasMaxLength(500);
            entity.Property(e => e.ConfidenceScore).HasPrecision(3, 2);
            entity.Property(e => e.Tags).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure AnalysisQuestion entity
        modelBuilder.Entity<AnalysisQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Question).IsRequired();
            entity.Property(e => e.Purpose);
            entity.Property(e => e.Priority).IsRequired().HasDefaultValue(2);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Category);
        });

        // Configure ArticleAnalysisResult entity
        modelBuilder.Entity<ArticleAnalysisResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.NewsId).IsRequired();
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OriginalSummary).IsRequired();
            entity.Property(e => e.EnhancedAnalysis);
            entity.Property(e => e.ReferencedKnowledge).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(e => e.SourceUrls).HasColumnType("text[]").HasDefaultValue(Array.Empty<string>());
            entity.Property(e => e.ConfidenceLevel).HasMaxLength(20);
            entity.Property(e => e.QuestionsAnswered);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.NewsId);
            entity.HasIndex(e => e.Category);
            // Foreign key to News table
            entity.HasOne<News>()
                .WithMany()
                .HasForeignKey(e => e.NewsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure KnowledgeDispute entity
        modelBuilder.Entity<KnowledgeDispute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.KnowledgeEntryId).IsRequired();
            entity.Property(e => e.ConflictingEntryId).IsRequired();
            entity.Property(e => e.Reason);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ResolvedAt);
            entity.Property(e => e.Resolution);
            entity.HasIndex(e => e.KnowledgeEntryId);
            entity.HasIndex(e => e.ConflictingEntryId);
            // Foreign keys to KnowledgeEntry table
            entity.HasOne<KnowledgeEntry>()
                .WithMany()
                .HasForeignKey(e => e.KnowledgeEntryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<KnowledgeEntry>()
                .WithMany()
                .HasForeignKey(e => e.ConflictingEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Account - Role relationship
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .HasConstraintName("fk_account_role");
    }
}