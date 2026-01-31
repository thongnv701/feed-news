using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("payment")]
public class Payment
{
    [Key] public long Id { get; set; }

    public long OrderId { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal Amount { get; set; }

    public long Status { get; set; }

    public long Type { get; set; }

    [StringLength(255)] public string? TransactionId { get; set; }

    [Column(TypeName = "text")] public string? TransactionContent { get; set; }

    [ForeignKey("OrderId")] public virtual Order Order { get; set; } = null!;
}