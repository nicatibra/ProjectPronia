namespace Pronia.Models
{
    public class Color : BaseEntity
    {
        public string Name { get; set; }

        //relational
        public List<ProductColor> ProductColors { get; set; }

    }
}
