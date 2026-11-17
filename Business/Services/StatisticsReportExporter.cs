using ClosedXML.Excel;
using CoffeeShop.Business.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CoffeeShop.Business.Services;

public static class StatisticsReportExporter
{
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string PdfContentType = "application/pdf";

    public static byte[] ExportExcel(CrmStatisticsViewModel model)
    {
        using var workbook = new XLWorkbook();
        var tables = CreateTables(model);

        foreach (var table in tables)
        {
            var sheetName = table.Name.Length > 28 ? table.Name.Substring(0, 28) : table.Name;
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.ShowGridLines = true;

            var colCount = Math.Max(table.Headers.Count, 4);

            // 1. Header công ty chuẩn báo cáo tài chính doanh nghiệp
            worksheet.Cell(1, 1).Value = "CÔNG TY CỔ PHẦN CÀ PHÊ KATINAT (KATINAT SAIGON KAFÉ)";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 11;
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#6F4E37");

            worksheet.Cell(2, 1).Value = "Địa chỉ: 91 Đồng Khởi, P. Bến Nghé, Quận 1, TP. Hồ Chí Minh · MST: 0316278899";
            worksheet.Cell(2, 1).Style.Font.FontSize = 9;
            worksheet.Cell(2, 1).Style.Font.Italic = true;

            worksheet.Cell(3, 1).Value = "Hotline: 1900 636 586 · Website: www.katinat.vn";
            worksheet.Cell(3, 1).Style.Font.FontSize = 9;
            worksheet.Cell(3, 1).Style.Font.Italic = true;

            // 2. Tiêu đề báo cáo
            worksheet.Range(5, 1, 5, colCount).Merge();
            worksheet.Cell(5, 1).Value = table.Title.ToUpper();
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Cell(5, 1).Style.Font.FontSize = 14;
            worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(5, 1).Style.Font.FontColor = XLColor.FromHtml("#28251F");

            worksheet.Range(6, 1, 6, colCount).Merge();
            worksheet.Cell(6, 1).Value = $"Kỳ báo cáo: {GetFilterDescription(model)}";
            worksheet.Cell(6, 1).Style.Font.Italic = true;
            worksheet.Cell(6, 1).Style.Font.FontSize = 10;
            worksheet.Cell(6, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Range(7, 1, 7, colCount).Merge();
            worksheet.Cell(7, 1).Value = $"Mã báo cáo: {model.ReportCode} | Người lập: {model.ReportGeneratedBy} | Thời điểm xuất: {model.ReportGeneratedAt:dd/MM/yyyy HH:mm}";
            worksheet.Cell(7, 1).Style.Font.FontSize = 9;
            worksheet.Cell(7, 1).Style.Font.FontColor = XLColor.FromHtml("#555555");
            worksheet.Cell(7, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Range(8, 1, 8, colCount).Merge();
            worksheet.Cell(8, 1).Value = "Đơn vị tính: Đồng Việt Nam (VNĐ)";
            worksheet.Cell(8, 1).Style.Font.Italic = true;
            worksheet.Cell(8, 1).Style.Font.FontSize = 9;
            worksheet.Cell(8, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // 3. Tiêu đề bảng (Header)
            const int startHeaderRow = 10;
            for (var column = 0; column < table.Headers.Count; column++)
            {
                var cell = worksheet.Cell(startHeaderRow, column + 1);
                cell.Value = table.Headers[column];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 10;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#6F4E37");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#555555");
            }
            worksheet.Row(startHeaderRow).Height = 24;

            // 4. Dữ liệu bảng
            var currentRow = startHeaderRow + 1;
            for (var row = 0; row < table.Rows.Count; row++)
            {
                var isTotalRow = false;
                var firstColValue = table.Rows[row].Length > 0 ? table.Rows[row][0]?.ToString() ?? "" : "";
                if (firstColValue.StartsWith("Tổng", StringComparison.OrdinalIgnoreCase) ||
                    firstColValue.StartsWith("TỔNG", StringComparison.OrdinalIgnoreCase))
                {
                    isTotalRow = true;
                }

                for (var column = 0; column < table.Rows[row].Length; column++)
                {
                    var cell = worksheet.Cell(currentRow, column + 1);
                    var val = table.Rows[row][column];

                    if (val is decimal decVal)
                    {
                        cell.Value = decVal;
                        cell.Style.NumberFormat.Format = "#,##0 \"₫\"";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    else if (val is double dblVal)
                    {
                        cell.Value = dblVal;
                        cell.Style.NumberFormat.Format = "#,##0.0";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    else if (val is int intVal)
                    {
                        cell.Value = intVal;
                        cell.Style.NumberFormat.Format = "#,##0";
                        cell.Style.Alignment.Horizontal = (table.Headers[column].Contains("STT") || table.Headers[column].Contains("Mã"))
                            ? XLAlignmentHorizontalValues.Center
                            : XLAlignmentHorizontalValues.Right;
                    }
                    else
                    {
                        cell.Value = XLCellValue.FromObject(val);
                        if (table.Headers[column].Contains("Ngày") || table.Headers[column].Contains("SĐT") || table.Headers[column].Contains("Trạng thái") || table.Headers[column].Contains("PTTT"))
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CCCCCC");

                    if (isTotalRow)
                    {
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F5EEE8");
                        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        cell.Style.Border.BottomBorder = XLBorderStyleValues.Double;
                    }
                }
                worksheet.Row(currentRow).Height = 20;
                currentRow++;
            }

            // 5. Phần Chữ ký xác nhận chuẩn văn bản kế toán
            var sigDateRow = currentRow + 2;
            worksheet.Cell(sigDateRow, colCount - 1).Value = $"TP. Hồ Chí Minh, ngày {DateTime.Now:dd} tháng {DateTime.Now:MM} năm {DateTime.Now:yyyy}";
            worksheet.Cell(sigDateRow, colCount - 1).Style.Font.Italic = true;
            worksheet.Cell(sigDateRow, colCount - 1).Style.Font.FontSize = 9;
            worksheet.Cell(sigDateRow, colCount - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var sigTitleRow = sigDateRow + 2;
            var col1 = 1;
            var col2 = Math.Max(2, colCount / 2);
            var col3 = colCount;

            worksheet.Cell(sigTitleRow, col1).Value = "NGƯỜI LẬP BIỂU";
            worksheet.Cell(sigTitleRow, col1).Style.Font.Bold = true;
            worksheet.Cell(sigTitleRow, col1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow, col2).Value = "KẾ TOÁN TRƯỞNG";
            worksheet.Cell(sigTitleRow, col2).Style.Font.Bold = true;
            worksheet.Cell(sigTitleRow, col2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow, col3).Value = "GIÁM ĐỐC ĐIỀU HÀNH";
            worksheet.Cell(sigTitleRow, col3).Style.Font.Bold = true;
            worksheet.Cell(sigTitleRow, col3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow + 1, col1).Value = "(Ký, ghi rõ họ tên)";
            worksheet.Cell(sigTitleRow + 1, col1).Style.Font.Italic = true;
            worksheet.Cell(sigTitleRow + 1, col1).Style.Font.FontSize = 8;
            worksheet.Cell(sigTitleRow + 1, col1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow + 1, col2).Value = "(Ký, ghi rõ họ tên)";
            worksheet.Cell(sigTitleRow + 1, col2).Style.Font.Italic = true;
            worksheet.Cell(sigTitleRow + 1, col2).Style.Font.FontSize = 8;
            worksheet.Cell(sigTitleRow + 1, col2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow + 1, col3).Value = "(Ký, đóng dấu)";
            worksheet.Cell(sigTitleRow + 1, col3).Style.Font.Italic = true;
            worksheet.Cell(sigTitleRow + 1, col3).Style.Font.FontSize = 8;
            worksheet.Cell(sigTitleRow + 1, col3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow + 5, col1).Value = model.ReportGeneratedBy;
            worksheet.Cell(sigTitleRow + 5, col1).Style.Font.Bold = true;
            worksheet.Cell(sigTitleRow + 5, col1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow + 5, col2).Value = "................................";
            worksheet.Cell(sigTitleRow + 5, col2).Style.Font.FontColor = XLColor.FromHtml("#888888");
            worksheet.Cell(sigTitleRow + 5, col2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(sigTitleRow + 5, col3).Value = "................................";
            worksheet.Cell(sigTitleRow + 5, col3).Style.Font.FontColor = XLColor.FromHtml("#888888");
            worksheet.Cell(sigTitleRow + 5, col3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

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
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("KATINAT SAIGON KAFÉ").Bold().FontSize(13).FontColor("#6F4E37");
                            col.Item().Text("CÔNG TY CỔ PHẦN CÀ PHÊ KATINAT · MST: 0316278899").FontSize(7.5f).FontColor("#666666");
                            col.Item().Text("Địa chỉ: 91 Đồng Khởi, P. Bến Nghé, Quận 1, TP.HCM · Hotline: 1900 636 586").FontSize(7.5f).FontColor("#666666");
                        });
                        row.ConstantItem(180).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Mã báo cáo: {model.ReportCode}").Bold().FontSize(8.5f);
                            col.Item().Text($"Thời điểm xuất: {model.ReportGeneratedAt:dd/MM/yyyy HH:mm}").FontSize(7.5f);
                            col.Item().Text($"Người lập: {model.ReportGeneratedBy}").FontSize(7.5f);
                        });
                    });
                    header.Item().PaddingTop(5).LineHorizontal(1).LineColor("#6F4E37");
                    header.Item().PaddingTop(5).AlignCenter().Text(GetReportTitle(model).ToUpper()).Bold().FontSize(12).FontColor("#28251F");
                    header.Item().AlignCenter().Text(GetFilterDescription(model)).FontSize(8.5f).FontColor("#555555");
                });

                page.Content().PaddingTop(12).Column(content =>
                {
                    content.Spacing(14);
                    foreach (var table in tables)
                    {
                        content.Item().Column(section =>
                        {
                            section.Item().PaddingBottom(5).Text(table.Title).SemiBold().FontSize(10.5f);
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
                                            .Padding(4)
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
                                            .Padding(4)
                                            .Text(FormatCell(value));
                                    }
                                }
                            });
                        });
                    }

                    // Chữ ký xác nhận cuối trang
                    content.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("Người lập biểu").SemiBold().FontSize(9);
                            col.Item().PaddingTop(2).Text("(Ký, ghi rõ họ tên)").FontSize(7).FontColor("#777777");
                            col.Item().PaddingTop(30).Text(model.ReportGeneratedBy).FontSize(8);
                        });
                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("Kế toán trưởng").SemiBold().FontSize(9);
                            col.Item().PaddingTop(2).Text("(Ký, ghi rõ họ tên)").FontSize(7).FontColor("#777777");
                            col.Item().PaddingTop(30).Text("................................").FontSize(8).FontColor("#999999");
                        });
                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("Giám đốc phê duyệt").SemiBold().FontSize(9);
                            col.Item().PaddingTop(2).Text("(Ký, đóng dấu)").FontSize(7).FontColor("#777777");
                            col.Item().PaddingTop(30).Text("................................").FontSize(8).FontColor("#999999");
                        });
                    });
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

        var tables = new List<ReportTable>
        {
            new("Thu - chi", financeHeaders, financeRows),
            new("Doanh thu nhân viên", staffHeaders, staffRows),
            new("Doanh thu sản phẩm", productHeaders, productRows)
        };

        if (model.InvoicesInRange.Count > 0)
        {
            var invoiceHeaders = new[] { "Mã HĐ", "Khách hàng / Người nhận", "SĐT", "Ngày tạo", "PTTT", "Trạng thái", "Tổng tiền" };
            var invoiceRows = model.InvoicesInRange.Select(inv => new object?[]
            {
                inv.MaHD,
                inv.HoTenNguoiNhan,
                inv.SdtNguoiNhan,
                inv.NgayTao.ToString("dd/MM/yyyy HH:mm"),
                inv.PhuongThucThanhToan,
                inv.TrangThai,
                inv.TongTien
            }).ToList();
            invoiceRows.Add(new object?[]
            {
                "TỔNG CỘNG",
                $"{model.RangeInvoiceCount} hóa đơn",
                "",
                $"{model.InvoiceFromDate:dd/MM/yyyy} – {model.InvoiceToDate:dd/MM/yyyy}",
                "",
                "",
                model.RangeTotalRevenue
            });
            tables.Add(new("Chi tiết hóa đơn", invoiceHeaders, invoiceRows));
        }

        return tables;
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

    private static string GetReportTitle(CrmStatisticsViewModel model) => model.Tab switch
    {
        "finance" => "Báo cáo doanh thu",
        "satisfaction" => "Báo cáo mức độ hài lòng",
        "surveys" => "Báo cáo khảo sát",
        _ => "Báo cáo khách hàng"
    };

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
