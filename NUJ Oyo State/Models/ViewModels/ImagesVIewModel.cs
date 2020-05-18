using NUJ_Oyo_State.Models.Data;

namespace NUJ_Oyo_State.Models.ViewModels
{
    public class Images
    {
        public Images() { }

        public Images(ImagesDTO row)
        {
            ImageId = row.ImageId;
            Image1 = row.Image1;
            Image2 = row.Image2;
            UserId = row.UserId;
            NewsId = row.NewsId;
        }

        public int ImageId { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public int UserId { get; set; }
        public int NewsId { get; set; }
    }

    public class Gallery
    {
        public Gallery() { }

        public Gallery(GalleryDTO row)
        {
            GalleryId = row.GalleryId;
            Image = row.Image;
            Description = row.Description;
        }

        public int GalleryId { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}