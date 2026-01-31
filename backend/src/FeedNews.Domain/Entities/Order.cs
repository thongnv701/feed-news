using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("order")]
public class Order
{
    [Key] public long Id { get; set; }

    public long CustomerId { get; set; }

    public long GAccountId { get; set; }

    public short Status { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal Price { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal Promotion { get; set; }

    public DateTimeOffset OrderAt { get; set; }

    public DateTimeOffset? CompleteAt { get; set; }

    public bool IsRefund { get; set; }

    public bool IsReport { get; set; }

    [Column(TypeName = "text")] public string? Reason { get; set; }

    [ForeignKey("CustomerId")] public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("GAccountId")] public virtual GameAccount GameAccount { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}