using System.ComponentModel;

namespace FeedNews.Domain.Enums;

public enum Roles
{
    [Description("Customer")] Customer = 1,
    [Description("Shop")] Shop = 2,
    [Description("Admin")] Admin = 3
}