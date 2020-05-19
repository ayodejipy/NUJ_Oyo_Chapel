using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NUJ_Oyo_State.Models;
using NUJ_Oyo_State.Models.Data;
using NUJ_Oyo_State.Models.Helpers;
using NUJ_Oyo_State.Models.ViewModels.Helpers;

namespace NUJ_Oyo_State.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        // init the db context
        private ApplicationDbContext db = new ApplicationDbContext();

        private SignInManager _signInManager;
        private UserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(UserManager userManager, SignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public SignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<SignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return Redirect("~/manage/index");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MembershipRequestVM model)
        {
            // check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // set gender
            string gender;
            if (model.Gender == "1")
            {
                gender = "Male";
            }
            else
            {
                gender = "Female";
            }

            // init membershipRequest
            MembershipRequestDTO membershipRequest = new MembershipRequestDTO()
            {
                FirstName = model.FirstName,
                OtherNames = model.OtherNames,
                Gender = gender,
                DOB = model.DOB.ToUniversalTime(),
                StateOfOrigin = model.StateOfOrigin,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Zip = model.Zip,
                MembershipId = model.MembershipId,
                Branch = model.Branch,
                Designation = model.Designation,
                ImageString = model.ImageString,
                Date = DateTime.UtcNow,
                Active = true
            };

            // save the request to database
            db.MembershipRequest.Add(membershipRequest);
            db.SaveChanges();

            // Set TempData message
            TempData["SM"] = "Your membership request has been sent successfully. A confirmation email will be sent to you.";

            // send mail notification to admin
            // Init and send email
            EmailMessageVM mailModel = new EmailMessageVM();

            // init redirect link from email
            var request = Request.Url.Authority.ToString().ToLower();
            var link = request + "/Home/About";

            mailModel.ToAddress = "davidire71@gmail.com";
            mailModel.Subject = "New Membership Registration Request";
            string message = "A New member has requested to register to the NUJ Oyo State Portal. Below are the details.";
            message += "<br><br>Name: " + model.FirstName.ToUpper() + " " + model.OtherNames;
            message += "<br>Member Id: " + model.MembershipId;
            message += "<br>Branch: " + model.Branch;
            message += "<br>Designation: " + model.Designation;
            message += "<br><br> <html><body><a href='https://" + link + "' style=\"padding: 8px 12px; border: 1px solid #17454E;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#17454E;text-decoration: none;font-weight:bold;display: inline-block;\">See Full Details Here</a></body></html>"; ;

            mailModel.Message = message;

            // send mail
            Email.SendMail(mailModel);

            return View();
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(int userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        private async Task<bool> CreateAccount(List<MembershipRequestDTO> model)
        {
            foreach (var member in model)
            {
                var user = new User
                {
                    UserName = member.Email,
                    Email = member.Email,
                    FirstName = member.FirstName,
                    OtherNames = member.OtherNames,
                    Gender = member.Gender,
                    DOB = member.DOB,
                    StateOfOrigin = member.StateOfOrigin,
                    Address = member.Address,
                    City = member.City,
                    State = member.State,
                    Zip = member.Zip,
                    MembershipId = member.MembershipId,
                    Branch = member.Branch,
                    Designation = member.Designation,
                };

                Random random = new Random();

                const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string lowerChars = "abcdefghijklmopqrstuvwxyz";
                const string num = "0123456789";
                string defaultPassword;

                defaultPassword = new string(Enumerable.Repeat(upperChars, 2)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                defaultPassword += new string(Enumerable.Repeat(num, 4)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                defaultPassword += new string(Enumerable.Repeat(lowerChars, 2)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                var result = await UserManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    //return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            if (model.Count > 1)
            {
                TempData["SM"] = "All members has been approved successfully.";
            }
            else
            {
                TempData["SM"] = "Member has been approved successfully.";
            }
            return true;
        }
        #endregion


        //GET:/Account/UserNavPartial
        public string UserNavPartial()
        {
            // Get username
            int userId = User.Identity.GetUserId<int>();

            return db.Users.Where(x => x.Id == userId).Select(x => x.FirstName).FirstOrDefault();
        }

        // GET:/Account/Requests
        [Authorize(Roles = "Admin")]
        public ActionResult Requests(int? id)
        {
            // Declare a list of memberRequests
            List<MembershipRequestVM> memberRequests;

            // return all requests if id == null
            if (id == null)
            {
                memberRequests =
                    db.MembershipRequest.ToArray()
                    .Where(x => x.Active == true)
                    .Select(x => new MembershipRequestVM(x)).ToList();
            }
            else
            {
                memberRequests =
                    db.MembershipRequest.ToArray()
                    .Where(x => x.Id == id && x.Active == true)
                    .Select(x => new MembershipRequestVM(x)).ToList();
            }

            // return view with memberRequests
            return View(memberRequests);
        }


        //POST:/Account/Approve
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Approve(int id)
        {
            if (ModelState.IsValid)
            {
                // init requestDTO
                MembershipRequestDTO requestDTO =
                    db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

                User existingUser = db.Users.Where(x => x.MembershipId == requestDTO.MembershipId).FirstOrDefault();
                if (existingUser != null)
                {
                    TempData["SM"] = "Error: Member with the ID: " + requestDTO.MembershipId + " already exist";
                    return RedirectToAction("Requests");
                }


                var user = new User
                {
                    UserName = requestDTO.Email,
                    Email = requestDTO.Email,
                    FirstName = requestDTO.FirstName,
                    OtherNames = requestDTO.OtherNames,
                    Gender = requestDTO.Gender,
                    DOB = requestDTO.DOB,
                    StateOfOrigin = requestDTO.StateOfOrigin,
                    Address = requestDTO.Address,
                    City = requestDTO.City,
                    State = requestDTO.State,
                    Zip = requestDTO.Zip,
                    MembershipId = requestDTO.MembershipId,
                    Branch = requestDTO.Branch,
                    Designation = requestDTO.Designation,
                };

                // generate a random password
                Random random = new Random();

                const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string lowerChars = "abcdefghijklmopqrstuvwxyz";
                const string num = "0123456789";
                const string specialChars = "()!#%";
                string defaultPassword;

                defaultPassword = new string(Enumerable.Repeat(specialChars, 1)
                 .Select(s => s[random.Next(s.Length)]).ToArray());

                defaultPassword += new string(Enumerable.Repeat(upperChars, 2)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                defaultPassword += new string(Enumerable.Repeat(num, 4)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                defaultPassword += new string(Enumerable.Repeat(lowerChars, 3)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                defaultPassword += new string(Enumerable.Repeat(specialChars, 1)
                 .Select(s => s[random.Next(s.Length)]).ToArray());


                var result = await UserManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    // save user image
                    ImagesDTO userImage = new ImagesDTO
                    {
                        Image1 = requestDTO.ImageString,
                        UserId = user.Id
                    };
                    db.Images.Add(userImage);

                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    var message = "Hello,";
                    message += "<br>Your request to join NUJ - Oyo Chapel has just been approved. This is your password <strong>" + defaultPassword + "</strong>";
                    message += "<br>Kindly click the button below to confirm your email address and log in with the password above.";
                    message += "<br><br> <html><body><a href='/" + callbackUrl + "' style=\"padding: 8px 12px; border: 1px solid #17454E;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#17454E;text-decoration: none;font-weight:bold;display: inline-block;\">See Full Details Here</a></body></html>";

                    await UserManager.SendEmailAsync(user.Id, "NUJ - Registration Request Approved.", message);

                    // update the user in membership request
                    requestDTO.DefaultPassword = defaultPassword;
                    requestDTO.UserApproved = true;
                    db.SaveChanges();

                    TempData["SM"] = "Member has been approved successfully.";
                }
                else
                {
                    string errMsg = "";
                    foreach (var error in result.Errors)
                    {
                        if (!error.ToString().Contains("Name"))
                            errMsg += error.ToString() + ", ";
                    }

                    TempData["SM"] = "Error: " + errMsg;
                }
            }

            // If we got this far, something failed, redisplay form
            return RedirectToAction("Requests");
        }

        //POST:/Account/Decline
        [Authorize(Roles = "Admin, BranchAdmin")]
        public string Decline(int id, string reason)
        {
            // init memberDTO
            MembershipRequestDTO memberDTO =
                    db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

            // deactivate the member
            memberDTO.UserApproved = false;
            memberDTO.Comment = reason;
            db.SaveChanges();

            // Send an email with this link
            EmailMessageVM mailModel = new EmailMessageVM();

            mailModel.ToAddress = memberDTO.Email;
            mailModel.Subject = "Membership Registration Declined";

            var message = "Hello " + memberDTO.FirstName.ToUpper() + ",<br/>";
            message += "Your request to join NUJ - Oyo Chapel has been declined.<br/>";
            message += "Note: " + reason + "<br/>";
            message += "Please contact NUJ Oyo Chapter for further enquiries or assistance.<br/>";
            message += "<address> <br/>";
            message += "One Microsoft Way <br/>";
            message += "Redmond, WA 98052 - 6399 <br/>";
            message += "<abbr title = \"Phone\"> P:";
            message += " 425.555.0100 <br/>";
            message += "Email: info@nujoyo.com <br/>";
            message += "</address> ";

            mailModel.Message = message;

            bool sent = Email.SendMail(mailModel);
            if (sent)
            {
                return "Member request declined successfully.";
            }
            else
            {
                return "Member request declined successfully. Unable to send email notification, try re-sending.";
            }
        }


        //GET:/Account/confirm-again
        [Authorize(Roles = "Admin, BranchAdmin")]
        [ActionName("confirm-again")]
        public async Task<string> ConfirmAgainAsync(int id)
        {
            // init requestDTO
            MembershipRequestDTO requestDTO =
                db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

            int userId = db.Users.Where(x => x.MembershipId == requestDTO.MembershipId).Select(x => x.Id).FirstOrDefault();
            if (userId == null)
                return "This user is not registered yet.";

            // Send an email with this link
            EmailMessageVM mailModel = new EmailMessageVM();

            mailModel.ToAddress = requestDTO.Email;
            mailModel.Subject = "NUJ - Registration Request Approved.";

            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userId);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userId, code = code }, protocol: Request.Url.Scheme);
            var message = "Hello,";
            message += "<br>Your request to join NUJ - Oyo Chapel has just been approved. This is your password <strong>" + requestDTO.DefaultPassword + "</strong>";
            message += "<br>Kindly click the button below to confirm your email address and log in with the password above.";
            message += "<br><br> <html><body><a href='/" + callbackUrl + "' style=\"padding: 8px 12px; border: 1px solid #17454E;border-radius: 2px;font-family: Helvetica, Arial, sans-serif;font-size: 14px; color: #ffffff;background-color:#17454E;text-decoration: none;font-weight:bold;display: inline-block;\">See Full Details Here</a></body></html>";
            mailModel.Message = message;

            bool sent = Email.SendMail(mailModel);
            if (sent)
            {
                return "Email re-sent successfully.";
            }
            else
            {
                return "Unable to re-send email, please try again later.";
            }
        }


        //GET:/Account/confirm-again
        [Authorize(Roles = "Admin, BranchAdmin")]
        [ActionName("decline-again")]
        public string DeclineAgain(int id)
        {
            // init requestDTO
            MembershipRequestDTO memberDTO =
                db.MembershipRequest.Where(x => x.Id == id).FirstOrDefault();

            // Send an email with this link
            EmailMessageVM mailModel = new EmailMessageVM();

            mailModel.ToAddress = memberDTO.Email;
            mailModel.Subject = "Membership Registration Declined";

            var message = "Hello " + memberDTO.FirstName.ToUpper() + ",<br/>";
            message += "Your request to join NUJ - Oyo Chapel has been declined.<br/>";
            message += "Note: " + memberDTO.Comment + "<br/>";
            message += "Please contact NUJ Oyo Chapter for further enquiries or assistance.<br/>";
            message += "<address> <br/>";
            message += "One Microsoft Way <br/>";
            message += "Redmond, WA 98052 - 6399 <br/>";
            message += "<abbr title = \"Phone\"> P:";
            message += " 425.555.0100 <br/>";
            message += "Email: info@nujoyo.com <br/>";
            message += "</address> ";

            mailModel.Message = message;

            bool sent = Email.SendMail(mailModel);
            if (sent)
            {
                return "Email re-sent successfully.";
            }
            else
            {
                return "Unable to re-send email, please try again later.";
            }
        }

                          







        [AllowAnonymous]
        public ActionResult Test()
        {
            // save user image
            ImagesDTO userImage = new ImagesDTO
            {

                Image1 = "this.imageijdnfldsfs",
                UserId = 1
            };

            db.Images.Add(userImage);
            db.SaveChanges();


            return View();
        }
    }
}