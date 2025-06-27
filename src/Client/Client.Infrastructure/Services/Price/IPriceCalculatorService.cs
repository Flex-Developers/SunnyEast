namespace Client.Infrastructure.Services.Price;

public interface IPriceCalculatorService
{
    /// <summary>  Возвращает цену для каждого объёма. </summary>
    IEnumerable<(string Volume, decimal? Price)> GetPrices(IEnumerable<string> volumes, decimal basePrice, byte? discountPercent = null);
}