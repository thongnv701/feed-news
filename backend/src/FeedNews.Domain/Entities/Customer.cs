using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("customer")]
public class Customer
{
    [Key] public long Id { get; set; }

    public long WalletId { get; set; }

    [ForeignKey("WalletId")] public virtual Wallet Wallet { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}