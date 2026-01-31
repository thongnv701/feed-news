using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("notification")]
public class Notification
{
    [Key] public long Id { get; set; }

    public long AccountId { get; set; }

    [StringLength(255)] public string Title { get; set; } = null!;

    [Column(TypeName = "text")] public string Content { get; set; } = null!;

    [Column(TypeName = "text")] public string? Data { get; set; }

    public bool IsRead { get; set; }

    public short Type { get; set; }

    [ForeignKey("AccountId")] public virtual Account Account { get; set; } = null!;
}