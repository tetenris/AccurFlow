using AccuFlow.Application.Features.Account.Commands;
using AccuFlow.Models.Account;
using AccuFlow.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AccuFlow.Controllers
{
    public class AccountController : BaseController
    {
        private readonly ISender _mediator;

        public AccountController(ISender mediator, IBaseService baseService) : base(baseService)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username dan password wajib diisi.");
                return View();
            }

            var result = await _mediator.Send(new LoginCommand(username, password));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Message);
                return View();
            }

            var passwordExpired = result.PasswordExpiresAt.HasValue && result.PasswordExpiresAt.Value <= DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim("UserId", result.UserId.ToString()),
                new Claim("UserName", result.UserName),
                new Claim("Email", result.Email),
                new Claim("RoleName", result.RoleName ?? "User"),
                new Claim(ClaimTypes.Name, result.UserName),
                new Claim(ClaimTypes.Email, result.Email),
                new Claim("FullName", result.FullName),
                new Claim("RoleId", result.RoleId.ToString()),
                new Claim("PasswordExpired", passwordExpired.ToString().ToLowerInvariant()),
                new Claim(ClaimTypes.Role, result.RoleName ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (passwordExpired)
            {
                TempData["PasswordExpiredMessage"] = "Password anda sudah lebih dari 1 bulan. Silakan ganti password untuk melanjutkan.";
                return RedirectToAction(nameof(ChangePassword));
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _mediator.Send(new ForgotPasswordCommand(model.Email));
            ViewBag.SuccessMessage = "Jika email terdaftar, instruksi reset password akan dikirim setelah layanan email aktif.";
            return View(new ForgotPasswordViewModel());
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userIdValue = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrWhiteSpace(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return RedirectToAction(nameof(Login));
                }

                var wasPasswordExpired = string.Equals(User.FindFirst("PasswordExpired")?.Value, "true", StringComparison.OrdinalIgnoreCase);
                await _mediator.Send(new ChangePasswordCommand(userId, model.CurrentPassword, model.NewPassword));
                await RefreshPasswordExpiredClaimAsync(false);
                TempData["SuccessMessage"] = "Password berhasil diperbarui.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private async Task RefreshPasswordExpiredClaimAsync(bool passwordExpired)
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return;
            }

            var existingClaim = identity.FindFirst("PasswordExpired");
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
            }

            identity.AddClaim(new Claim("PasswordExpired", passwordExpired.ToString().ToLowerInvariant()));
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                });
        }
    }
}
