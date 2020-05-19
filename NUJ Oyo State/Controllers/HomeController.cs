using NUJ_Oyo_State.Models;
using NUJ_Oyo_State.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NUJ_Oyo_State.Controllers
{
    public class HomeController : Controller
    {
        // init the db context
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Gallery()
        {
            return View();
        }

        [Authorize]
        public ActionResult Members()
        {
            List<User> users = db.Users.ToList();
            List<MembersVM> membersModel = new List<MembersVM>();
            foreach (var user in users)
            {
                MembersVM member = new MembersVM()
                {
                    FirstName = user.FirstName,
                    OtherNames = user.OtherNames,
                    Designation = user.Designation,
                    Image = db.Images.Where(x => x.UserId == user.Id).FirstOrDefault()
                };

                 membersModel.Add(member);
            }

            return View(membersModel);
        }

        public ActionResult News()
        {
            return View();
        }

        public ActionResult SinglePageNews()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}