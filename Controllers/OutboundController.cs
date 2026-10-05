using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Outbound;
using StationeryWarehouse.Services.Outbound;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class OutboundController : Controller
{
    private readonly IOutboundService _service;

    public OutboundController(
        IOutboundService service)
    {
        _service = service;
    }


    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        OutboundFilterViewModel filter)
    {
        var model =
            await _service
                .GetOutboundIssuesAsync(filter);

        return View(model);
    }


    // =========================================================
    // CREATE GET
    // =========================================================

    [HttpGet]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create()
    {
        var model =
            new OutboundFormViewModel
            {
                IssueDate = DateTime.Today,
                Status = "DRAFT",
                Warehouses =
                    await _service
                        .GetActiveWarehousesAsync()
            };

        return View(model);
    }


    // =========================================================
    // CREATE POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create(
        OutboundFormViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Warehouses =
                    await _service
                        .GetActiveWarehousesAsync();

                return View(model);
            }

            var currentUserId =
                GetCurrentUserId();

            await _service
                .CreateOutboundAsync(
                    model,
                    currentUserId);

            TempData["SuccessMessage"] =
                "Tạo phiếu xuất thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Warehouses =
                await _service
                    .GetActiveWarehousesAsync();

            return View(model);
        }
    }


    // =========================================================
    // EDIT GET
    // =========================================================

    [HttpGet]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Edit(long id)
    {
        var model =
            await _service
                .GetOutboundFormAsync(id);

        if (model == null)
            return NotFound();

        if (model.Status != "DRAFT")
        {
            TempData["ErrorMessage"] =
                "Chỉ được sửa phiếu xuất ở trạng thái DRAFT.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }

        return View(model);
    }


    // =========================================================
    // EDIT POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Edit(
        OutboundFormViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Warehouses =
                    await _service
                        .GetActiveWarehousesAsync();

                return View(model);
            }

            await _service
                .UpdateOutboundAsync(model);

            TempData["SuccessMessage"] =
                "Cập nhật phiếu xuất thành công.";

            return RedirectToAction(
                nameof(Detail),
                new { id = model.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Warehouses =
                await _service
                    .GetActiveWarehousesAsync();

            return View(model);
        }
    }


    // =========================================================
    // START PICKING
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> StartPicking(
        long id)
    {
        try
        {
            await _service
                .StartPickingAsync(id);

            TempData["SuccessMessage"] =
                "Đã bắt đầu lấy hàng.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Complete(
        long id)
    {
        try
        {
            var currentUserId =
                GetCurrentUserId();

            await _service
                .CompleteOutboundAsync(
                    id,
                    currentUserId);

            TempData["SuccessMessage"] =
                "Hoàn tất xuất kho thành công.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }


    // =========================================================
    // CANCEL
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
        "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Cancel(
        long id)
    {
        try
        {
            var currentUserId =
                GetCurrentUserId();

            await _service
                .CancelOutboundAsync(
                    id,
                    currentUserId);

            TempData["SuccessMessage"] =
                "Đã hủy phiếu xuất.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] =
                ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }


    // =========================================================
    // DETAIL
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var model =
            await _service
                .GetOutboundFormAsync(id);

        if (model == null)
            return NotFound();

        return View(model);
    }


    // =========================================================
    // SEARCH PRODUCT
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> SearchProduct(
        string keyword)
    {
        var result =
            await _service
                .SearchProductsAsync(keyword);

        return Json(result);
    }


    // =========================================================
    // SEARCH LOCATION
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> SearchLocation(
        long warehouseId,
        long productId,
        string keyword)
    {
        var result =
            await _service
                .SearchLocationsAsync(
                    warehouseId,
                    productId,
                    keyword);

        return Json(result);
    }


    // =========================================================
    // CURRENT USER
    // =========================================================

    private long GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!long.TryParse(
                value,
                out var userId))
        {
            throw new InvalidOperationException(
                "Không xác định được người dùng hiện tại.");
        }

        return userId;
    }
}