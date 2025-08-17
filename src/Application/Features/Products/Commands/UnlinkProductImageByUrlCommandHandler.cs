using Application.Common.Interfaces.Contexts;
using Application.Contract.Product.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands;

public sealed class UnlinkProductImageByUrlCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UnlinkProductImageByUrlCommand, int>
{
    public async Task<int> Handle(UnlinkProductImageByUrlCommand request, CancellationToken ct)
    {
        var url = (request.Url ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(url))
            return 0;

        var target = Normalize(url);

        var affected = 0;
        affected += await CleanProductImagesAsync(target, ct);
        affected += await CleanShopImagesAsync(target, ct);
        affected += await CleanProductCategoryImageAsync(target, ct);
        affected += await CleanStaffImagesAsync(target, ct); // сейчас no-op, оставлен для будущего

        if (affected > 0)
            await db.SaveChangesAsync(ct);

        return affected;
    }

    // ---------------- Products.Images[] ----------------
    private async Task<int> CleanProductImagesAsync(string target, CancellationToken ct)
    {
        // ВАЖНО: никакого Where по массиву — всё в памяти.
        var list = await db.Products.AsTracking().ToListAsync(ct);

        var affected = 0;

        foreach (var p in list)
        {
            var images = p.Images;
            if (images is null || images.Length == 0)
                continue;

            var filtered = RemoveMatches(images, target);
            if (filtered.Length != images.Length)
            {
                p.Images = filtered.Length == 0 ? [] : filtered;
                affected++;
            }
        }

        return affected;
    }

    // ---------------- Shops.Images[] ----------------
    private async Task<int> CleanShopImagesAsync(string target, CancellationToken ct)
    {
        var list = await db.Shops.AsTracking().ToListAsync(ct);

        var affected = 0;

        foreach (var s in list)
        {
            var images = s.Images;
            if (images is null || images.Length == 0)
                continue;

            var filtered = RemoveMatches(images, target);
            if (filtered.Length != images.Length)
            {
                s.Images = filtered.Length == 0 ? [] : filtered;
                affected++;
            }
        }

        return affected;
    }


    // ---------------- ProductCategories.ImageUrl (single) ----------------
    private async Task<int> CleanProductCategoryImageAsync(string target, CancellationToken ct)
    {
        var list = await db.ProductCategories
            .Where(c => c.ImageUrl != null && c.ImageUrl != "")
            .ToListAsync(ct);

        var affected = 0;

        foreach (var c in list)
        {
            var img = c.ImageUrl!;
            if (IsMatch(img, target))
            {
                c.ImageUrl = null;
                affected++;
            }
        }

        return affected;
    }

    // ---------------- Staff (на будущее) ----------------
    // В текущей модели изображений нет, метод оставлен как задел.
    private Task<int> CleanStaffImagesAsync(string target, CancellationToken ct)
        => Task.FromResult(0);

    // ================= helpers =================

    private static string Normalize(string s)
    {
        try { return new Uri(s, UriKind.Absolute).ToString().Trim(); }
        catch { return s.Trim(); }
    }

    private static string[] RemoveMatches(string[] source, string target)
        => source.Where(x => !IsMatch(x, target)).ToArray();

    private static bool ContainsMatch(string[] source, string target)
        => source.Any(x => IsMatch(x, target));

    private static bool IsMatch(string candidate, string target)
    {
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        // 1) точное сравнение (ignore-case)
        if (string.Equals(candidate, target, StringComparison.OrdinalIgnoreCase))
            return true;

        // 2) сравнение после URL-decode
        var cDec = Uri.UnescapeDataString(candidate);
        var tDec = Uri.UnescapeDataString(target);
        if (string.Equals(cDec, tDec, StringComparison.OrdinalIgnoreCase))
            return true;

        // 3) сравнение по имени файла (если CDN отдаёт один файл по разным URL)
        return string.Equals(GetFileName(candidate), GetFileName(target), StringComparison.OrdinalIgnoreCase)
               || string.Equals(GetFileName(cDec), GetFileName(tDec), StringComparison.OrdinalIgnoreCase);
    }

    private static string GetFileName(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        try
        {
            var u = new Uri(url, UriKind.Absolute);
            return Path.GetFileName(u.LocalPath);
        }
        catch
        {
            var idx = url.LastIndexOf('/');
            return idx >= 0 ? url[(idx + 1)..] : url;
        }
    }
}
