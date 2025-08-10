using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Sms;

public sealed class VerificationSessionStore(IMemoryCache cache) : IVerificationSessionStore
{
    private static readonly string Prefix = "otp:";

    public Task SaveAsync(VerificationSession session, CancellationToken ct)
    {
        var key = Prefix + session.SessionId;
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = session.ExpiresAt
        };
        cache.Set(key, session, options);
        return Task.CompletedTask;
    }

    public Task<VerificationSession?> GetAsync(string sessionId, CancellationToken ct)
    {
        return Task.FromResult(cache.TryGetValue(Prefix + sessionId, out VerificationSession s) ? s : null);
    }

    public Task UpdateAsync(VerificationSession session, CancellationToken ct)
    {
        cache.Set(Prefix + session.SessionId, session, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = session.ExpiresAt
        });
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string sessionId, CancellationToken ct)
    {
        cache.Remove(Prefix + sessionId);
        return Task.CompletedTask;
    }
}