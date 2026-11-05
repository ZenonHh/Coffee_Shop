using ClosedXML.Excel;
using CoffeeShop.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoffeeShop.Services;

public static class StatisticsReportExporter
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string PdfContentType = "application/pdf";

    public static byte[] ExportExcel(CrmStatisticsViewModel model)
    {
        using var workbook = new XLWorkbook();
        foreach (var table in CreateTables(model))
        {
            var worksheet = workbook.Worksheets.Add(table.Name);
            for (var column = 0; column < table.Headers.Count; column++)
            {
                worksheet.Cell(1, column + 1).Value = table.Headers[column];
                worksheet.Cell(1, column + 1).Style.Font.Bold = true;
                worksheet.Cell(1, column + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5EEE8");
            }

            for (var row = 0; row < table.Rows.Count; row++)
            {
                for (var column = 0; column < table.Rows[row].Length; column++)
                    worksheet.Cell(row + 2, column + 1).Value = XLCellValue.FromObject(table.Rows[row][column]);
            }

            if (table.Name is "Thu - chi" or "Doanh thu nhân viên" or "Doanh thu sản phẩm"
                && table.Rows.Count > 0
                && table.Headers.Count > 1)
            {
                worksheet.Range(2, 2, table.Rows.Count + 1, table.Headers.Count)
                    .Style.NumberFormat.Format = "#,##0 \"₫\"";
            }

            worksheet.SheetView.FreezeRows(1);
            worksheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static byte[] ExportPdf(CrmStatisticsViewModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var tables = CreateTables(model);
        var generatedAt = DateTime.Now;

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(model.Tab == "finance" && model.FinancePeriod == "month"
                    ? PageSizes.A3.Landscape()
                    : PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(style => style.FontSize(8));

                page.Header().Column(header =>
                {
                    header.Item().Text("BÁO CÁO THỐNG KÊ KATINAT COFFEE").SemiBold().FontSize(16);
                    header.Item().PaddingTop(4).Text(GetFilterDescription(model)).FontSize(9);
                    header.Item().PaddingTop(2).Text($"Thời điểm xuất: {generatedAt:dd/MM/yyyy HH:mm}").FontSize(8);
                });

                page.Content().PaddingTop(16).Column(content =>
                {
                    content.Spacing(16);
                    foreach (var table in tables)
                    {
                        content.Item().Column(section =>
                        {
                            section.Item().PaddingBottom(6).Text(table.Title).SemiBold().FontSize(11);
                            section.Item().Table(pdfTable =>
                            {
                                pdfTable.ColumnsDefinition(columns =>
                                {
                                    foreach (var _ in table.Headers)
                                        columns.RelativeColumn();
                                });

                                pdfTable.Header(headerRow =>
                                {
                                    foreach (var title in table.Headers)
                                    {
                                        headerRow.Cell()
                                            .Background("#F5EEE8")
                                            .Padding(5)
                                            .Text(title)
                                            .SemiBold();
                                    }
                                });

                                foreach (var row in table.Rows)
                                {
                                    foreach (var value in row)
                                    {
                                        pdfTable.Cell()
                                            .BorderBottom(0.5f)
                                            .BorderColor("#E5DED7")
                                            .Padding(5)
                                            .Text(FormatCell(value));
                                    }
                                }
                            });
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Trang ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    public static string GetFileName(CrmStatisticsViewModel model, string extension)
    {
        var period = model.Tab == "finance"
            ? $"{model.FinanceYear}-{model.FinancePeriod}"
            : $"{model.FromDate:yyyyMMdd}-{model.ToDate:yyyyMMdd}";
        return $"thong-ke-{model.Tab}-{period}.{extension}";
    }

    private static List<ReportTable> CreateTables(CrmStatisticsViewModel model) => model.Tab switch
    {
        "finance" => CreateFinanceTables(model),
        "satisfaction" => CreateSatisfactionTables(model),
        "surveys" => CreateSurveyTables(model),
        _ => CreateCustomerTables(model)
    };

    private static List<ReportTable> CreateFinanceTables(CrmStatisticsViewModel model)
    {
        var periods = model.FinancialPeriods;
        var financeHeaders = new[] { "Chỉ tiêu" }
            .Concat(periods.Select(period => period.Label))
            .Append(model.FinanceTotalLabel)
            .ToArray();
        var financeRows = new[]
        {
            new object?[] { "Doanh thu" }
                .Concat(periods.Select(period => (object?)period.Revenue))
                .Append((object?)periods.Sum(period => period.Revenue))
                .ToArray(),
            new object?[] { "Chi phí nhập hàng" }
                .Concat(periods.Select(period => (object?)period.Purchases))
                .Append((object?)periods.Sum(period => period.Purchases))
                .ToArray(),
            new object?[] { "Chênh lệch thu - chi" }
                .Concat(periods.Select(period => (object?)period.Difference))
                .Append((object?)periods.Sum(period => period.Difference))
                .ToArray()
        };
        var staffHeaders = new[] { "Nhân viên" }
            .Concat(model.StaffReportYears.Select(year => year.ToString()))
            .Append("Tổng")
            .ToArray();
        var staffRows = model.StaffRevenue.Select(staff =>
            new object?[] { staff.Name }
                .Concat(model.StaffReportYears.Select(year => (object?)staff.RevenueByYear.GetValueOrDefault(year)))
                .Append(staff.Total)
                .ToArray()).ToList();
        staffRows.Add(new object?[] { "Tổng nhân viên" }
            .Concat(model.StaffReportYears.Select(year =>
                (object?)model.StaffRevenue.Sum(staff => staff.RevenueByYear.GetValueOrDefault(year))))
            .Append(model.StaffRevenue.Sum(staff => staff.Total))
            .ToArray());

        var productHeaders = new[] { "Sản phẩm", "Quý 1", "Quý 2", "Quý 3", "Quý 4", "Tổng năm" };
        var productRows = model.ProductQuarterRevenue.Select(product =>
            new object?[] { product.Name }
                .Concat(Enumerable.Range(1, 4).Select(quarter =>
                    (object?)product.RevenueByQuarter.GetValueOrDefault(quarter)))
                .Append(product.Total)
                .ToArray()).ToList();
        productRows.Add(new object?[] { "Tổng sản phẩm" }
            .Concat(Enumerable.Range(1, 4).Select(quarter =>
                (object?)model.ProductQuarterRevenue.Sum(product => product.RevenueByQuarter.GetValueOrDefault(quarter))))
            .Append(model.ProductQuarterRevenue.Sum(product => product.Total))
            .ToArray());

        return
        [
            new("Thu - chi", financeHeaders, financeRows),
            new("Doanh thu nhân viên", staffHeaders, staffRows),
            new("Doanh thu sản phẩm", productHeaders, productRows)
        ];
    }

    private static List<ReportTable> CreateCustomerTables(CrmStatisticsViewModel model) =>
    [
        new("Tổng quan", ["Chỉ tiêu", "Giá trị"],
        [
            ["Tổng khách hàng", model.TotalCustomers],
            ["Khách hàng mới trong kỳ", model.NewCustomers],
            ["Từ ngày", model.FromDate.ToString("dd/MM/yyyy")],
            ["Đến ngày", model.ToDate.ToString("dd/MM/yyyy")]
        ]),
        new("Hạng thành viên", ["Hạng", "Số khách", "Tỷ lệ (%)"],
            model.LoyaltyTiers.Select(item => new object?[] { item.Label, item.Count, item.Percentage }).ToList()),
        new("Độ tuổi", ["Nhóm tuổi", "Số khách", "Tỷ lệ (%)"],
            model.AgeGroups.Select(item => new object?[] { item.Label, item.Count, item.Percentage }).ToList()),
        new("Sở thích", ["Danh mục", "Số khách", "Tỷ lệ (%)"],
            model.Interests.Select(item => new object?[] { item.Label, item.Count, item.Percentage }).ToList())
    ];

    private static List<ReportTable> CreateSatisfactionTables(CrmStatisticsViewModel model) =>
    [
        new("Tổng quan", ["Chỉ tiêu", "Giá trị"],
        [
            ["Điểm hài lòng trung bình", model.AverageSatisfaction],
            ["Tổng đánh giá", model.TotalFeedbackCount],
            ["Đánh giá có chấm điểm", model.RatedFeedbackCount],
            ["Khách đã phản hồi", model.RespondedCustomerCount],
            ["Tổng khách hàng", model.TotalCustomers],
            ["Từ ngày", model.FromDate.ToString("dd/MM/yyyy")],
            ["Đến ngày", model.ToDate.ToString("dd/MM/yyyy")]
        ]),
        new("Phân bố đánh giá", ["Số sao", "Số lượt", "Tỷ lệ (%)"],
            model.Ratings.Select(item => new object?[] { item.Rating, item.Count, item.Percentage }).ToList()),
        new("Xu hướng theo tháng", ["Tháng", "Số lượt", "Điểm trung bình"],
            model.MonthlyRatings.Select(item => new object?[] { item.Month, item.Count, item.Average }).ToList()),
        new("Sản phẩm được đánh giá cao", ["Sản phẩm", "Số lượt", "Điểm trung bình"],
            model.TopProducts.Select(item => new object?[] { item.Name, item.Count, item.Average }).ToList()),
        new("Sản phẩm cần cải thiện", ["Sản phẩm", "Số lượt", "Điểm trung bình"],
            model.LowestProducts.Select(item => new object?[] { item.Name, item.Count, item.Average }).ToList())
    ];

    private static List<ReportTable> CreateSurveyTables(CrmStatisticsViewModel model) =>
    [
        new("Đợt khảo sát", ["Tên khảo sát", "Đối tượng (khách)", "Lượt trả lời", "Tỷ lệ tham gia (%)"],
            model.Surveys.Select(item => new object?[]
            {
                item.Title,
                item.TargetCount,
                item.ResponseCount,
                item.Percentage
            }).ToList())
    ];

    private static string GetFilterDescription(CrmStatisticsViewModel model) =>
        model.Tab == "finance"
            ? $"Báo cáo doanh thu năm {model.FinanceYear}, xem theo {model.FinancePeriod switch
                {
                    "month" => "tháng",
                    "year" => "năm",
                    _ => "quý"
                }}"
            : $"Báo cáo {model.FromDate:dd/MM/yyyy} - {model.ToDate:dd/MM/yyyy}";

    private static string FormatCell(object? value) => value switch
    {
        null => "—",
        double number => number.ToString("0.##"),
        decimal amount => $"{amount:N0} ₫",
        _ => Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty
    };

    private sealed record ReportTable(string Name, IReadOnlyList<string> Headers, IReadOnlyList<object?[]> Rows)
    {
        public string Title => Name.Replace('-', ' ');
    }
}
