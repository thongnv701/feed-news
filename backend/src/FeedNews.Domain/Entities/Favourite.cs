using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("favourite")]
public class Favourite
{
    [Key] public long Id { get; set; }

    public long GAccountId { get; set; }

    public long CustomerId { get; set; }

    [ForeignKey("GAccountId")] public virtual GameAccount GameAccount { get; set; } = null!;

    [ForeignKey("CustomerId")] public virtual Account Customer { get; set; } = null!;
}