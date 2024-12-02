namespace Pronia.Models
{
    public class Size : BaseEntity
    {
        public string Name { get; set; }

        //relational
        public List<ProductSize> ProductSize { get; set; }


    }
}
