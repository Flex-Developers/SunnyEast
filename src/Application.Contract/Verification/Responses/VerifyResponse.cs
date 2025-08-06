namespace Application.Contract.Verification.Responses;

public sealed record VerifyResponse(bool Success, string? ReturnUrl);