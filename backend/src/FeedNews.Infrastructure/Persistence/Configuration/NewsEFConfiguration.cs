using FeedNews.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeedNews.Infrastructure.Persistence.Configuration;

/// <summary>
/// Entity Framework Core configuration for the News entity.
/// Defines table schema, column mappings, indexes, and constraints.
/// </summary>
public class NewsEFConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        // Table configuration
        builder.ToTable("news_feeds");

        // Primary key
        builder.HasKey(n => n.Id)
            .HasName("pk_news_feeds_id");

        // Property configurations
        builder.Property(n => n.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasColumnType("varchar(500)")
            .IsRequired();

        builder.Property(n => n.Url)
            .HasColumnName("url")
            .HasColumnType("varchar(2000)")
            .IsRequired();

        builder.Property(n => n.Summary)
            .HasColumnName("summary")
            .HasColumnType("varchar(2000)")
            .IsRequired(false);

        builder.Property(n => n.Source)
            .HasColumnName("source")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(n => n.Category)
            .HasColumnName("category")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(n => n.PublishedDate)
            .HasColumnName("published_date")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(n => n.FetchedAt)
            .HasColumnName("fetched_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(n => n.RankingScore)
            .HasColumnName("ranking_score")
            .HasColumnType("numeric(10,2)")
            .HasDefaultValue(0m);

        builder.Property(n => n.SlackSentAt)
            .HasColumnName("slack_sent_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique constraint on URL
        builder.HasIndex(n => n.Url)
            .IsUnique()
            .HasDatabaseName("uq_news_feeds_url");

        // Index for category and published date (for efficient filtering and sorting)
        builder.HasIndex(n => new { n.Category, n.PublishedDate })
            .IsDescending(false, true)
            .HasDatabaseName("idx_news_feeds_category_pubdate");

        // Index for fetched articles (articles not sent to Slack yet)
        builder.HasIndex(n => new { n.FetchedAt, n.SlackSentAt })
            .HasDatabaseName("idx_news_feeds_fetched_slack");
    }
}
