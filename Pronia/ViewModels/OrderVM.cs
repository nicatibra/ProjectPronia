namespace Pronia.ViewModels
{
    public class OrderVM
    {
        public string Address { get; set; }

        public List<BasketInOrderItemVM>? BasketInOrderItemsVMs { get; set; }
    }
}
