using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.DTOs.Auth;
using StationeryWarehouse.Services;

namespace StationeryWarehouse.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(
        IAuthService authService,
        IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    // ========================================
    // LOGIN - GET
    // ========================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        return View();
    }

    // ========================================
    // LOGIN - POST
    // ========================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user =
            await _authService.LoginAsync(
                model.Username,
                model.Password
            );

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Tên đăng nhập hoặc mật khẩu không chính xác."
            );

            return View(model);
        }

        var token =
            _jwtService.GenerateToken(user);

        Response.Cookies.Append(
            "access_token",
            token,
            new CookieOptions
            {
                HttpOnly = true,

                Secure = true,

                SameSite =
                    SameSiteMode.Strict,

                Expires =
                    DateTimeOffset.UtcNow.AddMinutes(30),

                IsEssential = true
            }
        );

        return RedirectToAction(
            "Index",
            "Home"
        );
    }

    // ========================================
    // REGISTER - GET
    // ========================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // ========================================
    // REGISTER - POST
    // ========================================

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterRequest model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result =
            await _authService.RegisterAsync(
                model.Username,
                model.FullName,
                model.Password
            );

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message
            );

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Đăng ký thành công. Vui lòng đăng nhập.";

        return RedirectToAction(
            nameof(Login)
        );
    }

    // ========================================
    // LOGOUT
    // ========================================

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
            "access_token"
        );

        return RedirectToAction(
            nameof(Login)
        );
    }

    // ========================================
    // ACCESS DENIED
    // ========================================

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}