using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Verification;

public sealed class SmsDailyQuotaService(IMemoryCache cache, IConfiguration config) : ISmsDailyQuotaService
{
    private readonly int _dailyLimit = config.GetValue<int>("SmsQuota:DailyLimit", 3);

    public Task<bool> TryConsumeAsync(string phone, CancellationToken ct = default)
    {
        var (key, ttl) = BuildKeyAndTtl(phone);
        var count = cache.Get<int?>(key) ?? 0;

        if (count >= _dailyLimit) return Task.FromResult(false);

        var opts = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        cache.Set(key, count + 1, opts);
        return Task.FromResult(true);
    }

    public Task<int> GetRemainingAsync(string phone, CancellationToken ct = default)
    {
        var (key, ttl) = BuildKeyAndTtl(phone);
        var count = cache.Get<int?>(key) ?? 0;
        return Task.FromResult(Math.Max(0, _dailyLimit - count));
    }

    private static (string key, TimeSpan ttl) BuildKeyAndTtl(string phone)
    {
        var digits = Regex.Replace(phone ?? "", @"\D", ""); // 79001234567
        var key = $"smsq:{digits}";

        var now = DateTimeOffset.UtcNow;
        var midnight = now.Date.AddDays(1);              // сбрасываем в полночь UTC
        var ttl = midnight - now;

        return (key, ttl);
    }
}