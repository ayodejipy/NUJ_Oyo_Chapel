using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NUJ_Oyo_State.Models.Data
{
    [Table("tblMembershipRequest")]
    public class MembershipRequestDTO
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public string StateOfOrigin { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string MembershipId { get; set; }
        public string Branch { get; set; }
        public string Designation { get; set; }
        public string ImageString { get; set; }
        public DateTime Date { get; set; }
        public bool? UserApproved { get; set; }
        public string DefaultPassword { get; set; }
        public bool Active { get; set; }
        public string Comment { get; set; }
    }
}