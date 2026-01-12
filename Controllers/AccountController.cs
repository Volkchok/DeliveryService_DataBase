using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyclinicDB.Services;
using PolyclinicDB.ViewModels;

namespace PolyclinicDB.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Учетная запись заблокирована");
                return View(model);
            }

            // Создаем claims для аутентификации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            if (user.PatientId.HasValue)
                claims.Add(new Claim("PatientId", user.PatientId.Value.ToString()));
            if (user.DoctorId.HasValue)
                claims.Add(new Claim("DoctorId", user.DoctorId.Value.ToString()));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Пользователь {Username} вошел в систему.", user.Username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем, существует ли пользователь
            if (await _authService.UserExistsAsync(model.Username, model.Email))
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким именем или email уже существует");
                return View(model);
            }

            try
            {
                var user = await _authService.RegisterAsync(model);

                // Автоматически входим после регистрации
                var loginModel = new LoginViewModel
                {
                    Username = model.Username,
                    Password = model.Password,
                    RememberMe = false
                };

                return await Login(loginModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Пользователь вышел из системы.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}