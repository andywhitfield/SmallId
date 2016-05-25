using SmallId.Code;
using SmallId.Controllers.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Web.Mvc;

namespace SmallId.Controllers
{
    [HandleError]
    public class AccountController : Controller
    {
        private readonly IFormsAuthentication formsAuth;
        private readonly IMembershipService membershipService;

        public AccountController()
        {
            formsAuth = new FormsAuthenticationService();
            membershipService = new RegistryMembershipService();
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Needs to take same parameter type as Controller.Redirect()")]
        public ActionResult LogOn(string userName, string password, bool rememberMe, string returnUrl)
        {
            if (!ValidateLogOn(userName, password))
            {
                return View();
            }

            formsAuth.SignIn(userName, rememberMe);
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Register(RegistrationViewModel registrationViewModel)
        {
            if (!CreateNewRegistration(registrationViewModel))
            {
                return View();
            }

            formsAuth.SignIn(registrationViewModel.Username, registrationViewModel.RememberMe);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult LogOff()
        {
            formsAuth.SignOut();

            return RedirectToAction("Index", "Home");
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity is WindowsIdentity)
            {
                throw new InvalidOperationException("Windows authentication is not supported.");
            }
        }

        private bool ValidateLogOn(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", "You must specify a username.");
            }
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("password", "You must specify a password.");
            }
            if (!membershipService.ValidateUser(userName, password))
            {
                ModelState.AddModelError("_FORM", "The username or password provided is incorrect.");
            }

            return ModelState.IsValid;
        }

        private bool CreateNewRegistration(RegistrationViewModel registrationViewModel)
        {
            if (string.IsNullOrWhiteSpace(registrationViewModel.Username))
            {
                ModelState.AddModelError("username", "You must provide a username.");
            }
            if (string.IsNullOrWhiteSpace(registrationViewModel.EmailAddress))
            {
                ModelState.AddModelError("emailaddress", "You must provide an email address.");
            }
            if (string.IsNullOrWhiteSpace(registrationViewModel.Password))
            {
                ModelState.AddModelError("password", "You must provide a password.");
            }
            if (registrationViewModel.Password != registrationViewModel.ConfirmPassword)
            {
                ModelState.AddModelError("confirmpassword", "The passwords do not match. Please make sure you've entered your password correctly.");
            }

            if (ModelState.IsValid && !membershipService.RegisterNewUser(registrationViewModel.Username, registrationViewModel.Password, registrationViewModel.EmailAddress))
            {
                ModelState.AddModelError("_FORM", "Could not register this new account. Please try an alternative username.");
            }
            return ModelState.IsValid;
        }
    }
}