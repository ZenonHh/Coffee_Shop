namespace CoffeeShop.Models.ViewModels;

public sealed class CrmStatisticsViewModel
{
    public string Tab { get; set; } = "customers";
    public string Period { get; set; } = "30days";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool RangeIsValid { get; set; }
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
