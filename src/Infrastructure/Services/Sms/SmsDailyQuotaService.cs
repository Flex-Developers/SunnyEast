using System.Text.RegularExpressions;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Sms;

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
        // ключ — по цифрам, чтобы одинаково работало для +7..., 8..., с дефисами и т.п.
        var digits = Regex.Replace(phone ?? "", @"\D", "");
        var key = $"smsq:{digits}";

        // Берём московский часовой пояс кроссплатформенно
        static TimeZoneInfo GetMoscowTz()
        {
            // Linux/macOS: "Europe/Moscow", Windows: "Russian Standard Time"
            foreach (var id in new[] { "Europe/Moscow", "Russian Standard Time" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { /* try next */ }
            }
            throw new InvalidOperationException("MSK timezone not found");
        }

        var tz = GetMoscowTz();

        // Текущее время в МСК
        var nowUtc = DateTimeOffset.UtcNow;
        var nowMsk = TimeZoneInfo.ConvertTime(nowUtc, tz);

        // Следующая полночь в МСК
        var nextMidnightMsk = nowMsk.Date.AddDays(1);

        // TTL до следующей полуночи МСК
        var ttl = nextMidnightMsk - nowMsk;

        return (key, ttl);
    }

}