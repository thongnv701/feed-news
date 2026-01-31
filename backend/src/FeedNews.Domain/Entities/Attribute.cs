using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("attribute")]
public class Attribute
{
    [Key] public long Id { get; set; }

    public long GameId { get; set; }

    [StringLength(255)] public string Key { get; set; } = null!;

    [StringLength(255)] public string Value { get; set; } = null!;

    [StringLength(255)] public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    [ForeignKey("GameId")] public virtual Game Game { get; set; } = null!;

    public virtual ICollection<GAccountAttribute> GAccountAttributes { get; set; } = new List<GAccountAttribute>();
}