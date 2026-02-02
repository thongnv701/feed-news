using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FeedNews.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FeedNews.Domain.Entities;

[Table("account")]
[Index("Email", Name = "account_email_unique", IsUnique = true)]
[Index("PhoneNumber", Name = "account_phone_number_unique", IsUnique = true)]
public class Account : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [StringLength(20)] public string? PhoneNumber { get; set; }

    [StringLength(200)] public string Email { get; set; } = null!;

    [StringLength(250)] public string Password { get; set; } = null!;

    [StringLength(300)] public string? AvatarUrl { get; set; }

    public string? FullName { get; set; }

    public Genders Gender { get; set; }

    public AccountTypes Type { get; set; }

    public string? DeviceToken { get; set; }

    public AccountStatus Status { get; set; }

    public int NumOfFlag { get; set; }

    public long RoleId { get; set; }

    [ForeignKey("RoleId")] public virtual Role Role { get; set; } = null!;
}