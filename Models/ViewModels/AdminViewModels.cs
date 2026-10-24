using CoffeeShop.Models.Entities;

namespace CoffeeShop.Models.ViewModels
{
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
