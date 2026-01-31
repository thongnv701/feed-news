using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("wallet")]
public class Wallet
{
    [Key] public long Id { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal AvailableAmount { get; set; }

    [Column(TypeName = "decimal(8, 2)")] public decimal IncomingAmount { get; set; }

    public int Status { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

    public virtual ICollection<WithdrawRequest> WithdrawRequests { get; set; } = new List<WithdrawRequest>();
}