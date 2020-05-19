using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NUJ_Oyo_State.Models.Data
{
    [Table("tblCategories")]
    public class CategoriesDTO
    {
        [Key]
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public string Slug { get; set; }
    }
}