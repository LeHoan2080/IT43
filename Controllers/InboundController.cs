using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Inbound;
using StationeryWarehouse.Services.Inbound;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class InboundController : Controller
{
    private readonly IInboundService _inboundService;

    public InboundController(IInboundService inboundService)
    {
        _inboundService = inboundService;
    }

    // =========================================================
    // GET: /Inbound
    // Danh sách phiếu nhập
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        InboundFilterViewModel filter)
    {
        filter.PageSize = 20;

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        var model =
            await _inboundService
                .GetInboundReceiptsAsync(filter);

        ViewData["Title"] = "Nhập kho";

        return View(model);
    }

    // =========================================================
    // GET: /Inbound/Create
    // Tạo phiếu nhập
    // =========================================================

    [HttpGet]
    [Authorize(
        Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create()
    {
        var warehouses =
            await _inboundService
                .GetActiveWarehousesAsync();

        var model = new InboundFormViewModel
        {
            ReceiptDate = DateTime.Today,
            Status = "DRAFT",
            Warehouses = warehouses,
            Lines = new List<InboundLineViewModel>()
        };

        ViewData["Title"] = "Tạo phiếu nhập";

        return View(model);
    }

    // =========================================================
    // POST: /Inbound/Create
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create(
        InboundFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Warehouses =
                await _inboundService
                    .GetActiveWarehousesAsync();

            ViewData["Title"] = "Tạo phiếu nhập";

            return View(model);
        }

        try
        {
            var currentUserId =
                GetCurrentUserId();

            await _inboundService
                .CreateInboundAsync(
                    model,
                    currentUserId);

            TempData["SuccessMessage"] =
                "Tạo phiếu nhập thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Warehouses =
                await _inboundService
                    .GetActiveWarehousesAsync();

            ViewData["Title"] = "Tạo phiếu nhập";

            return View(model);
        }
    }

    // =========================================================
    // GET: /Inbound/Edit/{id}
    // Chỉ DRAFT mới được sửa
    // =========================================================

    [HttpGet]
    [Authorize(
        Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Edit(long id)
    {
        var model =
            await _inboundService
                .GetInboundFormAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        if (model.Status != "DRAFT")
        {
            TempData["ErrorMessage"] =
                "Phiếu đã chuyển trạng thái và không thể sửa.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }

        ViewData["Title"] =
            $"Sửa phiếu {model.ReceiptNo}";

        return View(model);
    }

    // =========================================================
    // POST: /Inbound/Edit
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Edit(
        InboundFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Warehouses =
                await _inboundService
                    .GetActiveWarehousesAsync();

            ViewData["Title"] =
                $"Sửa phiếu {model.ReceiptNo}";

            return View(model);
        }

        try
        {
            await _inboundService
                .UpdateInboundAsync(model);

            TempData["SuccessMessage"] =
                "Cập nhật phiếu nhập thành công.";

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
                await _inboundService
                    .GetActiveWarehousesAsync();

            ViewData["Title"] =
                $"Sửa phiếu {model.ReceiptNo}";

            return View(model);
        }
    }

    // =========================================================
    // POST: /Inbound/StartReceiving
    // DRAFT -> RECEIVING
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> StartReceiving(
        long id)
    {
        try
        {
            await _inboundService
                .StartReceivingAsync(id);

            TempData["SuccessMessage"] =
                "Phiếu đã chuyển sang trạng thái đang nhận hàng.";

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
    // POST: /Inbound/Complete
    // RECEIVING -> DONE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Complete(long id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            await _inboundService.CompleteInboundAsync(
                id,
                currentUserId);

            TempData["SuccessMessage"] =
                "Phiếu nhập đã được hoàn tất.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }

    // =========================================================
    // POST: /Inbound/Cancel
    // DRAFT/RECEIVING -> CANCELLED
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles = "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Cancel(
        long id)
    {
        try
        {
            var currentUserId =
                GetCurrentUserId();

            await _inboundService
                .CancelInboundAsync(
                    id,
                    currentUserId);

            TempData["SuccessMessage"] =
                "Đã hủy phiếu nhập.";

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
    // GET: /Inbound/Detail/{id}
    // Xem chi tiết
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var model =
            await _inboundService
                .GetInboundFormAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        ViewData["Title"] =
            $"Phiếu nhập {model.ReceiptNo}";

        return View(model);
    }

    // =========================================================
    // Lấy User ID từ JWT Claim
    // =========================================================

    private long GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            throw new UnauthorizedAccessException(
                "Không xác định được người dùng hiện tại.");
        }

        if (!long.TryParse(
            userIdClaim,
            out var userId))
        {
            throw new UnauthorizedAccessException(
                "User ID không hợp lệ.");
        }

        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> SearchProduct(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Json(new
            {
                success = false,
                message = "Vui lòng nhập từ khóa tìm kiếm."
            });
        }

        var result =
            await _inboundService.SearchProductsAsync(
                keyword.Trim());

        return Json(new
        {
            success = true,
            data = result
        });
    }

    [HttpGet]
    public async Task<IActionResult> SearchLocation(
        long warehouseId,
        string keyword)
    {
        if (warehouseId <= 0)
        {
            return Json(new
            {
                success = false,
                message = "Kho nhận không hợp lệ."
            });
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Json(new
            {
                success = false,
                message = "Vui lòng nhập mã hoặc Barcode vị trí."
            });
        }

        var result =
            await _inboundService.SearchLocationsAsync(
                warehouseId,
                keyword.Trim());

        return Json(new
        {
            success = true,
            data = result
        });
    }
}