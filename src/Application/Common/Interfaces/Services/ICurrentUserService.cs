﻿namespace Application.Common.Interfaces.Services;

public interface ICurrentUserService
{
    public string? GetUserName();
    public Guid GetUserId();
}