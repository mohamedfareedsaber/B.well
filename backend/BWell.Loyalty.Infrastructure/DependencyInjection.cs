using System.Text;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Application.Validation;
using BWell.Loyalty.Infrastructure.Data;
using BWell.Loyalty.Infrastructure.Options;
using BWell.Loyalty.Infrastructure.Payments;
using BWell.Loyalty.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BWell.Loyalty.Infrastructure;

public static class DependencyInjection
{
    public const string AdminPolicy = "AdminOnly";
    public const string LoginRateLimit = "login";
    public const string PaymentRateLimit = "payments";
    public const string GlobalRateLimit = "global";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymobOptions>(configuration.GetSection(PaymobOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

        services.AddDbContext<BWellDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpClient("Paymob", (sp, client) =>
        {
            var paymob = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymobOptions>>().Value;
            client.BaseAddress = new Uri(paymob.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddScoped<ILoyaltyProgramService, LoyaltyProgramService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IPatientPortalService, PatientPortalService>();
        services.AddScoped<IPaymobPaymentGateway, PaymobPaymentGateway>();

        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be configured with at least 32 characters.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("""{"success":false,"message":"Authentication required."}""");
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("""{"success":false,"message":"Forbidden. Admin role required."}""");
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireAuthenticatedUser().RequireRole(nameof(Domain.Enums.UserRole.Admin)));
        });

        return services;
    }
}
