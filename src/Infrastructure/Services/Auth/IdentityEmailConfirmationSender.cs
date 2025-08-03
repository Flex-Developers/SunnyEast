using System.Text;
using System.Linq;
using Application.Auth;
using Domain.Entities;
using Infrastructure.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth;

public class IdentityEmailConfirmationSender : IEmailConfirmationSender
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMailSender _mailSender;
    private readonly MailgunOptions _options;
    private readonly ILogger<IdentityEmailConfirmationSender> _logger;

    public IdentityEmailConfirmationSender(UserManager<ApplicationUser> userManager,
        IMailSender mailSender,
        IOptions<MailgunOptions> options,
        ILogger<IdentityEmailConfirmationSender> logger)
    {
        _userManager = userManager;
        _mailSender = mailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendConfirmationAsync(ApplicationUser user, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(user.Email))
            return;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var link = $"{_options.ConfirmationUrlBase}?userId={user.Id}&token={encoded}";
        var html = EmailTemplateRenderer.RenderEmailConfirmation(link);
        await _mailSender.SendAsync(user.Email, "Подтверждение регистрации", html, null, ct);
    }

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;
        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decoded);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed for {UserId}", userId);
            return true;
        }
        _logger.LogWarning("Email confirmation failed for {UserId}: {Errors}", userId,
            string.Join(",", result.Errors.Select(e => e.Description)));
        return false;
    }

    public async Task ResendConfirmationAsync(string email, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.EmailConfirmed)
            return;
        await SendConfirmationAsync(user, ct);
    }
}
