using Stripe;
using Stripe.Checkout;
using backend.Models;

namespace backend.Services;

public class StripeService
{
    private readonly IConfiguration _config;

    public StripeService(IConfiguration config)
    {
        _config = config;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"] ?? "sk_test_placeholder";
    }

    public async Task<Session> CreateCheckoutSessionAsync(Order order, List<OrderItem> items, List<Models.Product> products)
    {
        var lineItems = items.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Name,
                        Images = new List<string> { product.ImageUrl }
                    },
                    UnitAmount = (long)(item.Price * 100)
                },
                Quantity = item.Quantity
            };
        }).ToList();

        if (order.DiscountAmount > 0)
        {
            lineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"Discount ({order.CouponCode})"
                    },
                    UnitAmount = -(long)(order.DiscountAmount * 100)
                },
                Quantity = 1
            });
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = _config["Stripe:SuccessUrl"] ?? "http://localhost:5173/orders?success=true",
            CancelUrl = _config["Stripe:CancelUrl"] ?? "http://localhost:5173/cart?cancelled=true",
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() }
            }
        };

        var service = new SessionService();
        return await service.CreateAsync(options);
    }
}
