using System.ComponentModel.DataAnnotations;

namespace Pronia.ViewModels
{
    public class LoginVM
    {
        [MinLength(4, ErrorMessage = "Symbols in Username or Email can't be less than 4!")]
        [MaxLength(256, ErrorMessage = "Symbols in Username or Email can't be more than 256!")]
        public string UsernameOrEmail { get; set; }



        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Symbols in Password can't be less than 4!")]
        public string Password { get; set; }



        public bool IsPersistent { get; set; }
    }
}
