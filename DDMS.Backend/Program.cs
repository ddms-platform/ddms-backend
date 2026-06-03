using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Configurations;
using DDMS.Backend.Data;
using DDMS.Backend.Extensions;
using DDMS.Backend.Models.Repositories.Implementations;
using DDMS.Backend.Models.Repositories.Interfaces;
using DDMS.Backend.Models.Services.Implementations;
using DDMS.Backend.Models.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDdmsLocalization();
builder.Services.AddDdmsSwagger();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection(CloudinaryOptions.SectionName));
builder.Services.Configure<EmailVerificationOptions>(builder.Configuration.GetSection(EmailVerificationOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection(GoogleOptions.SectionName));
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection(CloudinaryOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.secretKey))
{
    throw new InvalidOperationException(
        "Jwt:SecretKey is not configured. Set Jwt__SecretKey environment variable or User Secrets.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.issuer,
            ValidAudience = jwtOptions.audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.ConfigureDdmsJwtBearer();
    });

builder.Services.AddAuthorization();

var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
var isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsOptions.PolicyName, policy =>
    {
        if (corsOptions.AllowedOrigins.Length > 0)
        {
            policy.WithOrigins(corsOptions.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else if (isDevelopment)
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, _) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ApiErrorResponse
        {
            code = ErrorCode.AuthRateLimited,
            message = ErrorCode.Messages.AuthRateLimited
        });
    };

    options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthSessionService, AuthSessionService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<ITourSearchRepository, TourSearchRepository>();
builder.Services.AddScoped<ITourSearchService, TourSearchService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<ITourImageRepository, TourImageRepository>();
builder.Services.AddScoped<ITourImageService, TourImageService>();
builder.Services.AddScoped<IFaqRepository, FaqRepository>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IDockScheduleRepository, DockScheduleRepository>();
builder.Services.AddScoped<IDockScheduleService, DockScheduleService>();

// ── Cloudinary ───────────────────────────────────────────────
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// ── Boat ─────────────────────────────────────────────────────
builder.Services.AddScoped<IBoatRepository, BoatRepository>();
builder.Services.AddScoped<IBoatCabinRepository, BoatCabinRepository>();
builder.Services.AddScoped<IBoatAddonRepository, BoatAddonRepository>();
builder.Services.AddScoped<IBoatImageRepository, BoatImageRepository>();
builder.Services.AddScoped<IBoatService, BoatService>();
builder.Services.AddScoped<IBoatCabinService, BoatCabinService>();
builder.Services.AddScoped<IBoatAddonService, BoatAddonService>();
builder.Services.AddScoped<IBoatImageService, BoatImageService>();

// ── Dock ─────────────────────────────────────────────────────
builder.Services.AddScoped<IDockRepository, DockRepository>();
builder.Services.AddScoped<IDockService, DockService>();

var app = builder.Build();

app.UseRequestLocalization();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BoatTour API v1");
        options.RoutePrefix = "";
    });
}

app.UseHttpsRedirection();
app.UseCors(CorsOptions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
