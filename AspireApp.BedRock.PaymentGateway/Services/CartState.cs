using AspireApp.BedRock.PaymentGateway.Models;

namespace AspireApp.BedRock.PaymentGateway.Services;

public class CartState
{
    public DeliveryFormModel? DeliveryAddress { get; private set; }
    
    public event Action? OnChange;

    public async Task UpdateDeliveryAddress(DeliveryFormModel address)
    {
        DeliveryAddress = address;
        OnChange?.Invoke();
        await Task.CompletedTask;
    }

    public bool HasDeliveryAddress => DeliveryAddress != null;
}