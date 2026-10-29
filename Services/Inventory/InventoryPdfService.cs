using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StationeryWarehouse.Models.Inventory;

namespace StationeryWarehouse.Services.Inventory;

public class InventoryPdfService : IInventoryPdfService
{
    private static readonly CultureInfo VietnameseCulture =
        CultureInfo.GetCultureInfo("vi-VN");

    public byte[] GenerateInventoryPdf(InventoryIndexViewModel model)
    {
        var filterSummary = BuildInventoryFilterSummary(model);

        return Document.Create(document => document.Page(page =>
        {
            ConfigurePage(page, "Báo cáo tồn kho hiện tại", filterSummary);

            page.Content().PaddingTop(10).Column(column =>
            {
                column.Spacing(10);
                column.Item().Row(row =>
                {
                    row.Spacing(8);
                    SummaryCard(row.RelativeItem(), "Sản phẩm trong kết quả", model.Filter.TotalItems.ToString("N0", VietnameseCulture));
                    SummaryCard(row.RelativeItem(), "Có tồn trong phạm vi", model.InStockProducts.ToString("N0", VietnameseCulture));
                    SummaryCard(row.RelativeItem(), "Dưới Min Stock", model.LowStockProducts.ToString("N0", VietnameseCulture));
                    SummaryCard(row.RelativeItem(), "Không có tồn trong phạm vi", (model.Filter.TotalItems - model.InStockProducts).ToString("N0", VietnameseCulture));
                });

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(72);
                        columns.RelativeColumn(2.4f);
                        columns.ConstantColumn(48);
                        columns.ConstantColumn(44);
                        columns.ConstantColumn(68);
                        columns.ConstantColumn(68);
                        columns.ConstantColumn(58);
                        columns.ConstantColumn(62);
                        columns.RelativeColumn(1.3f);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Barcode");
                        HeaderCell(header.Cell(), "Tên sản phẩm / Mã");
                        HeaderCell(header.Cell(), "Loại");
                        HeaderCell(header.Cell(), "ĐVT");
                        HeaderCell(header.Cell(), "Tồn toàn hệ thống");
                        HeaderCell(header.Cell(), "Tồn theo phạm vi");
                        HeaderCell(header.Cell(), "Min Stock");
                        HeaderCell(header.Cell(), "Trạng thái");
                        HeaderCell(header.Cell(), "Vị trí");
                    });

                    foreach (var item in model.Items)
                    {
                        BodyCell(table.Cell(), item.Barcode ?? "-");
                        BodyCell(table.Cell(), $"{item.ProductName}\n{item.ProductCode}");
                        BodyCell(table.Cell(), item.ProductType == "BOOK" ? "Sách" : "VPP");
                        BodyCell(table.Cell(), item.Unit);
                        BodyCell(table.Cell(), item.TotalQuantity.ToString("N0", VietnameseCulture), true);
                        BodyCell(table.Cell(), item.ScopedQuantity.ToString("N0", VietnameseCulture), true);
                        BodyCell(table.Cell(), item.MinStock.ToString("N0", VietnameseCulture), true);
                        BodyCell(table.Cell(), item.IsLowStock ? "Tồn thấp" : "Đủ tồn");
                        BodyCell(table.Cell(), string.IsNullOrWhiteSpace(item.Location) ? "Chưa có tồn" : item.Location);
                    }
                });
            });

