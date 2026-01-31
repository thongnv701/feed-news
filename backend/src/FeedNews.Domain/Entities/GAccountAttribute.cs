using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("gaccount_attribute")]
public class GAccountAttribute
{
    [Key] public long Id { get; set; }

    public long AttributeId { get; set; }

    public long GAccountId { get; set; }

    public string Description { get; set; } = null!;

    public bool IsSpecial { get; set; }

    public int Level { get; set; }

    [ForeignKey("AttributeId")] public virtual Attribute Attribute { get; set; } = null!;

    [ForeignKey("GAccountId")] public virtual GameAccount GameAccount { get; set; } = null!;
}