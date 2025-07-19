using Application.Common.Interfaces.Services;

namespace Infrastructure.Services;

public sealed class DateTimeService : IDateTimeService
{
    private static readonly TimeZoneInfo _msk =
        TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Moscow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _msk);
}