using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.Otp;
using Application.Contract.User.Responses;
using Application.Contract.Verification.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Features.Verification.Commands;

public sealed class ConfirmRegistrationCommandHandler(
    IApplicationDbContext db,
    IOtpService otp,
    IOptions<OtpOptions> options,
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwt) : IRequestHandler<ConfirmRegistrationCommand, JwtTokenResponse>
{
    private readonly OtpOptions _opt = options.Value;

    public async Task<JwtTokenResponse> Handle(ConfirmRegistrationCommand request, CancellationToken ct)
    {
        var phone = otp.NormalizePhoneE164(request.Phone);
        var now = DateTimeOffset.UtcNow;

        var rec = await db.PhoneOtps
            .Where(x => x.PhoneE164 == phone && x.Purpose == "register")
            .OrderByDescending(x => x.LastSentAt)
            .FirstOrDefaultAsync(ct);

        if (rec is null || rec.ExpiresAt < now)
            throw new InvalidOperationException("Код истёк. Запросите новый.");

        if (!otp.Verify(request.Code, rec.CodeHash, rec.Salt))
        {
            rec.Attempts++;
            if (rec.Attempts >= _opt.MaxAttempts)
            {
                rec.BlockedUntil = now.AddMinutes(_opt.BlockMinutesOnFail);
            }
            await db.SaveChangesAsync(ct);
            throw new InvalidOperationException("Неверный код.");
        }

        // валидный код — создаём пользователя
        var exists = await userManager.FindByNameAsync(request.Email);
        if (exists is not null)
            throw new InvalidOperationException("Пользователь с таким email уже существует.");

        var fullName = string.IsNullOrWhiteSpace(request.FullName)
            ? request.Email                  
            : request.FullName.Trim();

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email    = request.Email,
            PhoneNumber = phone,
            PhoneNumberConfirmed = true,
            Name = fullName                
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
            throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => e.Description)));

        // Сразу выдаём JWT
        var jwtToken = await jwt.GenerateAsync(user); // используй твой IJwtTokenService
        return jwtToken;
    }
}