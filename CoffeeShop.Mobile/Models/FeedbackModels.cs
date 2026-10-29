namespace CoffeeShop.Mobile.Models;

public sealed record SurveyItem(
    int MaKhaoSat,
    string TieuDe,
    string? MoTa,
    DateTime NgayKetThuc,
    IReadOnlyList<SurveyQuestionItem> Questions);

public sealed record SurveyQuestionItem(
    int MaCauHoi,
    string NoiDungCauHoi,
    string LoaiCauHoi);

public sealed record SurveyAnswerRequest(int MaCauHoi, string NoiDungTraLoi);
public sealed record SubmitSurveyRequest(IReadOnlyList<SurveyAnswerRequest> Answers);
public sealed record SubmitReviewRequest(int MaSP, int DanhGia, string NoiDung);
