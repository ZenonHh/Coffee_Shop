using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Controllers.Api;
using CoffeeShop.Services;

namespace CoffeeShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly PersistentCartStore _cartStore;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            CoffeeShopDbContext context,
            PersistentCartStore cartStore,
            ILogger<AccountController> logger)
        {
            _context = context;
            _cartStore = cartStore;
            _logger = logger;
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
                .FirstOrDefaultAsync(account => account.TenDangNhap == username);
            if (existingAccount != null && !existingAccount.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng liên hệ cửa hàng.");
                return View(model);
            }
            if (existingAccount != null && !AuthApiController.IsCustomerRole(existingAccount.MaVaiTro, existingAccount.Role?.TenVaiTro))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản quản trị vui lòng đăng nhập tại trang Admin.");
                return View(model);
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.TaiKhoan == username && c.TrangThai);

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
        public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
        {
            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.Username = model.Username?.Trim() ?? string.Empty;
            model.Phone = model.Phone?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (System.Text.Encoding.UTF8.GetByteCount(model.Password) > 72)
            {
                ModelState.AddModelError(nameof(model.Password), "Mật khẩu không được vượt quá 72 byte UTF-8.");
                return View(model);
            }

            Customer customer;
            try
            {
                var matchingCustomers = await _context.Customers.AsNoTracking()
                    .Where(c => c.TaiKhoan == model.Username || c.Sdt == model.Phone)
                    .Select(c => new
                    {
                        UsernameMatches = c.TaiKhoan == model.Username,
                        PhoneMatches = c.Sdt == model.Phone
                    })
                    .ToListAsync(cancellationToken);
                var usernameExistsInStaffAccounts = await _context.Accounts.AsNoTracking()
                    .AnyAsync(account => account.TenDangNhap == model.Username, cancellationToken);

                if (matchingCustomers.Any(c => c.UsernameMatches) || usernameExistsInStaffAccounts)
                {
                    ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập này đã được sử dụng.");
                }

                if (matchingCustomers.Any(c => c.PhoneMatches))
                {
                    ModelState.AddModelError(nameof(model.Phone), "Số điện thoại này đã được đăng ký cho tài khoản khác.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                await using var transaction = await _context.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                var customerRoleId = await CustomerRoleResolver.GetOrCreateAsync(_context, cancellationToken);

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                customer = new Customer
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
                    MaVaiTro = customerRoleId,
                    TenDangNhap = model.Username,
                    MatKhau = passwordHash,
                    HoTen = model.FullName
                };

                _context.Customers.Add(customer);
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (AuthApiController.IsUniqueConstraintViolation(exception))
            {
                _logger.LogWarning(exception,
                    "Web registration conflict. SQL detail: {SqlError}",
                    exception.InnerException?.Message ?? exception.Message);
                ModelState.AddModelError(string.Empty,
                    "Tên đăng nhập hoặc số điện thoại vừa được một tài khoản khác đăng ký. Vui lòng kiểm tra lại.");
                return View(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,
                    "Web registration failed. SQL detail: {SqlError}",
                    exception.InnerException?.Message ?? exception.Message);
                ModelState.AddModelError(string.Empty,
                    "Không thể tạo tài khoản lúc này. Vui lòng thử lại sau.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, customer.TaiKhoan ?? model.Username),
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
        public async Task<IActionResult> Profile(CancellationToken cancellationToken)
        {
            var customer = await FindCurrentCustomerAsync(tracking: false, cancellationToken);
            if (customer == null)
                return RedirectToAction(nameof(Logout));

            return View(await BuildProfileViewModelAsync(customer, cancellationToken));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            ProfileViewModel model, CancellationToken cancellationToken)
        {
            var customer = await FindCurrentCustomerAsync(tracking: true, cancellationToken);
            if (customer is null)
                return RedirectToAction(nameof(Logout));

            var editProfile = model.EditProfile;
            editProfile.FullName = editProfile.FullName?.Trim() ?? string.Empty;
            editProfile.Phone = editProfile.Phone?.Trim() ?? string.Empty;
            editProfile.Email = string.IsNullOrWhiteSpace(editProfile.Email) ? null : editProfile.Email.Trim();
            editProfile.Address = string.IsNullOrWhiteSpace(editProfile.Address) ? null : editProfile.Address.Trim();

            if (ModelState.IsValid
                && await _context.Customers.AsNoTracking()
                    .AnyAsync(item => item.MaKH != customer.MaKH && item.Sdt == editProfile.Phone, cancellationToken))
            {
                ModelState.AddModelError("EditProfile.Phone", "Số điện thoại đã được sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                var invalidViewModel = await BuildProfileViewModelAsync(customer, cancellationToken);
                invalidViewModel.EditProfile = editProfile;
                ViewBag.ShowProfileEdit = true;
                return View("Profile", invalidViewModel);
            }

            customer.HoTen = editProfile.FullName;
            customer.Sdt = editProfile.Phone;
            customer.Email = editProfile.Email;
            customer.DiaChi = editProfile.Address;
            var account = await _context.Accounts
                .FirstOrDefaultAsync(item => item.TenDangNhap == customer.TaiKhoan, cancellationToken);
            if (account is not null)
                account.HoTen = customer.HoTen;

            await _context.SaveChangesAsync(cancellationToken);
            TempData["Message"] = "Cập nhật thông tin thành công!";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            [Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model,
            CancellationToken cancellationToken)
        {
            var customer = await FindCurrentCustomerAsync(tracking: true, cancellationToken);
            if (customer is null)
                return RedirectToAction(nameof(Logout));

            if (ModelState.IsValid
                && System.Text.Encoding.UTF8.GetByteCount(model.NewPassword) > 72)
            {
                ModelState.AddModelError(
                    "ChangePassword.NewPassword",
                    "Mật khẩu mới không được vượt quá 72 byte UTF-8.");
            }

            if (ModelState.IsValid
                && customer.MatKhau != model.CurrentPassword
                && !BCryptVerify(model.CurrentPassword, customer.MatKhau ?? string.Empty))
            {
                ModelState.AddModelError(
                    "ChangePassword.CurrentPassword",
                    "Mật khẩu hiện tại không chính xác.");
            }

            if (!ModelState.IsValid)
            {
                var viewModel = await BuildProfileViewModelAsync(customer, cancellationToken);
                viewModel.ChangePassword = model;
                return View("Profile", viewModel);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            customer.MatKhau = passwordHash;

            if (!string.IsNullOrWhiteSpace(customer.TaiKhoan))
            {
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(
                        item => item.TenDangNhap == customer.TaiKhoan,
                        cancellationToken);
                if (account is not null)
                    account.MatKhau = passwordHash;
            }

            await _context.SaveChangesAsync(cancellationToken);
            TempData["Message"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }

        private async Task<Customer?> FindCurrentCustomerAsync(bool tracking, CancellationToken cancellationToken)
        {
            IQueryable<Customer> query = _context.Customers;
            if (!tracking)
                query = query.AsNoTracking();

            if (int.TryParse(User.FindFirstValue("CustomerId"), out var customerId) && customerId > 0)
                return await query.FirstOrDefaultAsync(item => item.MaKH == customerId, cancellationToken);

            var username = User.Identity?.Name;
            return string.IsNullOrWhiteSpace(username)
                ? null
                : await query.FirstOrDefaultAsync(item => item.TaiKhoan == username, cancellationToken);
        }

        private async Task<ProfileViewModel> BuildProfileViewModelAsync(
            Customer customer, CancellationToken cancellationToken)
        {
            return new ProfileViewModel
            {
                Customer = new ProfileCustomerViewModel
                {
                    MaKH = customer.MaKH,
                    TaiKhoan = customer.TaiKhoan,
                    HoTen = customer.HoTen,
                    Sdt = customer.Sdt,
                    Email = customer.Email,
                    DiemTichLuy = customer.DiemTichLuy,
                    HangTV = customer.HangTV,
                    DiaChi = customer.DiaChi
                },
                EditProfile = new ProfileEditViewModel
                {
                    FullName = customer.HoTen,
                    Phone = customer.Sdt,
                    Email = customer.Email,
                    Address = customer.DiaChi
                },
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
                        LyDoHuy = order.LyDoHuy,
                        OrderDetails = order.OrderDetails.Select(detail => new ProfileOrderLineViewModel
                        {
                            TenSP = detail.Product!.TenSP,
                            SoLuong = detail.SoLuong
                        }).ToList()
                    })
                    .ToListAsync(cancellationToken),
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
                    .ToListAsync(cancellationToken)
            };
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
