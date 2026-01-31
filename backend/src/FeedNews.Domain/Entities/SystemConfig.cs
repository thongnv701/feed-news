using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedNews.Domain.Entities;

[Table("system_config")]
public class SystemConfig
{
    [Key] public long Id { get; set; }

    public string PopupMessage { get; set; } = null!;

    public string AnnounceMessage { get; set; } = null!;
}