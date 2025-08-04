namespace Application.Common.Interfaces.Services.Otp;

public interface IOtpService
{
    string GenerateNumericCode(int length);
    (string Hash, string Salt) HashCode(string code);
    bool Verify(string code, string hash, string salt);
    string NormalizePhoneE164(string phone);
}