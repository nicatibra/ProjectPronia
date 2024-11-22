namespace Pronia.Models.Products
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; }

        public bool? IsPrimary { get; set; }


        //relational
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}
