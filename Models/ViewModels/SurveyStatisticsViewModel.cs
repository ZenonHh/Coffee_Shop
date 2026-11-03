namespace CoffeeShop.Models.ViewModels;

public sealed class SurveyStatisticsViewModel
{
    public int? SelectedSurveyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SurveyTitle { get; set; }
    public int QuestionCount { get; set; }
    public int ResponseCount { get; set; }
    public double? AverageRating { get; set; }
    public List<SurveyStatisticsSurveyOptionViewModel> Surveys { get; set; } = new();
    public List<SurveyStatisticsQuestionViewModel> Questions { get; set; } = new();
}

public sealed class SurveyStatisticsSurveyOptionViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}

public sealed class SurveyStatisticsQuestionViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int ResponseCount { get; set; }
    public double? AverageRating { get; set; }
    public List<SurveyStatisticsOptionViewModel> Options { get; set; } = new();
    public List<SurveyStatisticsRatingViewModel> Ratings { get; set; } = new();
    public List<SurveyStatisticsTextAnswerViewModel> TextAnswers { get; set; } = new();
}

public sealed class SurveyStatisticsOptionViewModel
{
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class SurveyStatisticsRatingViewModel
{
    public int Rating { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public sealed class SurveyStatisticsTextAnswerViewModel
{
    public string Text { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; }
}
