using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Telematics.Models;
using System.Collections.Generic;

namespace Telematics.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public string getNewCompanyCode()
        {
            using (var db = new ApplicationDbContext())
            {
                Random rn;
                int count;
                string code;
                do
                {
                    count = 0;
                    rn = new Random();
                    code = rn.Next(1000, 9999).ToString();
                    count = (from c in db.Companie
                                         where c.code == code
                                         select c).Count();

                } while (count > 0);
                var current_company = (from c in db.Companie
                                       join u in db.Users on c.name equals u.Company
                                       where u.UserName == User.Identity.Name
                                       select c).FirstOrDefault();
                current_company.code = code;
                db.SaveChanges();
                return code;
            }
        }

        //
        // GET: /Account/Profile
        [Authorize(Roles = "Super, Admin")]
        public ActionResult Profile()
        {
            using (var db = new ApplicationDbContext())
            {
                var current_user = (from u in db.Users
                                    join c in db.Companie on u.Company equals c.name
                                    where u.UserName == User.Identity.Name
                                    select new ProfileViewModel() { user = u, company = c }).FirstOrDefault();
            
                return View(current_user);
            }
        }

        //
        // GET: /Account/Vehicles
        [Authorize(Roles = "Super, Admin, Manager")]
        public ActionResult Vehicles()
        {
            using (var db = new ApplicationDbContext())
            {
                var current_user = (from u in db.Users
                                    where u.UserName == User.Identity.Name
                                    select u).FirstOrDefault();
                var vehicles_query = (from v in db.Vehicle
                                      join o in db.OBD on v.device_no equals o.device_id
                                      where o.company == current_user.Company
                                      select v).ToList();

                var vehicles_list = new Vehicles()
                {
                    vehicle_list = vehicles_query.ToList()
                };
                return View(vehicles_list);
            }
        }

        public ActionResult IsOBDUnique(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                try
                {
                    var tag = db.OBD.Single(m => m.device_id == id);
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
        }

        //
        // GET: /Account/VehicleRegister
        [AllowAnonymous]
        [Authorize(Roles = "Super, Admin, Manager")]
        public ActionResult VehicleRegister()
        {
            return View();
        }

        //
        // POST: /Account/VehicleRegister
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> VehicleRegister(VehicleRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var current_user = UserManager.FindById(User.Identity.GetUserId());
                var isUnique = true;
                using (var db = new ApplicationDbContext())
                {
                    var v_count = (from v in db.Vehicle
                                   where v.lic_no == model.LicensePlateNumber
                                   select v).Count();
                    var o_count = (from o in db.OBD
                                   where o.device_id == model.DeviceNo
                                   select o).Count();
                    if (v_count > 0)
                    {
                        isUnique = false;
                        ModelState.AddModelError("LicensePlateNumber", "License plate number already in use!");
                    }
                    if (o_count > 0) {
                        isUnique = false;
                        ModelState.AddModelError("DeviceNo", "Device number already in use!");
                    }
                }
                if (isUnique)
                { 
                    using (var db = new ApplicationDbContext())
                    {
                        db.Vehicle.Add(new Vehicle { lic_no = model.LicensePlateNumber, brand = model.Brand, v_model = model.VehicleModel, device_no = model.DeviceNo });
                        db.OBD.Add(new OBD { id = model.id, device_id = model.DeviceNo, company = current_user.Company });
                        db.SaveChanges();
                        return RedirectToAction("Vehicles", "Account");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/EditVehicle
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> EditVehicle(int vehicleid)
        {
            if (vehicleid == null)
            {
                return RedirectToAction("Vehicles", "Account");
            }
            using (var db = new ApplicationDbContext())
            {
                var vehicle = (from v in db.Vehicle
                               where v.id == vehicleid
                               select v).FirstOrDefault();

                var model = new VehicleRegisterViewModel()
                {
                    id = vehicle.id,
                    LicensePlateNumber = vehicle.lic_no,
                    Brand = vehicle.brand,
                    VehicleModel = vehicle.v_model,
                    DeviceNo = vehicle.device_no
                };
                return View(model);
            }
        }

        //
        // POST: /Account/EditVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> EditVehicle(VehicleRegisterViewModel edit_model)
        {
            if (ModelState.IsValid)
            {
                var isUnique = true;
                using (var db = new ApplicationDbContext())
                {
                    var v_count = (from v in db.Vehicle
                                   where v.lic_no == edit_model.LicensePlateNumber
                                   && v.id != edit_model.id
                                   select v).Count();
                    var o_count = (from o in db.OBD
                                   join v in db.Vehicle on o.device_id equals v.device_no
                                   where o.device_id == edit_model.DeviceNo
                                   && v.id != edit_model.id
                                   select o).Count();
                    if (v_count > 0)
                    {
                        isUnique = false;
                        ModelState.AddModelError("LicensePlateNumber", "License plate number already in use!");
                    }
                    if (o_count > 0)
                    {
                        isUnique = false;
                        ModelState.AddModelError("DeviceNo", "Device number already in use!");
                    }
                }
                if (isUnique)
                {
                    using (var db = new ApplicationDbContext())
                    {
                        var vehicle = (from v in db.Vehicle
                                       where v.id == edit_model.id
                                       select v).FirstOrDefault();
                        var obd = (from o in db.OBD
                                   join v in db.Vehicle on o.device_id equals v.device_no
                                   where v.id == edit_model.id
                                   select o).FirstOrDefault();
                        vehicle.lic_no = edit_model.LicensePlateNumber;
                        vehicle.brand = edit_model.Brand;
                        vehicle.v_model = edit_model.VehicleModel;
                        vehicle.device_no = edit_model.DeviceNo;
                        obd.device_id = edit_model.DeviceNo;

                        db.SaveChanges();
                        return RedirectToAction("Vehicles", "Account");
                    }
                }
            }
            

            // If we got this far, something failed, redisplay form
            return View(edit_model);
        }


        //
        // POST: /Account/Delete
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> DeleteVehicle(int vehicleid)
        {
            using (var db = new ApplicationDbContext())
            {
                var vehicle = (from v in db.Vehicle
                               where v.id == vehicleid
                               select v).FirstOrDefault();
                var obd = (from o in db.OBD
                           join v in db.Vehicle on o.device_id equals v.device_no
                           select o).FirstOrDefault();
                db.Vehicle.Remove(vehicle);
                db.OBD.Remove(obd);
                db.SaveChanges();
                return RedirectToAction("Vehicles", "Account");
            }
        }


        //
        // GET: /Account/Management
        [Authorize(Roles = "Super, Admin, Manager")]
        public ActionResult Management()
        {
            using (var db = new ApplicationDbContext())
            {
                var users = new List<ApplicationUser>();
                if (User.IsInRole("Super"))
                {
                    users = (from u in db.Users
                                 join r in db.Roles on u.Roles.FirstOrDefault().RoleId equals r.Id
                                 join c in db.Companie on u.Company equals c.name
                                 where r.Name != "Super"
                                 && c.name == "Citrine"
                                 select u).ToList();
                } else if (User.IsInRole("Admin"))
                {
                    users = (from u in db.Users
                                 join r in db.Roles on u.Roles.FirstOrDefault().RoleId equals r.Id
                                 join c in db.Companie on u.Company equals c.name
                                 where r.Name != "Super"
                                 where r.Name != "Admin"
                                 select u).ToList();
                } else if (User.IsInRole("Manager"))
                {
                    users = (from u in db.Users
                                 join r in db.Roles on u.Roles.FirstOrDefault().RoleId equals r.Id
                                 join c in db.Companie on u.Company equals c.name
                                 where r.Name == "Driver"
                                 select u).ToList();
                } else
                {
                    users = (from u in db.Users
                                 join r in db.Roles on u.Roles.FirstOrDefault().RoleId equals r.Id
                                 join c in db.Companie on u.Company equals c.name
                                 select u).ToList();
                }
                

                var roles = new List<string>();
                foreach (var user in users)
                {

                    if (user.Roles.Count > 0)
                    {
                        var t = user.Roles.First().RoleId;
                        var rolename = (from r in db.Roles
                                        where r.Id.Equals(t)
                                        select r.Name).FirstOrDefault();

                        roles.Add(rolename);
                    }
                    else
                    {
                        roles.Add("");
                    }

                }

                var model = new UserWithRoles()
                {
                    users = users.ToList(),
                    roles = roles.ToList()
                };
                return View(model);
            }
        }

        //
        // GET: /Account/Edit
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> Edit(string userid)
        {
            if (userid == null)
            {
                return RedirectToAction("Management", "Account");
            }
            var target_user = await UserManager.FindByIdAsync(userid);
            var access_right = "Driver";
            if (UserManager.IsInRole(target_user.Id, "Manager"))
            {
                access_right = "Manager";

                using (var appdb = new ApplicationDbContext())
                {
                    var list = (from r in appdb.Roles
                                where r.Name == "Driver"
                                select new SelectListItem() { Text = r.Name, Value = r.Name }
                                 ).ToList();
                    foreach (SelectListItem item in list)
                    {
                        if (item.Value.Equals(access_right))
                        {
                            item.Selected = true;
                        }
                    }
                    ViewBag.SelectList = list;
                }
            } else if (UserManager.IsInRole(target_user.Id, "Admin"))
            {
                access_right = "Admin";

                using (var appdb = new ApplicationDbContext())
                {
                    var list = (from r in appdb.Roles
                                where r.Name != "Super"
                                && r.Name != "Admin"
                                select new SelectListItem() { Text = r.Name, Value = r.Name }
                                 ).ToList();
                    foreach (SelectListItem item in list)
                    {
                        if (item.Value.Equals(access_right))
                        {
                            item.Selected = true;
                        }
                    }
                    ViewBag.SelectList = list;
                }
            }

            var model = new EditViewModel()
            {
                Id = target_user.Id,
                Email = target_user.Email,
                AccessRole = access_right
            };
            return View(model);
        }

        //
        // POST: /Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> Edit(EditViewModel edit_model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(edit_model.Email);
                var result = await UserManager.ChangePasswordAsync(edit_model.Id, edit_model.OldPassword, edit_model.Password);
                if (result.Succeeded)
                {
                    var role_result = await UserManager.AddToRoleAsync(user.Id, edit_model.AccessRole);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Management", "Account");
                }
                AddErrors(result);
            }

            var access_right = "Driver";
            if (UserManager.IsInRole(edit_model.Id, "Manager"))
            {
                access_right = "Manager";

                using (var appdb = new ApplicationDbContext())
                {
                    var list = (from r in appdb.Roles
                                where r.Name == "Driver"
                                select new SelectListItem() { Text = r.Name, Value = r.Name }
                                 ).ToList();
                    foreach (SelectListItem item in list)
                    {
                        if (item.Value.Equals(access_right))
                        {
                            item.Selected = true;
                        }
                    }
                    ViewBag.SelectList = list;
                }
            }
            else if (UserManager.IsInRole(edit_model.Id, "Admin"))
            {
                access_right = "Admin";

                using (var appdb = new ApplicationDbContext())
                {
                    var list = (from r in appdb.Roles
                                where r.Name != "Super"
                                && r.Name != "Admin"
                                select new SelectListItem() { Text = r.Name, Value = r.Name }
                                 ).ToList();
                    foreach (SelectListItem item in list)
                    {
                        if (item.Value.Equals(access_right))
                        {
                            item.Selected = true;
                        }
                    }
                    ViewBag.SelectList = list;
                }
            }

            // If we got this far, something failed, redisplay form
            return View(edit_model);
        }

        //
        // POST: /Account/Delete
        [Authorize(Roles = "Super, Admin, Manager")]
        public async void Delete(string userid)
        {
            var user = await UserManager.FindByIdAsync(userid);
            var result = await UserManager.DeleteAsync(user);
            RedirectToAction("Management", "Account");
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
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
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
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
        [Authorize(Roles = "Super, Admin, Manager")]
        public ActionResult Register()
        {
            var access_right = "Driver";
            if (User.IsInRole("Manager"))
            {
                access_right = "Manager";

                using (var appdb = new ApplicationDbContext())
                {
                    var list = (from r in appdb.Roles
                                where r.Name == "Driver"
                                select new SelectListItem() { Text = r.Name, Value = r.Name }
                                 ).ToList();
                    foreach (SelectListItem item in list)
                    {
                        if (item.Value.Equals(access_right))
                        {
                            item.Selected = true;
                        }
                    }
                    ViewBag.SelectList = list;
                }
            }
            else if (User.IsInRole("Admin"))
            {
                access_right = "Admin";

                using (var appdb = new ApplicationDbContext())
                {
                    var list = (from r in appdb.Roles
                                where r.Name != "Super"
                                && r.Name != "Admin"
                                select new SelectListItem() { Text = r.Name, Value = r.Name }
                                 ).ToList();
                    foreach (SelectListItem item in list)
                    {
                        if (item.Value.Equals(access_right))
                        {
                            item.Selected = true;
                        }
                    }
                    ViewBag.SelectList = list;
                }
            }
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Super, Admin, Manager")]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var current_user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber = model.PhoneNumber, Company = current_user.Company };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var role_result = await UserManager.AddToRoleAsync(user.Id, model.AccessRole);
                    
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Management", "Account");
                }
                AddErrors(result);
            }

            using (var appdb = new ApplicationDbContext())
            {
                ViewBag.SelectList = (from r in appdb.Roles
                                      where r.Name != "Super"
                                      select new SelectListItem() { Text = r.Name, Value = r.Name }
                             ).ToList();
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
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

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
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
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
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
            return RedirectToAction("Login", "Account");
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
            return RedirectToAction("Index", "Dashboard");
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
        #endregion
    }
}