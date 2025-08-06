namespace Application.Contract.Verification.Responses;

public sealed record CheckAvailabilityResponse(bool PhoneAvailable, bool EmailAvailable);