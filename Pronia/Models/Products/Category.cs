namespace Pronia.Models.Products
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        //relational
        public List<Product> Products { get; set; }
    }
}
