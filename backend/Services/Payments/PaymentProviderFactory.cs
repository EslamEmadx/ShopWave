using Microsoft.Extensions.DependencyInjection;

namespace backend.Services.Payments;

public class PaymentProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentProvider GetProvider(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "stripe" => _serviceProvider.GetRequiredService<StripePaymentProviderPlaceholder>(),
            "paymob" => _serviceProvider.GetRequiredService<PaymobPaymentProviderPlaceholder>(),
            _ => throw new ArgumentException($"Unsupported payment provider: {providerName}")
        };
    }
}
