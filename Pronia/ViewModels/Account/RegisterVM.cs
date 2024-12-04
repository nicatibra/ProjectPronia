using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModels
{
    public class RegisterVM
    {
        [MinLength(3, ErrorMessage = "Required minimum 3 symbols")]
        [MaxLength(25, ErrorMessage = "Maximum 25 symbols are allowed")]
        public string Name { get; set; }


        [MinLength(3, ErrorMessage = "Required minimum 3 symbols")]
        [MaxLength(25, ErrorMessage = "Maximum 25 symbols are allowed")]
        public string Surname { get; set; }


        [MinLength(4, ErrorMessage = "Required minimum 4 symbols")]
        [MaxLength(100, ErrorMessage = "Maximum 100 symbols are allowed")]
        public string UserName { get; set; }


        [MaxLength(256, ErrorMessage = "Maximum 256 symbols are allowed")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }


        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }


    }
}
