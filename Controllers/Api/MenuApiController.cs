using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/menu")]
public sealed class MenuApiController(CoffeeShopDbContext db) : ControllerBase
{
    /// <summary>List product categories in display order, excluding toppings.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> Categories(CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .Where(category => category.MaLoaiSP != 9)
            .OrderBy(category => category.ThuTuHienThi)
            .Select(category => new CategoryResponse(category.MaLoaiSP, category.TenLoaiSP))
            .ToListAsync(cancellationToken);
        return Ok(categories);
    }

    /// <summary>Search available menu products with their lowest active variant price.</summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCardResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductCardResponse>>> Products(
        [FromQuery] int? categoryId, [FromQuery] string? keyword, CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking()
            .Where(product => product.TrangThai && product.MaLoaiSP != 9);
        if (categoryId is > 0)
            query = query.Where(product => product.MaLoaiSP == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.Trim();
            query = query.Where(product => product.TenSP.Contains(search));
        }

        var products = await query.OrderBy(product => product.TenSP)
            .Select(product => new ProductCardResponse(
                product.MaSP,
                product.TenSP,
                product.HinhAnh,
                product.Category!.TenLoaiSP,
                product.Variants.Where(variant => variant.TrangThai)
                    .Select(variant => (decimal?)variant.GiaBan)
                    .Min(),
                product.SoLuongTon <= 0))
            .ToListAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>Get product sizes and in-stock toppings for customization.</summary>
    [HttpGet("product-detail/{id:int}")]
    [ProducesResponseType(typeof(ProductDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailResponse>> ProductDetail(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking()
            .Where(item => item.MaSP == id && item.TrangThai && item.MaLoaiSP != 9)
            .Select(item => new ProductDetailBase(
                item.MaSP, item.TenSP, item.HinhAnh, item.Category!.TenLoaiSP, item.SoLuongTon))
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null)
            return NotFound();

        var variants = await db.ProductVariants.AsNoTracking()
            .Where(variant => variant.MaSP == id && variant.TrangThai)
            .OrderBy(variant => variant.MaBienThe)
            .Select(variant => new ProductVariantResponse(variant.MaBienThe, variant.KichCo, variant.GiaBan))
            .ToListAsync(cancellationToken);
        var toppings = await db.Products.AsNoTracking()
            .Where(item => item.TrangThai && item.SoLuongTon > 0 && item.MaLoaiSP == 9)
            .OrderBy(item => item.TenSP)
            .Select(item => new ToppingResponse(
                item.MaSP,
                item.TenSP,
                item.SoLuongTon,
                item.Variants.Where(variant => variant.TrangThai)
                    .OrderBy(variant => variant.MaBienThe)
                    .Select(variant => (decimal?)variant.GiaBan)
                    .FirstOrDefault() ?? 10000m))
            .ToListAsync(cancellationToken);

        return Ok(new ProductDetailResponse(
            product.MaSP, product.TenSP, product.HinhAnh, product.TenLoaiSP, product.SoLuongTon,
            variants, toppings));
    }

    private sealed record ProductDetailBase(int MaSP, string TenSP, string? HinhAnh, string TenLoaiSP, int SoLuongTon);
}

/// <summary>Menu category.</summary>
public sealed record CategoryResponse(int MaLoaiSP, string TenLoaiSP);

/// <summary>Compact product data for menu grids.</summary>
public sealed record ProductCardResponse(
    int MaSP, string TenSP, string? HinhAnh, string TenLoaiSP, decimal? GiaTu, bool IsOutOfStock);

/// <summary>Product variant size and current price.</summary>
public sealed record ProductVariantResponse(int MaBienThe, string KichCo, decimal GiaBan);

/// <summary>Available topping and its current price.</summary>
public sealed record ToppingResponse(int MaSP, string TenSP, int SoLuongTon, decimal GiaBan);

/// <summary>Product detail with active sizes and available toppings.</summary>
public sealed record ProductDetailResponse(
    int MaSP, string TenSP, string? HinhAnh, string TenLoaiSP, int SoLuongTon,
    IReadOnlyList<ProductVariantResponse> Variants, IReadOnlyList<ToppingResponse> Toppings);
