using System.ComponentModel.DataAnnotations.Schema;

namespace Pronia.Models
{
    public class Slide : BaseEntity
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Order { get; set; }

        [NotMapped] //sql dusmesin deye
        public IFormFile Photo { get; set; }
    }
}
