namespace Application.Contract.Order.Commands;

public class UpdateCartItemCommand : IRequest<string>
{
    public required string Slug { get; set; }
    public int? Gr100SelectedCount { get; set; }
    public int? Gr300SelectedCount { get; set; }
    public int? Gr500SelectedCount { get; set; }
    public int? Gr1000SelectedCount { get; set; }
    public int? Gr2000SelectedCount { get; set; }
    public int? Gr3000SelectedCount { get; set; }
    public int? Gr5000SelectedCount { get; set; }

    public int? Ml100SelectedCount { get; set; }
    public int? Ml300SelectedCount { get; set; }
    public int? Ml500SelectedCount { get; set; }
    public int? Ml1000SelectedCount { get; set; }
    public int? Ml2000SelectedCount { get; set; }
    public int? Ml3000SelectedCount { get; set; }
    public int? Ml5000SelectedCount { get; set; }

    public int? OneSelectedCount { get; set; }
    public int? TwoSelectedCount { get; set; }
    public int? ThreeSelectedCount { get; set; }
    public int? FiveSelectedCount { get; set; }
}