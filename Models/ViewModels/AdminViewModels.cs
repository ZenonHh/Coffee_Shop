using CoffeeShop.Models.Entities;

namespace CoffeeShop.Models.ViewModels
{
    public class AdminFeedbackViewModel
    {
        public List<Feedback> Feedbacks { get; set; } = new();
        public int TotalFeedbacks { get; set; }
        public int FilteredFeedbacks { get; set; }
        public int PendingFeedbacks { get; set; }
        public int ApprovedFeedbacks { get; set; }
        public double AverageRating { get; set; }
        public int[] RatingCounts { get; set; } = new int[5];
        public string? Status { get; set; }
        public int? Rating { get; set; }
        public string? Keyword { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> TopProducts { get; set; } = new();
    }
}
