using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Inventory;
using StationeryWarehouse.Services.Inventory;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(
        IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        InventoryFilterViewModel filter)
    {
        // Chỉ cho phép PageSize = 20
        filter.PageSize = 20;

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
}