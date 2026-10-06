using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Supplier;
using StationeryWarehouse.Services.Supplier;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class SupplierController : Controller
{
    private readonly ISupplierService _supplierService;

    public SupplierController(
        ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    // =========================================================
    // INDEX
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        SupplierFilterViewModel filter)
    {
        if (filter.Page < 1)
            filter.Page = 1;

        var model =
            await _supplierService
                .GetSuppliersAsync(filter);

        return View(model);
    }

    // =========================================================
    // DETAIL
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var model =
            await _supplierService
                .GetSupplierDetailAsync(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    // =========================================================
    // CREATE - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER")]
    public IActionResult Create()
    {
        return View(
            new SupplierFormViewModel());
    }

    // =========================================================
    // CREATE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Create(
        SupplierFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _supplierService
                .CreateSupplierAsync(model);

            TempData["SuccessMessage"] =
                "Thêm nhà cung cấp/NXB thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }

    // =========================================================
    // EDIT - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Edit(long id)
    {
        var model =
            await _supplierService
                .GetSupplierForEditAsync(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    // =========================================================
    // EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Edit(
        SupplierFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _supplierService
                .UpdateSupplierAsync(model);

            TempData["SuccessMessage"] =
                "Cập nhật nhà cung cấp/NXB thành công.";

            return RedirectToAction(
                nameof(Detail),
                new { id = model.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            return View(model);
        }
    }

    // =========================================================
    // DEACTIVATE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            await _supplierService
                .DeactivateSupplierAsync(id);

            TempData["SuccessMessage"] =
                "Đã ngừng sử dụng nhà cung cấp/NXB.";

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
    // ACTIVATE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            await _supplierService
                .ActivateSupplierAsync(id);

            TempData["SuccessMessage"] =
                "Đã kích hoạt nhà cung cấp/NXB.";

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
}