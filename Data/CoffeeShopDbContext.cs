using Microsoft.EntityFrameworkCore;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Data
{
    public class CoffeeShopDbContext : DbContext
    {
        public CoffeeShopDbContext(DbContextOptions<CoffeeShopDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<PointHistory> PointHistories { get; set; } = null!;
        public DbSet<Voucher> Vouchers { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<OrderDetailTopping> OrderDetailToppings { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<Survey> Surveys { get; set; } = null!;
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; } = null!;
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key cho ChiTietHoaDon_Topping
            modelBuilder.Entity<OrderDetailTopping>()
                .HasKey(t => new { t.MaCTHD, t.MaSP });

            modelBuilder.Entity<OrderDetailTopping>()
                .HasOne(t => t.OrderDetail)
                .WithMany(od => od.Toppings)
                .HasForeignKey(t => t.MaCTHD)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetailTopping>()
                .HasOne(t => t.ToppingProduct)
                .WithMany()
                .HasForeignKey(t => t.MaSP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.MaLoaiSP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.MaSP)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.MaHD)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.MaSP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Variant)
                .WithMany()
                .HasForeignKey(od => od.MaBienThe)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
