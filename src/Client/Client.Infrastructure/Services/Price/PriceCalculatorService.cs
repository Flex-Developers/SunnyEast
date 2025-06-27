namespace Client.Infrastructure.Services.Price;

public class PriceCalculatorService : IPriceCalculatorService
{
    private static readonly Dictionary<string, (int factor, string group)> Units = new()
    {
        ["г"]  = (1,     "mass"),
        ["кг"] = (1000,  "mass"),
        ["мл"] = (1,     "volume"),
        ["л"]  = (1000,  "volume"),
        ["шт"] = (1,     "piece")
    };

    public IEnumerable<(string Volume, decimal? Price)> GetPrices(IEnumerable<string> volumes, decimal basePrice, byte? discountPercent = null)
    {
        var volList = volumes.ToList();
        if (!volList.Any()) yield break;

        // базовая цена: с учётом скидки, если есть
        var realBasePrice = discountPercent is > 0
            ? Math.Round(basePrice * (1 - discountPercent.Value / 100m), 2)
            : basePrice;

        var (baseVal, baseUnit, baseGroup) = Parse(volList[0]);
        var baseQty = baseVal * Units[baseUnit].factor;

        foreach (var v in volList)
        {
            var (val, unit, group) = Parse(v);

            if (group != baseGroup) { yield return (v, null); continue; }

            var qty   = val * Units[unit].factor;
            var price = Math.Round(realBasePrice * qty / baseQty, 2);

            yield return (v, price);
        }
    }

    // helper -----------------------------------------------------------
    private static (int val, string unit, string group) Parse(string raw)
    {
        var parts = raw.Trim().ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int  v   = int.Parse(parts[0]);
        var  u   = parts.Length > 1 ? parts[1] : "шт";
        var  g   = Units.TryGetValue(u, out var info) ? info.group : "other";

        return (v, u, g);
    }
}