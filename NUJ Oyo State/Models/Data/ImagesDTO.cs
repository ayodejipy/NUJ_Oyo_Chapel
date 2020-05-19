using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NUJ_Oyo_State.Models.Data
{
    [Table("tblImages")]
    public class ImagesDTO
    {
        [Key]
        public int ImageId { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public int UserId { get; set; }
        public int NewsId { get; set; }
    }
}