            AddFooter(page.Footer());
        })).GeneratePdf();
    }

    public byte[] GenerateMovementPdf(InventoryMovementReportViewModel model)
    {
        var filterSummary = BuildMovementFilterSummary(model);

        return Document.Create(document => document.Page(page =>
        {
            ConfigurePage(page, "Báo cáo dịch chuyển tồn kho", filterSummary);

            page.Content().PaddingTop(10).Column(column =>
            {
                column.Spacing(10);
                column.Item().Row(row =>
                {
                    row.Spacing(8);
                    SummaryCard(row.RelativeItem(), "Tổng lượt biến động", model.Filter.TotalItems.ToString("N0", VietnameseCulture));
                    SummaryCard(row.RelativeItem(), "Nhập / chuyển đến", model.InboundMovementCount.ToString("N0", VietnameseCulture));
                    SummaryCard(row.RelativeItem(), "Xuất / chuyển đi", model.OutboundMovementCount.ToString("N0", VietnameseCulture));
                    SummaryCard(row.RelativeItem(), "Điều chỉnh", model.AdjustmentMovementCount.ToString("N0", VietnameseCulture));
                });

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(86);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(76);
                        columns.ConstantColumn(72);
                        columns.ConstantColumn(92);
                        columns.ConstantColumn(98);
                        columns.ConstantColumn(72);
                        columns.RelativeColumn(1.6f);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Thời điểm");
                        HeaderCell(header.Cell(), "Sản phẩm / Mã");
                        HeaderCell(header.Cell(), "Loại biến động");
                        HeaderCell(header.Cell(), "Thay đổi");
                        HeaderCell(header.Cell(), "Kho / Bin");
                        HeaderCell(header.Cell(), "Chứng từ");
                        HeaderCell(header.Cell(), "Người thực hiện");
                        HeaderCell(header.Cell(), "Ghi chú");
                    });

                    foreach (var item in model.Items)
                    {
                        BodyCell(table.Cell(), item.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                        BodyCell(table.Cell(), $"{item.ProductName}\n{item.ProductCode}");
                        BodyCell(table.Cell(), GetMovementLabel(item.MovementType));
                        BodyCell(table.Cell(), FormatMovementDelta(item));
                        BodyCell(table.Cell(), $"{item.WarehouseCode} / {item.LocationCode}");
                        BodyCell(table.Cell(), item.ReferenceNo ?? "-");
                        BodyCell(table.Cell(), item.UserName);
                        BodyCell(table.Cell(), item.Note ?? "-");
                    }
                });
            });

            AddFooter(page.Footer());
        })).GeneratePdf();
    }

    private static void ConfigurePage(PageDescriptor page, string title, string filterSummary)
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(24);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(style => style.FontFamily("Lato").FontSize(8));
        page.Header().Column(column =>
        {
            column.Item().Text(title)
                .FontSize(18)
                .Bold()
                .FontColor(Colors.BlueGrey.Darken3);
            column.Item().Text($"Xuất lúc {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                .FontSize(8)
                .FontColor(Colors.Grey.Darken1);
            column.Item().Text(filterSummary)
                .FontSize(8)
                .FontColor(Colors.Grey.Darken2);
            column.Item().PaddingTop(5).LineHorizontal(1)
                .LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void SummaryCard(IContainer container, string label, string value)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten4)
            .Padding(8)
            .Column(column =>
            {
                column.Item().Text(label)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                column.Item().Text(value)
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.BlueGrey.Darken3);
            });
    }

    private static void HeaderCell(IContainer container, string text)
    {
        container
            .Background(Colors.Grey.Lighten3)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten1)
            .PaddingVertical(5)
            .PaddingHorizontal(4)
            .Text(text)
            .FontSize(7)
            .Bold()
            .FontColor(Colors.Grey.Darken3);
    }

    private static void BodyCell(IContainer container, string text, bool alignRight = false)
    {
        var cell = container
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(4)
            .AlignMiddle();

        var descriptor = alignRight
            ? cell.AlignRight().Text(text)
            : cell.Text(text);

        descriptor.FontSize(7);
    }

    private static void AddFooter(IContainer container)
    {
        container
            .PaddingTop(8)
            .AlignCenter()
            .Text(text =>
            {
                text.Span("Trang ");
                text.CurrentPageNumber();
                text.Span(" / ");
                text.TotalPages();
            });
    }

    private static string BuildInventoryFilterSummary(InventoryIndexViewModel model)
    {
        var filters = new List<string>();
        var filter = model.Filter;

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            filters.Add($"Từ khóa: {filter.Keyword}");
        }

        if (!string.IsNullOrWhiteSpace(filter.ProductType))
        {
            filters.Add($"Loại: {(filter.ProductType == "BOOK" ? "Sách" : "Văn phòng phẩm")}");
        }

        if (!string.IsNullOrWhiteSpace(filter.Supplier))
        {
            filters.Add($"NXB / Nhà cung cấp: {filter.Supplier}");
        }

        if (long.TryParse(filter.WarehouseId, out var warehouseId))
        {
            var warehouse = model.Warehouses.FirstOrDefault(x => x.Id == warehouseId);
            filters.Add($"Kho: {warehouse?.Code ?? warehouseId.ToString(VietnameseCulture)}");
        }

        if (long.TryParse(filter.LocationId, out var locationId))
        {
            var location = model.Locations.FirstOrDefault(x => x.Id == locationId);
            filters.Add($"Bin: {location?.Label ?? locationId.ToString(VietnameseCulture)}");
        }

        if (filter.LowStockOnly)
        {
            filters.Add("Chỉ sản phẩm dưới Min Stock");
        }

        return filters.Count == 0
            ? "Bộ lọc: Tất cả"
            : $"Bộ lọc: {string.Join(" | ", filters)}";
    }

    private static string BuildMovementFilterSummary(InventoryMovementReportViewModel model)
    {
        var filters = new List<string>();
        var filter = model.Filter;

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            filters.Add($"Từ khóa: {filter.Keyword}");
        }

        if (filter.FromDate.HasValue)
        {
            filters.Add($"Từ ngày: {filter.FromDate.Value:dd/MM/yyyy}");
        }

        if (filter.ToDate.HasValue)
        {
            filters.Add($"Đến ngày: {filter.ToDate.Value:dd/MM/yyyy}");
        }

        if (filter.MovementType is not null)
        {
            filters.Add($"Loại: {GetMovementLabel(filter.MovementType)}");
        }

        if (filter.WarehouseId.HasValue)
        {
            var warehouse = model.Warehouses.FirstOrDefault(x => x.Id == filter.WarehouseId.Value);
            filters.Add($"Kho: {warehouse?.Code ?? filter.WarehouseId.Value.ToString(VietnameseCulture)}");
        }

        if (filter.LocationId.HasValue)
        {
            var location = model.Locations.FirstOrDefault(x => x.Id == filter.LocationId.Value);
            filters.Add($"Bin: {location?.Label ?? filter.LocationId.Value.ToString(VietnameseCulture)}");
        }

        return filters.Count == 0
            ? "Bộ lọc: Tất cả"
            : $"Bộ lọc: {string.Join(" | ", filters)}";
    }

    private static string GetMovementLabel(string movementType) => movementType switch
    {
        "IN" => "Nhập kho",
        "OUT" => "Xuất kho",
        "TRANSFER_OUT" => "Điều chuyển đi",
        "TRANSFER_IN" => "Điều chuyển đến",
        "ADJUSTMENT" => "Điều chỉnh",
        _ => movementType
    };

    private static string FormatMovementDelta(InventoryMovementListItemViewModel item)
    {
        if (!item.QuantityDelta.HasValue)
        {
            return $"{item.Quantity} {item.Unit} (chưa xác định chiều)";
        }

        return $"{item.QuantityDelta.Value.ToString("+0;-0;0", VietnameseCulture)} {item.Unit}";
    }
}