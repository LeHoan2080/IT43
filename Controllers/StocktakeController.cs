using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Stocktake;
using StationeryWarehouse.Services.Stocktake;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class StocktakeController : Controller
{
    private readonly IStocktakeService _stocktakeService;

    private const string MutationRoles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR";

    public StocktakeController(IStocktakeService stocktakeService)
    {
        _stocktakeService = stocktakeService;
    }

    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        StocktakeFilterViewModel filter)
    {
        try
        {
            var model =
                await _stocktakeService.GetStocktakesAsync(filter);

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return View(
                new StocktakeIndexViewModel
                {
                    Filter = filter,
                    Warehouses =
                        await _stocktakeService
                            .GetActiveWarehousesAsync()
                });
        }
    }

    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> Create()
    {
        var model =
            new StocktakeFormViewModel
            {
                StocktakeDate = DateTime.Today,
                Status = "DRAFT",
                Warehouses =
                    await _stocktakeService
                        .GetActiveWarehousesAsync()
            };

        return View(model);
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> Create(
        StocktakeFormViewModel model)
    {
        try
        {
            await _stocktakeService.CreateStocktakeAsync(
                model,
                GetCurrentUserId());

            TempData["Success"] =
                "Tạo phiếu kiểm kê thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi tạo phiếu kiểm kê.";
        }

        model.Warehouses =
            await _stocktakeService
                .GetActiveWarehousesAsync();

        return View(model);
    }

    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> Edit(long id)
    {
        var model =
            await _stocktakeService
                .GetStocktakeFormAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> Edit(
        StocktakeFormViewModel model)
    {
        try
        {
            await _stocktakeService
                .UpdateStocktakeAsync(model);

            TempData["Success"] =
                "Cập nhật phiếu kiểm kê thành công.";

            return RedirectToAction(
                nameof(Detail),
                new { id = model.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi cập nhật phiếu kiểm kê.";
        }

        model.Warehouses =
            await _stocktakeService
                .GetActiveWarehousesAsync();

        return View(model);
    }

    // =========================================================
    // DETAIL
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var model =
            await _stocktakeService
                .GetStocktakeFormAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    // =========================================================
    // START COUNTING
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> StartCounting(
        long id)
    {
        try
        {
            await _stocktakeService
                .StartCountingAsync(id);

            TempData["Success"] =
                "Đã bắt đầu kiểm kê.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi bắt đầu kiểm kê.";
        }

        return RedirectToAction(
            nameof(Detail),
            new { id });
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> Complete(
        long id)
    {
        try
        {
            await _stocktakeService
                .CompleteStocktakeAsync(
                    id,
                    GetCurrentUserId());

            TempData["Success"] =
                "Hoàn tất kiểm kê và cập nhật tồn kho thành công.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi hoàn tất kiểm kê.";
        }

        return RedirectToAction(
            nameof(Detail),
            new { id });
    }

    // =========================================================
    // CANCEL
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = MutationRoles)]
    public async Task<IActionResult> Cancel(
        long id)
    {
        try
        {
            await _stocktakeService
                .CancelStocktakeAsync(id);

            TempData["Success"] =
                "Đã hủy phiếu kiểm kê.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi hủy phiếu kiểm kê.";
        }

        return RedirectToAction(
            nameof(Detail),
            new { id });
    }

    // =========================================================
    // SEARCH PRODUCT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> SearchProduct(
        string keyword)
    {
        var result =
            await _stocktakeService
                .SearchProductsAsync(keyword);

        return Json(result);
    }

    // =========================================================
    // SEARCH LOCATION
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> SearchLocation(
        long warehouseId,
        string keyword)
    {
        var result =
            await _stocktakeService
                .SearchLocationsAsync(
                    warehouseId,
                    keyword);

        return Json(result);
    }

    // =========================================================
    // CURRENT USER
    // =========================================================

    private long GetCurrentUserId()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!long.TryParse(
                userId,
                out var currentUserId))
        {
            throw new UnauthorizedAccessException(
                "Không xác định được người dùng hiện tại.");
        }

        return currentUserId;
    }
}