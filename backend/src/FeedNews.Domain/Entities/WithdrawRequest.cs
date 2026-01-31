using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("withdraw_request")]
public class WithdrawRequest
{
    [Key] public long Id { get; set; }

    public long WalletId { get; set; }

    [StringLength(255)] public string BankCode { get; set; } = null!;

    [StringLength(255)] public string BankShortName { get; set; } = null!;

    [StringLength(255)] public string BankAccountNumber { get; set; } = null!;

    [StringLength(255)] public string BankAccountName { get; set; } = null!;

    [Column(TypeName = "decimal(8, 2)")] public decimal Amount { get; set; }

    [Column(TypeName = "text")] public string? Reason { get; set; }

    [ForeignKey("WalletId")] public virtual Wallet Wallet { get; set; } = null!;
}