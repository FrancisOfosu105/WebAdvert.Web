using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly CognitoSignInManager<CognitoUser> _signInManager;
        private readonly CognitoUserManager<CognitoUser> _cognitoUserManager;
        private readonly CognitoUserPool _cognitoUserPool;

        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager,
            CognitoUserPool cognitoUserPool)
        {
            _signInManager = signInManager as CognitoSignInManager<CognitoUser>;
            _cognitoUserManager = userManager as CognitoUserManager<CognitoUser>;
            _cognitoUserPool = cognitoUserPool;
        }

        // GET
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);


            if (result.Succeeded)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid username or password.");

            return View(model);
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var user = _cognitoUserPool.GetUser(viewModel.Email);

            if (user.Status != null)
            {
                ModelState.AddModelError("UserExists", "User with this email already exists");
                return View(viewModel);
            }

            user.Attributes.Add(CognitoAttribute.Name.AttributeName, viewModel.Email);

            var result = await _cognitoUserManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Confirm");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }


            return View(viewModel);
        }


        [HttpGet]
        public IActionResult Confirm()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(ConfirmViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _cognitoUserManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("UserNotFound", "User doesn't exist.");

                return View(model);
            }

            var result = await _cognitoUserManager.ConfirmSignUpAsync(user, model.Code, false);
            
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return View(model);
        }
    }
}