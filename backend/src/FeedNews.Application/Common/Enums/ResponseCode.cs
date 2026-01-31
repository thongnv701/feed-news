using System.ComponentModel;

namespace FeedNews.Application.Common.Enums;

public enum ResponseCode
{
    [Description("Common Error")] CommonError = 1,

    [Description("2")] ValidationError = 2,

    [Description("Mapping Error")] MappingError = 3,

    [Description("Unauthorized")] Unauthorized = 4
}