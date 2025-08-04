using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces.Services.Otp;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Otp;

public sealed class OtpService(IOptions<OtpOptions> options) : IOtpService
{
    private readonly OtpOptions _opt = options.Value;

    public string GenerateNumericCode(int length)
    {
        var digits = "0123456789";
        var data = RandomNumberGenerator.GetBytes(length);
        return new string(data.Select(b => digits[b % 10]).ToArray());
    }

    public (string Hash, string Salt) HashCode(string code)
    {
        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        return (Hash(code, salt), salt);
    }

    public bool Verify(string code, string hash, string salt) => Hash(code, salt) == hash;

    private static string Hash(string code, string salt)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(code + salt);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    public string NormalizePhoneE164(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("8") && digits.Length == 11) digits = "7" + digits[1..];
        if (digits.Length == 10) digits = "7" + digits;
        return "+" + digits;
    }
}