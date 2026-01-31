namespace FeedNews.Domain.Entities;

public class BaseEntity
{
    public long? CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}