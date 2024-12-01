using System.ComponentModel.DataAnnotations;

namespace Pronia.Areas.Admin.ViewModels
{
    public class CreateCategoryVM
    {
        [Required(ErrorMessage = "The field is required!")]
        [MaxLength(30, ErrorMessage = "There can be max 30 symbols!")]
        public string Name { get; set; }
    }
}
