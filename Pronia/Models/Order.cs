namespace Pronia.Models
{
    public class Order : BaseEntity
    {
        public string DateString { get; set; }
        public string Address { get; set; }
        public decimal TotalPrice { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        public bool? Status { get; set; }
    }
}
