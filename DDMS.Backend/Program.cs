using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Common.Identity;
using DDMS.Backend.Common.Responses;
using DDMS.Backend.Configurations;
using DDMS.Backend.Data;
using DDMS.Backend.Extensions;
using DDMS.Backend.Repositories.Implementations;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Implementations;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DDMS.Backend.Hubs;
using DDMS.Backend.Infrastructure.Jobs;
using PayOS;

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
builder.Services.AddRequestValidation();
builder.Services.AddProjectDependencies();
builder.Services.AddSignalR();
var payOsSection = builder.Configuration.GetSection("PayOS");
builder.Services.AddSingleton(new PayOSClient(
    payOsSection["ClientId"] ?? "",
    payOsSection["ApiKey"] ?? "",
    payOsSection["ChecksumKey"] ?? ""
));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection(CloudinaryOptions.SectionName));
builder.Services.Configure<EmailVerificationOptions>(builder.Configuration.GetSection(EmailVerificationOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection(GoogleOptions.SectionName));
builder.Services.AddOptions<BillingOptions>()
    .Bind(builder.Configuration.GetSection(BillingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<BoatComplianceOptions>()
    .Bind(builder.Configuration.GetSection(BoatComplianceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHostedService<BoatComplianceBackgroundService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.secretKey))
{
    throw new InvalidOperationException(
        "Jwt:SecretKey is not configured. Set Jwt__SecretKey environment variable or User Secrets.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

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
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else if (isDevelopment)
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
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
builder.Services.AddScoped<IOwnerRegistrationService, OwnerRegistrationService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IOwnerToursRepository, OwnerToursRepository>();
builder.Services.AddScoped<IOwnerToursService, OwnerToursService>();
builder.Services.AddScoped<IOwnerRoutesRepository, OwnerRoutesRepository>();
builder.Services.AddScoped<IOwnerRoutesService, OwnerRoutesService>();
builder.Services.AddScoped<IPublicTourSearchRepository, PublicTourSearchRepository>();
builder.Services.AddScoped<IPublicTourSearchService, PublicTourSearchService>();
builder.Services.AddScoped<IPublicTourCatalogService, PublicTourCatalogService>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ITourImageRepository, TourImageRepository>();
builder.Services.AddScoped<ITourImageService, TourImageService>();
builder.Services.AddScoped<IFaqRepository, FaqRepository>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IDockScheduleRepository, DockScheduleRepository>();
builder.Services.AddScoped<IDockScheduleService, DockScheduleService>();


builder.Services.AddScoped<IBoatRepository, BoatRepository>();
builder.Services.AddScoped<IBoatCabinRepository, BoatCabinRepository>();
builder.Services.AddScoped<IBoatAddonRepository, BoatAddonRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IBoatImageRepository, BoatImageRepository>();
builder.Services.AddScoped<IBoatService, BoatService>();
builder.Services.AddScoped<IBoatCabinService, BoatCabinService>();
builder.Services.AddScoped<IBoatAddonService, BoatAddonService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IBoatImageService, BoatImageService>();


builder.Services.AddScoped<IDockRepository, DockRepository>();
builder.Services.AddScoped<IDockService, DockService>();

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();



        // Clean up duplicate docks named "Bến Du Thuyền Sông Hàn"
        var hanDocks = await dbContext.docks.Where(d => d.name == "Bến Du Thuyền Sông Hàn").ToListAsync();
        if (hanDocks.Count > 1)
        {
            var dockToKeep = hanDocks[0];
            var docksToDelete = hanDocks.Skip(1).ToList();

            var deleteDockIds = docksToDelete.Select(d => d.id).ToList();
            var schedulesToDelete = await dbContext.dock_schedules
                .Where(s => deleteDockIds.Contains(s.dock_id))
                .ToListAsync();
            dbContext.dock_schedules.RemoveRange(schedulesToDelete);
            
            dbContext.docks.RemoveRange(docksToDelete);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"[Seeding] Cleaned up {docksToDelete.Count} duplicate docks.");
        }

        // Automatically seed active dock schedules for all boats at the Han River dock
        var firstDock = await dbContext.docks.FirstOrDefaultAsync(d => d.name == "Bến Du Thuyền Sông Hàn")
                        ?? await dbContext.docks.FirstOrDefaultAsync();
        if (firstDock != null)
        {
            var now = DateTime.UtcNow;
            var allBoats = await dbContext.boats.ToListAsync();
            
            // Clean up old schedules to reset for this view
            var oldSchedules = await dbContext.dock_schedules.ToListAsync();
            dbContext.dock_schedules.RemoveRange(oldSchedules);
            await dbContext.SaveChangesAsync();

            foreach (var boat in allBoats)
            {
                dbContext.dock_schedules.Add(new DDMS.Backend.Models.Entities.dock_schedule
                {
                    id = Guid.NewGuid(),
                    dock_id = firstDock.id,
                    boat_id = boat.id,
                    start_time = now.AddDays(-1),
                    end_time = now.AddDays(5),
                    created_at = now
                });
            }
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"[Seeding] Successfully seeded active dock schedules for {allBoats.Count} boats at dock: {firstDock.name}");

            // Automatically make sure roles and admin role exist
            var adminRole = await dbContext.roles.FirstOrDefaultAsync(r => r.name == "admin");
            if (adminRole == null)
            {
                adminRole = new DDMS.Backend.Models.Entities.role { name = "admin", description = "Administrator" };
                dbContext.roles.Add(adminRole);
                await dbContext.SaveChangesAsync();
            }

            // Seed a default admin user: admin@ddms.com / Admin@123
            var adminUser = await dbContext.users.FirstOrDefaultAsync(u => u.email == "admin@ddms.com");
            if (adminUser == null)
            {
                adminUser = new DDMS.Backend.Models.Entities.user
                {
                    id = Guid.NewGuid(),
                    full_name = "System Administrator",
                    email = "admin@ddms.com",
                    password_hash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    is_active = true,
                    email_verified_at = DateTime.UtcNow,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };
                dbContext.users.Add(adminUser);
                await dbContext.SaveChangesAsync();
                Console.WriteLine("[Seeding] Created admin@ddms.com user.");
            }

            var hasAdminRole = await dbContext.user_roles.AnyAsync(ur => ur.user_id == adminUser.id && ur.role_id == adminRole.id);
            if (!hasAdminRole)
            {
                dbContext.user_roles.Add(new DDMS.Backend.Models.Entities.user_role { user_id = adminUser.id, role_id = adminRole.id });
                await dbContext.SaveChangesAsync();
                Console.WriteLine("[Seeding] Assigned admin role to admin@ddms.com.");
            }

            // Cleanup: a previous seed mistakenly granted admin to every user on each startup.
            var strayAdminRoles = await dbContext.user_roles
                .Where(ur => ur.role_id == adminRole.id && ur.user_id != adminUser.id)
                .ToListAsync();
            if (strayAdminRoles.Count > 0)
            {
                dbContext.user_roles.RemoveRange(strayAdminRoles);
                await dbContext.SaveChangesAsync();
                Console.WriteLine($"[Seeding] Removed admin role from {strayAdminRoles.Count} non-admin users.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration or seeding failed: {ex.Message}");
    }
}

app.UseRequestLocalization();
app.UseMiddleware<GlobalExceptionMiddleware>();


    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BoatTour API v1");
       
    });

// In Development, FE often calls http://localhost:5015; redirect breaks axios POST (Network Error).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(CorsOptions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<BillingHub>("/hub/billing");
app.MapHub<ChatHub>("/hub/chat");
app.MapControllers();
app.Run();
