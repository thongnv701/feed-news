using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("wallet_transaction")]
public class WalletTransaction
{
    [Key] public long Id { get; set; }

    public long WalletId { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal AvailableAmountBefore { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal IncomingAmountBefore { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal Amount { get; set; }

    public short Type { get; set; }

    [Column(TypeName = "text")] public string? Description { get; set; }

    [ForeignKey("WalletId")] public virtual Wallet Wallet { get; set; } = null!;
}