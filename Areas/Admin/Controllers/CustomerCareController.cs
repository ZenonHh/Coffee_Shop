using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class CustomerCareController : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index(string tab = "surveys")
    {
        return tab.Equals("contacts", StringComparison.OrdinalIgnoreCase)
            || tab.Equals("reviews", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Index", "Feedback", new { area = "Admin", tab = tab.Equals("contacts", StringComparison.OrdinalIgnoreCase) ? "contacts" : "reviews" })
            : RedirectToAction("Index", "Surveys", new { area = "Admin" });
    }
}
