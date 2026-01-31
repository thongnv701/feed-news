using System.ComponentModel;

namespace FeedNews.Domain.Enums;

public enum AccountTypes
{
    [Description("Local")] Local = 1,
    [Description("Google")] Google = 2
}