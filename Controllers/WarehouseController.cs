using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Warehouse;
using StationeryWarehouse.Services.Warehouse;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class WarehouseController : Controller
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(
        IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    // ============================================================
    // S03 - KHO & VỊ TRÍ
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        long? warehouseId,
        int page = 1)
    {
        const int pageSize = 20;

        if (page < 1)
        {
            page = 1;
        }

        var warehouses =
            await _warehouseService.GetWarehousesAsync();

        var locationResult =
            await _warehouseService.GetLocationsAsync(
                warehouseId,
                page,
                pageSize);

        locationResult.Warehouses = warehouses;

        ViewData["Title"] = "Kho & vị trí";

        ViewBag.SelectedWarehouseId =
            warehouseId;

        return View(locationResult);
    }


    // ============================================================
    // TẠO KHO
    // ============================================================

    [HttpGet]
    public IActionResult CreateWarehouse()
    {
        ViewData["Title"] = "Tạo kho";

        return View(
            new WarehouseFormViewModel());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWarehouse(
        WarehouseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Tạo kho";

            return View(model);
        }

        try
        {
            await _warehouseService
                .CreateWarehouseAsync(model);

            TempData["SuccessMessage"] =
                "Tạo kho thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            ViewData["Title"] = "Tạo kho";

            return View(model);
        }
    }


    // ============================================================
    // SỬA KHO
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> EditWarehouse(long id)
    {
        var model =
            await _warehouseService
                .GetWarehouseFormAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        ViewData["Title"] = "Sửa kho";

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWarehouse(
        WarehouseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Sửa kho";

            return View(model);
        }

        try
        {
            await _warehouseService
                .UpdateWarehouseAsync(model);

            TempData["SuccessMessage"] =
                "Cập nhật kho thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            ViewData["Title"] = "Sửa kho";

            return View(model);
        }
    }


    // ============================================================
    // TẠO VỊ TRÍ
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> CreateLocation(
        long? warehouseId)
    {
        var warehouses =
            await _warehouseService
                .GetActiveWarehousesAsync();

        ViewBag.Warehouses = warehouses;

        var model = new LocationFormViewModel
        {
            WarehouseId = warehouseId ?? 0
        };

        if (warehouseId.HasValue)
        {
            ViewBag.ParentLocations =
                await _warehouseService
                    .GetActiveParentLocationsAsync(
                        warehouseId.Value);
        }
        else
        {
            ViewBag.ParentLocations =
                new List<LocationListViewModel>();
        }

        ViewData["Title"] = "Tạo vị trí";

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLocation(
        LocationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareLocationForm(model);

            ViewData["Title"] = "Tạo vị trí";

            return View(model);
        }

        try
        {
            await _warehouseService
                .CreateLocationAsync(model);

            TempData["SuccessMessage"] =
                "Tạo vị trí thành công.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    warehouseId = model.WarehouseId
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await PrepareLocationForm(model);

            ViewData["Title"] = "Tạo vị trí";

            return View(model);
        }
    }


    // ============================================================
    // SỬA VỊ TRÍ
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> EditLocation(long id)
    {
        var model =
            await _warehouseService
                .GetLocationFormAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        await PrepareLocationForm(
            model,
            id);

        ViewData["Title"] = "Sửa vị trí";

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLocation(
        LocationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PrepareLocationForm(
                model,
                model.Id);

            ViewData["Title"] = "Sửa vị trí";

            return View(model);
        }

        try
        {
            await _warehouseService
                .UpdateLocationAsync(model);

            TempData["SuccessMessage"] =
                "Cập nhật vị trí thành công.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    warehouseId = model.WarehouseId
                });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            await PrepareLocationForm(
                model,
                model.Id);

            ViewData["Title"] = "Sửa vị trí";

            return View(model);
        }
    }


    // ============================================================
    // CHUẨN BỊ DỮ LIỆU CHO FORM VỊ TRÍ
    // ============================================================

    private async Task PrepareLocationForm(
        LocationFormViewModel model,
        long? excludeId = null)
    {
        ViewBag.Warehouses =
            await _warehouseService
                .GetActiveWarehousesAsync();

        if (model.WarehouseId > 0)
        {
            ViewBag.ParentLocations =
                await _warehouseService
                    .GetActiveParentLocationsAsync(
                        model.WarehouseId,
                        excludeId);
        }
        else
        {
            ViewBag.ParentLocations =
                new List<LocationListViewModel>();
        }
    }
}