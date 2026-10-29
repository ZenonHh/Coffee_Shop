using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public partial class FeedbackViewModel(ApiService api) : ObservableObject
{
    public ObservableCollection<ProductItem> Products { get; } = [];
    public ObservableCollection<SurveyItem> Surveys { get; } = [];
    public IReadOnlyList<int> RatingOptions { get; } = [1, 2, 3, 4, 5];

    [ObservableProperty]
    private ProductItem? selectedProduct;

    [ObservableProperty]
    private int rating = 5;

    [ObservableProperty]
    private string comment = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var products = await api.GetProductsAsync(null, null);
            Products.Clear();
            foreach (var product in products.Where(item => !item.IsOutOfStock))
                Products.Add(product);

            var surveys = await api.GetSurveysAsync();
            Surveys.Clear();
            foreach (var survey in surveys)
                Surveys.Add(survey);
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải phản hồi", exception.Message, "Đóng");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (SelectedProduct is null)
        {
            await Shell.Current.DisplayAlertAsync("Thiếu thông tin", "Vui lòng chọn món cần đánh giá.", "Đóng");
            return;
        }
        if (Comment.Trim().Length < 3)
        {
            await Shell.Current.DisplayAlertAsync("Thiếu nhận xét", "Vui lòng nhập nhận xét tối thiểu 3 ký tự.", "Đóng");
            return;
        }

        try
        {
            await api.SubmitReviewAsync(new SubmitReviewRequest(SelectedProduct.MaSP, Rating, Comment.Trim()));
            Comment = string.Empty;
            await Shell.Current.DisplayAlertAsync("Cảm ơn bạn", "Đánh giá đã được gửi.", "Đóng");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể gửi đánh giá", exception.Message, "Đóng");
        }
    }

    public async Task SubmitSurveyAsync(SurveyItem survey, IReadOnlyList<SurveyAnswerRequest> answers)
    {
        try
        {
            await api.SubmitSurveyAsync(survey.MaKhaoSat, new SubmitSurveyRequest(answers));
            await Shell.Current.DisplayAlertAsync("Cảm ơn bạn", "Câu trả lời khảo sát đã được gửi.", "Đóng");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể gửi khảo sát", exception.Message, "Đóng");
        }
    }
}
