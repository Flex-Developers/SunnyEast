namespace Application.Common.Interfaces.Services;

public interface ISmsDailyQuotaService
{
    /// <summary>Пытаемся «израсходовать» одну отправку. true = можно, false = лимит исчерпан.</summary>
    Task<bool> TryConsumeAsync(string phone, CancellationToken ct = default);

    /// <summary>Сколько попыток осталось сегодня.</summary>
    Task<int> GetRemainingAsync(string phone, CancellationToken ct = default);
}