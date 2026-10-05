using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Transfer;
using StationeryWarehouse.Services.Transfer;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class TransferController : Controller
{
    private readonly ITransferService _transferService;

    public TransferController(
        ITransferService transferService)
    {
        _transferService = transferService;
    }


    // =========================================================
    // INDEX
    // GET: /Transfer
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        TransferFilterViewModel filter)
    {
        try
        {
            var model =
                await _transferService
                    .GetTransfersAsync(filter);

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                ex.Message;

            return View(
                new TransferIndexViewModel
                {
                    Items = new List<
                        TransferListItemViewModel>(),

                    Filter = filter,

                    Warehouses =
                        await _transferService
                            .GetActiveWarehousesAsync()
                });
        }
    }


    // =========================================================
    // CREATE - GET
    // GET: /Transfer/Create
    // =========================================================

    [HttpGet]
    [Authorize(
        Roles =
            "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create()
    {
        var warehouses =
            await _transferService
                .GetActiveWarehousesAsync();

        var model =
            new TransferFormViewModel
            {
                TransferDate = DateTime.Today,

                Status = "DRAFT",

                Warehouses = warehouses,

                Lines =
                    new List<TransferLineViewModel>
                    {
                        new TransferLineViewModel()
                    }
            };

        return View(model);
    }


    // =========================================================
    // CREATE - POST
    // POST: /Transfer/Create
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
            "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Create(
        TransferFormViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Warehouses =
                    await _transferService
                        .GetActiveWarehousesAsync();

                return View(model);
            }


            var currentUserId =
                GetCurrentUserId();


            await _transferService
                .CreateTransferAsync(
                    model,
                    currentUserId);


            TempData["Success"] =
                "Tạo phiếu điều chuyển thành công.";


            /*
             * Nếu CreateTransferAsync hiện tại
             * trả về Task<long>, nên đổi đoạn này
             * thành redirect trực tiếp đến Detail.
             *
             * Tuy nhiên service hiện tại của bạn
             * đang trả Task nên chưa có Id ở đây.
             */

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Warehouses =
                await _transferService
                    .GetActiveWarehousesAsync();

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                "Có lỗi xảy ra khi tạo phiếu: "
                + ex.Message);

            model.Warehouses =
                await _transferService
                    .GetActiveWarehousesAsync();

            return View(model);
        }
    }


    // =========================================================
    // EDIT - GET
    // GET: /Transfer/Edit/5
    // =========================================================

    [HttpGet]
    [Authorize(
        Roles =
            "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Edit(
        long id)
    {
        var model =
            await _transferService
                .GetTransferFormAsync(id);

        if (model == null)
        {
            TempData["Error"] =
                "Không tìm thấy phiếu điều chuyển.";

            return RedirectToAction(
                nameof(Index));
        }


        if (model.Status == "DONE")
        {
            TempData["Error"] =
                "Phiếu đã hoàn tất, không thể sửa.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }


        if (model.Status == "CANCELLED")
        {
            TempData["Error"] =
                "Phiếu đã hủy, không thể sửa.";

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }


        return View(model);
    }


    // =========================================================
    // EDIT - POST
    // POST: /Transfer/Edit
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(
        Roles =
            "ADMIN,WAREHOUSE_MANAGER,WAREHOUSE_OPERATOR")]
    public async Task<IActionResult> Edit(
        TransferFormViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                model.Warehouses =
                    await _transferService
                        .GetActiveWarehousesAsync();

                return View(model);
            }


            await _transferService
                .UpdateTransferAsync(model);


            TempData["Success"] =
                "Cập nhật phiếu điều chuyển thành công.";


            return RedirectToAction(
                nameof(Detail),
                new
                {
                    id = model.Id
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Warehouses =
                await _transferService
                    .GetActiveWarehousesAsync();

            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                "Có lỗi xảy ra khi cập nhật phiếu: "
                + ex.Message);

            model.Warehouses =
                await _transferService
                    .GetActiveWarehousesAsync();

            return View(model);
        }
    }


    // =========================================================
    // DETAIL - GET
    // GET: /Transfer/Detail/5
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Detail(
        long id)
    {
        var model =
            await _transferService
                .GetTransferFormAsync(id);

        if (model == null)
        {
            TempData["Error"] =
                "Không tìm thấy phiếu điều chuyển.";

            return RedirectToAction(
                nameof(Index));
        }


        return View(model);
    }


    // =========================================================
    // COMPLETE
    // POST: /Transfer/Complete
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


            await _transferService
                .CompleteTransferAsync(
                    id,
                    currentUserId);


            TempData["Success"] =
                "Hoàn tất phiếu điều chuyển thành công.";


            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi hoàn tất phiếu: "
                + ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }


    // =========================================================
    // CANCEL
    // POST: /Transfer/Cancel
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
            await _transferService
                .CancelTransferAsync(id);


            TempData["Success"] =
                "Hủy phiếu điều chuyển thành công.";


            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] =
                ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] =
                "Có lỗi xảy ra khi hủy phiếu: "
                + ex.Message;

            return RedirectToAction(
                nameof(Detail),
                new { id });
        }
    }


    // =========================================================
    // SEARCH PRODUCT
    // GET: /Transfer/SearchProduct?keyword=...
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> SearchProduct(
        string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Json(
                new List<
                    TransferProductSearchViewModel>());
        }


        var products =
            await _transferService
                .SearchProductsAsync(
                    keyword);


        return Json(products);
    }


    // =========================================================
    // SEARCH SOURCE LOCATION
    // GET: /Transfer/SearchSourceLocation
    // =========================================================

    [HttpGet]
    public async Task<IActionResult>
        SearchSourceLocation(
            long warehouseId,
            long productId,
            string keyword)
    {
        if (warehouseId <= 0 ||
            productId <= 0)
        {
            return Json(
                new List<
                    TransferLocationSearchViewModel>());
        }


        var locations =
            await _transferService
                .SearchSourceLocationsAsync(
                    warehouseId,
                    productId,
                    keyword ?? string.Empty);


        return Json(locations);
    }


    // =========================================================
    // SEARCH DESTINATION LOCATION
    // GET: /Transfer/SearchDestinationLocation
    // =========================================================

    [HttpGet]
    public async Task<IActionResult>
        SearchDestinationLocation(
            long warehouseId,
            string keyword)
    {
        if (warehouseId <= 0)
        {
            return Json(
                new List<
                    TransferLocationSearchViewModel>());
        }


        var locations =
            await _transferService
                .SearchDestinationLocationsAsync(
                    warehouseId,
                    keyword ?? string.Empty);


        return Json(locations);
    }


    // =========================================================
    // CURRENT USER
    // =========================================================

    private long GetCurrentUserId()
    {
        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            throw new UnauthorizedAccessException(
                "Không xác định được người dùng hiện tại.");
        }


        if (!long.TryParse(
                claim.Value,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "ID người dùng không hợp lệ.");
        }


        return userId;
    }
}