namespace Application.Contract;

public class ProductPriceDto
{
    public decimal? Gr100 { get; set; }
    public decimal? Gr300 { get; set; }
    public decimal? Gr500 { get; set; }
    public decimal? Gr1000 { get; set; }
    public decimal? Gr2000 { get; set; }
    public decimal? Gr3000 { get; set; }
    public decimal? Gr5000 { get; set; }

    public decimal? Ml100 { get; set; }
    public decimal? Ml300 { get; set; }
    public decimal? Ml500 { get; set; }
    public decimal? Ml1000 { get; set; }
    public decimal? Ml2000 { get; set; }
    public decimal? Ml3000 { get; set; }
    public decimal? Ml5000 { get; set; }

    public decimal? One { get; set; }
    public decimal? Two { get; set; }
    public decimal? Three { get; set; }
    public decimal? Five { get; set; }
    public int DiscountPercent { get; set; }
}