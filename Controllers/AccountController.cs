using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Controllers.Api;

namespace CoffeeShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly PersistentCartStore _cartStore;

        public AccountController(CoffeeShopDbContext context, PersistentCartStore cartStore)
        {
            _context = context;
            _cartStore = cartStore;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin") || User.IsInRole("NhanVien"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = model.Username?.Trim() ?? string.Empty;
            var password = model.Password ?? string.Empty;

            // Tài khoản nhân viên phải đăng nhập tại khu vực Admin riêng.
            var existingAccount = await _context.Accounts
                .AsNoTracking()
                .Include(account => account.Role)
                .FirstOrDefaultAsync(account => account.TenDangNhap == username && account.TrangThai);
            if (existingAccount != null && !AuthApiController.IsCustomerRole(existingAccount.MaVaiTro, existingAccount.Role?.TenVaiTro))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản quản trị vui lòng đăng nhập tại trang Admin.");
                return View(model);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.TaiKhoan == username);

            if (customer != null && (customer.MatKhau == password || BCryptVerify(password, customer.MatKhau ?? string.Empty)))
            {
                if (!customer.MatKhau!.StartsWith("$2", StringComparison.Ordinal))
                {
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    customer.MatKhau = passwordHash;
                    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.TenDangNhap == username);
                    if (account != null)
                        account.MatKhau = passwordHash;
                    await _context.SaveChangesAsync();
                }

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, customer.TaiKhoan ?? customer.Sdt),
                    new Claim(ClaimTypes.GivenName, customer.HoTen),
                    new Claim(ClaimTypes.Role, "Customer"),
                    new Claim("CustomerId", customer.MaKH.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                await _cartStore.AssociateGuestCartWithCustomerAsync(customer.MaKH);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && !model.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _context.Customers
                .AnyAsync(c => c.TaiKhoan == model.Username || c.Sdt == model.Phone)
                || await _context.Accounts.AnyAsync(account => account.TenDangNhap == model.Username);

            if (existingUser)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc số điện thoại đã được sử dụng.");
                return View(model);
            }

            var customerRoleId = await _context.Roles.AsNoTracking()
                .Where(role => role.TenVaiTro == "Khách hàng" || role.TenVaiTro == "Customer"
                    || role.TenVaiTro == "KhachHang"
                    || (role.MaVaiTro == 1 && role.TenVaiTro != "Admin" && role.TenVaiTro != "Staff" && role.TenVaiTro != "NhanVien"))
                .OrderBy(role => role.MaVaiTro == 1 ? 0 : 1)
                .Select(role => (int?)role.MaVaiTro)
                .FirstOrDefaultAsync();
            if (customerRoleId == null)
            {
                ModelState.AddModelError(string.Empty, "Hệ thống chưa cấu hình vai trò khách hàng. Vui lòng liên hệ cửa hàng.");
                return View(model);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var customer = new Customer
            {
                TaiKhoan = model.Username,
                MatKhau = passwordHash,
                HoTen = model.FullName,
                Sdt = model.Phone,
                Email = model.Email,
                HangTV = "Đồng",
                DiemTichLuy = 0,
                NgayTao = DateTime.Now
            };

            var account = new Account
            {
                MaVaiTro = customerRoleId.Value,
                TenDangNhap = model.Username,
                MatKhau = passwordHash,
                HoTen = model.FullName
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            _context.Customers.Add(customer);
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Đăng nhập tự động sau khi đăng ký
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, customer.TaiKhoan),
                new Claim(ClaimTypes.GivenName, customer.HoTen),
                new Claim(ClaimTypes.Role, "Customer"),
                new Claim("CustomerId", customer.MaKH.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            await _cartStore.AssociateGuestCartWithCustomerAsync(customer.MaKH);

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity?.Name;
            var customer = await _context.Customers
                .AsNoTracking()
                .Where(c => c.TaiKhoan == username)
                .Select(c => new ProfileCustomerViewModel
                {
                    MaKH = c.MaKH,
                    TaiKhoan = c.TaiKhoan,
                    HoTen = c.HoTen,
                    Sdt = c.Sdt,
                    Email = c.Email,
                    DiemTichLuy = c.DiemTichLuy,
                    HangTV = c.HangTV,
                    DiaChi = c.DiaChi
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return RedirectToAction(nameof(Logout));
            }

            var viewModel = new ProfileViewModel
            {
                Customer = customer,
                RecentOrders = await _context.Orders.AsNoTracking()
                    .Where(order => order.MaKH == customer.MaKH)
                    .OrderByDescending(order => order.NgayTao)
                    .Take(10)
                    .Select(order => new ProfileOrderViewModel
                    {
                        MaHD = order.MaHD,
                        NgayTao = order.NgayTao,
                        TongTien = order.TongTien,
                        TrangThai = order.TrangThai,
                        OrderDetails = order.OrderDetails.Select(detail => new ProfileOrderLineViewModel
                        {
                            TenSP = detail.Product!.TenSP,
                            SoLuong = detail.SoLuong
                        }).ToList()
                    })
                    .ToListAsync(),
                PointHistories = await _context.PointHistories.AsNoTracking()
                    .Where(history => history.MaKH == customer.MaKH)
                    .OrderByDescending(history => history.NgayThucHien)
                    .Take(10)
                    .Select(history => new ProfilePointHistoryViewModel
                    {
                        LyDo = history.LyDo,
                        NgayThucHien = history.NgayThucHien,
                        DiemThayDoi = history.DiemThayDoi
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string fullName, string phone, string? email, string? address)
        {
            var username = User.Identity?.Name;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == username);
            if (customer != null)
            {
                customer.HoTen = fullName;
                customer.Sdt = phone;
                customer.Email = email;
                customer.DiaChi = address;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction(nameof(Profile));
        }

        public async Task<IActionResult> Logout()
        {
            if (int.TryParse(User.FindFirstValue("CustomerId"), out var customerId) && customerId > 0)
                await _cartStore.AssociateGuestCartWithCustomerAsync(customerId);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool BCryptVerify(string password, string hash)
        {
            if (!hash.StartsWith("$2", StringComparison.Ordinal)) return false;
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch { return false; }
        }
    }
}
