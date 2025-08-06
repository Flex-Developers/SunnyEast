using Application.Contract.Verification.Commands;
using Application.Contract.Verification.Responses;

namespace Client.Infrastructure.Services.Verification;

public interface IVerificationClient
{
    Task<CheckAvailabilityResponse> CheckAvailabilityAsync(string? email, string? phone);
    Task<StartVerificationResponse> StartAsync(StartVerificationCommand req);
    Task<ResendResponse> ResendAsync(string sessionId);
    Task<VerifyResponse> VerifyAsync(string sessionId, string code);
    Task<GetSessionStateResponse> GetStateAsync(string sessionId);
}