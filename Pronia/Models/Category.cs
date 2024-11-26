using System.ComponentModel.DataAnnotations;

namespace Pronia.Models
{
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "The field is required!")]
        [MaxLength(30, ErrorMessage = "Herf sayinin limitini ashdiniz!")]
        public string Name { get; set; }

        //relational
        public List<Product>? Products { get; set; }
    }
}
