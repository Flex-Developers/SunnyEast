﻿namespace Application.Contract.CartItem.Commands;

public class CreateCartItemCommand : IRequest<string>
{
    public required string ShopOrderSlug { get; set; }
    public int Quantity { get; set; } = 1;
    public required string CartSlug { get; set; }
}