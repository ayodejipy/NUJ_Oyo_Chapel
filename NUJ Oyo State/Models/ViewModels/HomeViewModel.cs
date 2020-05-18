using NUJ_Oyo_State.Models.Data;

namespace NUJ_Oyo_State.Models.ViewModels
{
    public class MembersVM
    {
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string Designation { get; set; }

        public virtual ImagesDTO Image { get; set; }
    }
}