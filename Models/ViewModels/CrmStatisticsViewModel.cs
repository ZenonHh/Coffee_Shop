namespace CoffeeShop.Models.ViewModels;

public sealed class CrmStatisticsViewModel
{
    public string Tab { get; set; } = "customers";
    public string Period { get; set; } = "30days";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool RangeIsValid { get; set; }
    public int FinanceYear { get; set; }
    public string FinancePeriod { get; set; } = "quarter";
    public string FinanceTotalLabel => FinancePeriod == "year" ? "Tổng kỳ" : "Tổng năm";
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int RatedFeedbackCount { get; set; }
    public int RespondedCustomerCount { get; set; }
    public int TotalFeedbackCount { get; set; }
    public double? AverageSatisfaction { get; set; }
    public List<CrmSegmentViewModel> AgeGroups { get; set; } = new();
    public List<CrmSegmentViewModel> Interests { get; set; } = new();
    public List<CrmSegmentViewModel> LoyaltyTiers { get; set; } = new();
    public List<CrmRatingViewModel> Ratings { get; set; } = new();
    public List<CrmProductRatingViewModel> TopProducts { get; set; } = new();
    public List<CrmProductRatingViewModel> LowestProducts { get; set; } = new();
    public List<CrmMonthlyRatingViewModel> MonthlyRatings { get; set; } = new();
    public List<CrmSurveyParticipationViewModel> Surveys { get; set; } = new();
    public decimal TotalCompletedRevenue { get; set; }
    public int CompletedInvoiceCount { get; set; }
    public decimal TotalApprovedPurchases { get; set; }
    public decimal RevenueMinusPurchases { get; set; }
    public List<FinancePeriodViewModel> FinancialPeriods { get; set; } = new();
    public List<int> StaffReportYears { get; set; } = new();
    public List<StaffRevenueViewModel> StaffRevenue { get; set; } = new();
    public List<ProductQuarterRevenueViewModel> ProductQuarterRevenue { get; set; } = new();
}

public sealed class FinancePeriodViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Purchases { get; set; }
    public decimal Difference => Revenue - Purchases;
}

public sealed class StaffRevenueViewModel
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<int, decimal> RevenueByYear { get; set; } = new();
    public decimal Total => RevenueByYear.Values.Sum();
}

public sealed class ProductQuarterRevenueViewModel
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<int, decimal> RevenueByQuarter { get; set; } = new();
    public decimal Total => RevenueByQuarter.Values.Sum();
}

public sealed class CrmRatingViewModel
{
    public int Rating { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class CrmProductRatingViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Average { get; set; }
}

public sealed class CrmMonthlyRatingViewModel
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Average { get; set; }
}

public sealed class CrmSurveyParticipationViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TargetCount { get; set; }
    public int ResponseCount { get; set; }
    public double Percentage { get; set; }
}
