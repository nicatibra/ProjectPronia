using Pronia.ViewModels;

namespace Pronia.Services.Implementations
{
    public interface IBasketService
    {
        public Task<List<BasketItemVM>> GetBasketAsync();

    }
}
