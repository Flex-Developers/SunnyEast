namespace Application.Contract.Product.Commands;

public record UnlinkProductImageByUrlCommand(string Url) : IRequest<int>;