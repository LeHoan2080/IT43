using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationeryWarehouse.Models.Product;
using StationeryWarehouse.Services.Product;

namespace StationeryWarehouse.Controllers;

[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(
        IProductService productService)
    {
        _productService = productService;
    }


    // =========================================================
    // INDEX
    // GET: /Product
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        ProductFilterViewModel filter)
    {
        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        var model =
            await _productService
                .GetProductsAsync(filter);

        return View(model);
    }


    // =========================================================
    // DETAIL
    // GET: /Product/Detail/5
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Detail(
        long id)
    {
        var model =
            await _productService
                .GetProductDetailAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }


    // =========================================================
    // CREATE - GET
    // GET: /Product/Create
    // =========================================================

    [HttpGet]
    [Authorize(Roles =
        "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Create()
    {
        var model =
            new ProductFormViewModel();

        model.Publishers =
            await BuildPublisherSelectListAsync();

        return View(model);
    }


    // =========================================================
    // CREATE - POST
    // POST: /Product/Create
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles =
        "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Create(
        ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Publishers =
                await BuildPublisherSelectListAsync();

            return View(model);
        }

        try
        {
            await _productService
                .CreateProductAsync(model);

            TempData["SuccessMessage"] =
                "Thêm sản phẩm thành công.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Publishers =
                await BuildPublisherSelectListAsync();

            return View(model);
        }
    }


    // =========================================================
    // EDIT - GET
    // GET: /Product/Edit/5
    // =========================================================

    [HttpGet]
    [Authorize(Roles =
        "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Edit(
        long id)
    {
        var model =
            await _productService
                .GetProductForEditAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }


    // =========================================================
    // EDIT - POST
    // POST: /Product/Edit
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles =
        "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Edit(
        ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Publishers =
                await BuildPublisherSelectListAsync();

            return View(model);
        }

        try
        {
            await _productService
                .UpdateProductAsync(model);

            TempData["SuccessMessage"] =
                "Cập nhật sản phẩm thành công.";

            return RedirectToAction(
                nameof(Detail),
                new { id = model.Id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message);

            model.Publishers =
                await BuildPublisherSelectListAsync();

            return View(model);
        }
    }


    // =========================================================
    // DEACTIVATE
    // POST: /Product/Deactivate
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles =
        "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Deactivate(
        long id)
    {
        try
        {
            await _productService
                .DeactivateProductAsync(id);

            TempData["SuccessMessage"] =
                "Đã ngừng sử dụng sản phẩm.";

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
    // ACTIVATE
    // POST: /Product/Activate
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles =
        "ADMIN,WAREHOUSE_MANAGER")]
    public async Task<IActionResult> Activate(
        long id)
    {
        try
        {
            await _productService
                .ActivateProductAsync(id);

            TempData["SuccessMessage"] =
                "Đã kích hoạt sản phẩm.";

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
    // PRIVATE
    // =========================================================

    private async Task<
        List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>>
        BuildPublisherSelectListAsync()
    {
        var publishers =
            await _productService
                .GetPublishersAsync();

        return publishers
            .Select(x =>
                new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value =
                        x.Id.ToString(),

                    Text =
                        $"{x.Code} - {x.Name}"
                })
            .ToList();
    }
}