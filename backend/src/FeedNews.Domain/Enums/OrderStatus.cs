using System.ComponentModel;

namespace FeedNews.Domain.Enums;

public enum OrderStatus
{
    [Description("Placed: Your order has been successfully placed.")]
    Pending = 1,

    [Description("The restaurant has confirmed your order")]
    Confirmed = 2,

    [Description("Your order is on its way.")]
    Delivering = 3,

    [Description("Your order has been delivered. Enjoy your meal!")]
    Successful = 4,

    [Description("our order has been cancelled by customer or shop.")]
    Cancelled = 5,

    [Description("Your order has been cancelled by shop.")]
    Fail = 6,

    [Description("Your order has been Reject.")]
    Rejected = 7
}