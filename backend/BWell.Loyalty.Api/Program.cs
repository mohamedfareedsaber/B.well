using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BWell.Loyalty.Api.Middleware;
using BWell.Loyalty.Application.Validation;
using BWell.Loyalty.Infrastructure;
using BWell.Loyalty.Infrastructure.Data;
using BWell.Loyalty.Infrastructure.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Paymob.local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

var security = builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"success":false,"message":"Too many requests. Please try again later."}""", token);
    };

    options.AddFixedWindowLimiter(DependencyInjection.GlobalRateLimit, opt =>
    {
        opt.PermitLimit = security.GlobalPermitLimit;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter(DependencyInjection.LoginRateLimit, opt =>
    {
        opt.PermitLimit = security.LoginPermitLimit;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter(DependencyInjection.PaymentRateLimit, opt =>
    {
        opt.PermitLimit = security.PaymentPermitLimit;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

var cors = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(cors.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "B.Well Loyalty API v1"));
}
else
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseRateLimiter();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting(DependencyInjection.GlobalRateLimit);

try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    logger.LogError(ex, "Database migration/seed failed. Check SQL Server connection string.");
    throw;
}

app.Run();

public partial class Program;
