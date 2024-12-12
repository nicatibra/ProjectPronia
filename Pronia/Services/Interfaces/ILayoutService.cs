using Pronia.ViewModels.Basket;

namespace Pronia.Services.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<List<BasketItemVM>> GetBasketAsync();
    }
}
