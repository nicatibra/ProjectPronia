using Microsoft.AspNetCore.Identity;

namespace Pronia.Models
{
    public class AppUser : IdentityUser
    {
        //Gelecekde gender, adres, prof sekili de olacaq
        public string Name { get; set; }
        public string Surname { get; set; }

        public List<BasketItem> BasketItems { get; set; }

    }
}
