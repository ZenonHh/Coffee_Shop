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
        public DbSet<CartSnapshot> CartSnapshots { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;
        public DbSet<Survey> Surveys { get; set; } = null!;
        public DbSet<SurveyQuestion> SurveyQuestions { get; set; } = null!;
        public DbSet<SurveyAnswer> SurveyAnswers { get; set; } = null!;
        public DbSet<SurveyQuestionOption> SurveyQuestionOptions { get; set; } = null!;
        public DbSet<SurveyAudienceCustomer> SurveyAudienceCustomers { get; set; } = null!;
        public DbSet<SurveyResponse> SurveyResponses { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Ingredient> Ingredients { get; set; } = null!;
        public DbSet<StaffMember> StaffMembers { get; set; } = null!;
        public DbSet<Purchase> Purchases { get; set; } = null!;
        public DbSet<PurchaseDetail> PurchaseDetails { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;

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

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.MaNCC)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.StaffMember)
                .WithMany()
                .HasForeignKey(p => p.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Approver)
                .WithMany()
                .HasForeignKey(p => p.NguoiDuyetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseDetail>()
                .HasOne(d => d.Purchase)
                .WithMany(p => p.Details)
                .HasForeignKey(d => d.MaPN)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseDetail>()
                .HasOne(d => d.Ingredient)
                .WithMany()
                .HasForeignKey(d => d.MaNL)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseDetail>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.MaSP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Voucher)
                .WithMany()
                .HasForeignKey(o => o.MaVoucher)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Voucher>()
                .HasOne(voucher => voucher.Customer)
                .WithMany()
                .HasForeignKey(voucher => voucher.MaKH)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.StaffMember)
                .WithMany()
                .HasForeignKey(o => o.MaNV)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SurveyAudienceCustomer>()
                .HasKey(target => new { target.MaKhaoSat, target.MaKH });

            modelBuilder.Entity<SurveyAudienceCustomer>()
                .HasOne(target => target.Survey)
                .WithMany(survey => survey.AudienceCustomers)
                .HasForeignKey(target => target.MaKhaoSat)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SurveyAudienceCustomer>()
                .HasOne(target => target.Customer)
                .WithMany()
                .HasForeignKey(target => target.MaKH)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SurveyQuestionOption>()
                .HasOne(option => option.Question)
                .WithMany(question => question.Options)
                .HasForeignKey(option => option.MaCauHoi)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SurveyResponse>()
                .HasIndex(response => new { response.MaKhaoSat, response.MaKH })
                .IsUnique();

            modelBuilder.Entity<SurveyResponse>()
                .HasOne(response => response.Survey)
                .WithMany(survey => survey.Responses)
                .HasForeignKey(response => response.MaKhaoSat)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SurveyResponse>()
                .HasOne(response => response.Customer)
                .WithMany()
                .HasForeignKey(response => response.MaKH)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SurveyAnswer>()
                .HasOne(answer => answer.Response)
                .WithMany(response => response.Answers)
                .HasForeignKey(answer => answer.MaLuotTraLoi)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
