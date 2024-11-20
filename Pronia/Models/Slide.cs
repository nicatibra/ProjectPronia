namespace Pronia.Models
{
    public class Slide : BaseEntity
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Order { get; set; }


        public Slide(string title, string subTititle, string description, string imageUrl, int order, bool isdeleted, DateTime created)
        {
            Title = title;
            SubTitle = subTititle;
            Description = description;
            ImageUrl = imageUrl;
            Order = order;
            IsDeleted = isdeleted;
            CreatedAt = created;
        }

        public Slide()
        {

        }

    }
}
