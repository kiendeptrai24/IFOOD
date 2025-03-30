using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using iFood.Data;
using iFood.Models;
using iFood.ViewModels;
using iFood;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using iFood.Interfaces;

namespace MyMvcApp;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IPhotoService _photoService;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IPhotoService photoService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _photoService = photoService;
    }
    [HttpGet]
    public IActionResult Login()
    {
        var reponse = new LoginViewModel();
        
        return View(reponse);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginVM)
    {   
        if(!ModelState.IsValid) return View(loginVM);

        var user = await _userManager.FindByEmailAsync(loginVM.EmailAddress);
        if(user != null)
        {
            // User is found, check passwork
            var PasswordCheck = await _userManager.CheckPasswordAsync(user, loginVM.Password);
            if(PasswordCheck)
            {
                // Password correct, sign in
                var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
                if(result.Succeeded)
                    return RedirectToAction("Index", "Home");
            }
            // password is incorrect
            TempData["Error"] = "Wrong credentials. please, try again";
            return View(loginVM);
        }
        //User not Found
        TempData["Error"] = "Wrong credentials. please, try again";
        return View(loginVM);
    }
    public async Task LoginByGoogle()
    {
        await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme
        , new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse")

        });
    }
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        if (!result.Succeeded)
        {
            TempData["error"] = "Đăng nhập thất bại!";
            return RedirectToAction("Login", "Account");
        }

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims.Select(claim => new
        {
            claim.Issuer,
            claim.OriginalIssuer,
            claim.Type,
            claim.Value
        });
        //get email claims
        
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var userName = email.Split('@')[0];

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser == null)
        {
            var passwordHasher = new PasswordHasher<AppUser>();
            var hashedPassword = passwordHasher.HashPassword(null,"123456789"); 

            var newUser = new AppUser {UserName = userName,Email = email};

            newUser.PasswordHash = hashedPassword;
            var createUserResult = await _userManager.CreateAsync(newUser);
            if(!createUserResult.Succeeded)
            {
                TempData["error"] = "Sign up account failed, please try again";
                return RedirectToAction("Login","Account");
            }       
            else
            {
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                TempData["SuccessMessage"] = "Sign up account successfully!";
                return RedirectToAction("Index", "Home");
            }
        }
        else
        {
            await _signInManager.SignInAsync(existingUser, isPersistent: false);
            return RedirectToAction("Index","Home"); 
        }
        

    // Debug: return Json(claims);
}


    [HttpGet]
    public IActionResult Register()
    {
        var response = new RegisterViewModel();
        return View(response);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (!ModelState.IsValid) return View(registerViewModel);

        var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);
        if (user != null)
        {
            TempData["Error"] = "This email address is already in use";
            return View(registerViewModel);
        }

        var newUser = new AppUser()
        {
            Email = registerViewModel.EmailAddress,
            UserName = registerViewModel.EmailAddress
        };
        var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

        if (!newUserResponse.Succeeded)
        {
            TempData["Error"] = "Your password must be at least 6 characters Include uppercase, lowercase, numbers and special characters";
            return View(registerViewModel);
        }

        await _userManager.AddToRoleAsync(newUser, UserRoles.User);
        return RedirectToAction("Index", "Home");
    }
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login","Account");
    }
    
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var model = new ProfileUserViewModel
        {
            Email = user.Email,
            Name = user.UserName,
            Phone = user.PhoneNumber,
            AvatarUrl = user.ProfileImageUrl
        };

        return View(model);
    }
    [HttpPost]
    public async Task<IActionResult> Profile(ProfileUserViewModel profileUserVM)
    {

        if (!ModelState.IsValid)
        {
            return View(profileUserVM);
        }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Cập nhật thông tin
            user.Email = profileUserVM.Email;
            user.UserName = profileUserVM.Name;
            user.PhoneNumber = profileUserVM.Phone;

            // Nếu có mật khẩu mới, thực hiện đổi mật khẩu an toàn
            if (!string.IsNullOrEmpty(profileUserVM.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, profileUserVM.Password);
                if (!resetResult.Succeeded)
                {
                    foreach (var error in resetResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(profileUserVM);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(profileUserVM);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
    }
    [HttpGet]
    public IActionResult ForgetPassword()
    {
        return View();
    }

   [HttpPost]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Kiểm tra email có tồn tại không
        var user = await _userManager.FindByEmailAsync(model.EmailAddress);
        if (user == null)
        {
            TempData["Error"] = "Email address not found.";
            return View(model);
        }

        // Kiểm tra số điện thoại có khớp với tài khoản không
        if (user.PhoneNumber != model.PhoneNumber)
        {
            TempData["Error"] = "Phone number does not match our records.";
            return View(model);
        }
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

        if (result.Succeeded)
        {
            TempData["Success"] = "Password has been changed successfully.";
            return RedirectToAction("Login", "Account");
        }

        TempData["Error"] = "Failed to reset password. Please try again.";
        return View(model);

    }
    [HttpPost]
    public async Task<IActionResult> ChangeAvatar(ChangeAvatarViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found.");

        // Upload ảnh mới
        var photoResult = await _photoService.AddPhotoAsync(model.Avatar);
        if (photoResult.Error != null)
        {
            ModelState.AddModelError("Avatar", "Photo upload failed");
            return View(model);
        }

        // Xóa ảnh cũ nếu có
        if (!string.IsNullOrEmpty(user.ProfileImageUrl))
        {
            await _photoService.DeletePhotoAsync(user.ProfileImageUrl);
        }

        // Cập nhật đường dẫn ảnh
        user.ProfileImageUrl = photoResult.Url.ToString();
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Avatar updated successfully!";
        return RedirectToAction("Profile", "Account");
    }

}
