using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Settings;
using StationeryWarehouse.Services.Settings;

namespace StationeryWarehouse.Controllers;

[Authorize(Roles = "ADMIN")]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;

    public SettingsController(
        ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    // =========================================================
    // S08 - CÀI ĐẶT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        int usersPage = 1,
        int rolesPage = 1)
    {
        var model =
            await _settingsService.GetSettingsAsync(
                usersPage,
                rolesPage);

        return View(model);
    }

    // =========================================================
    // THÊM NGƯỜI DÙNG
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        var settings =
            await _settingsService.GetSettingsAsync();

        var model = new UserEditViewModel
        {
            IsActive = true,
            Roles = settings.Roles
        };

        return View("EditUser", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(
        UserEditViewModel model,
        string password)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(model);

            return View("EditUser", model);
        }

        try
        {
            await _settingsService.CreateUserAsync(
                model,
                password);

            TempData["SuccessMessage"] =
                "Đã tạo người dùng thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadRolesAsync(model);

            return View("EditUser", model);
        }
    }

    // =========================================================
    // SỬA NGƯỜI DÙNG
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> EditUser(long id)
    {
        var model =
            await _settingsService.GetUserAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(
        UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(model);

            return View(model);
        }

        try
        {
            await _settingsService.UpdateUserAsync(model);

            TempData["SuccessMessage"] =
                "Đã cập nhật người dùng thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await LoadRolesAsync(model);

            return View(model);
        }
    }

    // =========================================================
    // KHÓA / MỞ KHÓA NGƯỜI DÙNG
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(long id)
    {
        try
        {
            await _settingsService.ToggleUserStatusAsync(id);

            TempData["SuccessMessage"] =
                "Đã cập nhật trạng thái người dùng.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // ĐỔI MẬT KHẨU
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> ChangePassword(long id)
    {
        var model =
            await _settingsService
                .GetChangePasswordAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _settingsService
                .ChangePasswordAsync(model);

            TempData["SuccessMessage"] =
                "Đã đổi mật khẩu thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }

    // =========================================================
    // HELPER
    // =========================================================

    private async Task LoadRolesAsync(
        UserEditViewModel model)
    {
        var settings =
            await _settingsService.GetSettingsAsync();

        model.Roles = settings.Roles;
    }
}