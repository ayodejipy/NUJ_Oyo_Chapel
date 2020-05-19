using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NUJ_Oyo_State.Models.Data
{
    [Table("tblGallery")]
    public class GalleryDTO
    {
        [Key]
        public int GalleryId { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}