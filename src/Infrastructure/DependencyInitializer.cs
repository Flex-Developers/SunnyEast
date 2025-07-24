using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces.Contexts;
using Application.Common.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public static class DependencyInitializer
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configurations)
    {
        var mySqlConnectionString = configurations.GetConnectionString("mySql");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (configurations["UseInMemoryDatabase"] == "True")
                options.UseInMemoryDatabase("testDb");
            else
                options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString));
        });
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<IPriceCalculatorService, PriceCalculatorService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();
        services.AddSingleton<IVolumeGroupService, VolumeGroupService>();
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(identityOptions =>
            {
                identityOptions.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
                identityOptions.ClaimsIdentity.UserNameClaimType = ClaimTypes.NameIdentifier;
                identityOptions.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;

                identityOptions.Password.RequiredLength = 8;
                identityOptions.Password.RequiredUniqueChars = 4;
                identityOptions.Password.RequireNonAlphanumeric = false;
                identityOptions.Password.RequireLowercase = true;
                identityOptions.Password.RequireUppercase = true;
                identityOptions.Password.RequireDigit = true;

                identityOptions.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(op =>
        {
            op.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configurations["JWT:Secret"]!)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
    }
}