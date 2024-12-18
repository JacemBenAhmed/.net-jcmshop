using projTP.Models;

namespace projTP.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCategories { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string HighestStockProduct { get; set; }
        public List<dynamic> OrdersByStatus { get; set; }
        public List<dynamic> ProductsByCategory { get; set; }
        public List<dynamic> TopCustomers { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<Product> TopSellingProducts { get; set; }
    }

}
