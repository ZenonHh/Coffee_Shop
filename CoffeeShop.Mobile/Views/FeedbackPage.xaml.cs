using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class FeedbackPage : ContentPage
{
    private readonly FeedbackViewModel _viewModel;

    public FeedbackPage(FeedbackViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnSurveyClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: SurveyItem survey })
            return;
        if (survey.Questions.Count == 0)
        {
            await DisplayAlertAsync("Khảo sát chưa sẵn sàng", "Khảo sát này chưa có câu hỏi.", "Đóng");
            return;
        }

        var answers = new List<SurveyAnswerRequest>();
        foreach (var question in survey.Questions)
        {
            var answer = await DisplayPromptAsync(
                survey.TieuDe, question.NoiDungCauHoi, "Tiếp tục", "Hủy",
                maxLength: 2000, keyboard: Keyboard.Default);
            if (string.IsNullOrWhiteSpace(answer))
                return;
            answers.Add(new SurveyAnswerRequest(question.MaCauHoi, answer.Trim()));
        }

        await _viewModel.SubmitSurveyAsync(survey, answers);
    }
}
