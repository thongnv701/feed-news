using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("game")]
public class Game
{
    [Key] public long Id { get; set; }

    [StringLength(255)] public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [StringLength(255)] public string LogoUrl { get; set; } = null!;

    public virtual ICollection<Attribute> Attributes { get; set; } = new List<Attribute>();

    public virtual ICollection<GameAccount> GameAccounts { get; set; } = new List<GameAccount>();
}