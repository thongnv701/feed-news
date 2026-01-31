using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("game_account")]
public class GameAccount
{
    [Key] public long Id { get; set; }

    public long GameId { get; set; }

    [StringLength(255)] public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    [Column(TypeName = "decimal(8, 2)")] public decimal Price { get; set; }

    public short Status { get; set; }

    [Column(TypeName = "text")] public string? Description { get; set; }

    [StringLength(255)] public string? ImageUrl { get; set; }

    [StringLength(255)] public string? AttributesJson { get; set; }

    [ForeignKey("GameId")] public virtual Game Game { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<GAccountAttribute> GAccountAttributes { get; set; } = new List<GAccountAttribute>();
}