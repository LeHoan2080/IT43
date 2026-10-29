using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Inventory;
using StationeryWarehouse.Services.Inventory;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryPdfService _inventoryPdfService;

    public InventoryController(
        IInventoryService inventoryService,
        IInventoryPdfService inventoryPdfService)
    {
        _inventoryService = inventoryService;
        _inventoryPdfService = inventoryPdfService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        InventoryFilterViewModel filter)
    {
        filter.PageSize =
            StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        var model =
            await _inventoryService
                .GetInventoryAsync(filter);

        ViewData["Title"] = "Tồn kho";

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(
        InventoryFilterViewModel filter)
    {
        filter.Page = 1;

        var model =
            await _inventoryService
                .GetInventoryAsync(filter, includeAllResults: true);

        var pdf = _inventoryPdfService.GenerateInventoryPdf(model);
        var fileName = $"inventory-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";

        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var product =
            await _inventoryService
                .GetProductInventoryAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        ViewData["Title"] =
            product.ProductName;

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> Movements(
        InventoryMovementFilterViewModel filter)
    {
        var model =
            await _inventoryService
                .GetMovementReportAsync(filter);

        ViewData["Title"] = "Dịch chuyển tồn kho";

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportMovementsPdf(
        InventoryMovementFilterViewModel filter)
    {
        filter.Page = 1;

        var model =
            await _inventoryService
                .GetMovementReportAsync(filter, includeAllResults: true);

        var pdf = _inventoryPdfService.GenerateMovementPdf(model);
        var fileName = $"inventory-movements-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";

        return File(pdf, "application/pdf", fileName);
    }
}