# DDMS Backend — Coding Standards

Rule code áp dụng cho mọi PR mới. Vi phạm = reviewer reject. Reviewer dùng checklist cuối file để verify.

---

## 0. Cấu trúc thư mục

```
DDMS.Backend/
│
├── Program.cs                          # Composition root — DI + middleware pipeline
├── appsettings.json                    # KHÔNG chứa secret
├── DDMS.Backend.csproj
│
├── Controllers/                        # ❶ HTTP layer
│   ├── OwnerBillingController.cs
│   ├── AdminPromotionsController.cs
│   └── ... (1 file = 1 resource, đặt theo audience prefix Admin/Owner/Public)
│
├── Services/                           # ❷ Business logic
│   ├── Interfaces/                     # IXxxService.cs (1 interface / 1 file)
│   │   ├── IBillingService.cs
│   │   ├── IBookingService.cs
│   │   └── ...
│   └── Implementations/                # XxxService.cs
│       ├── BillingService.cs
│       ├── BookingService.cs
│       └── ...
│
├── Repositories/                       # ❸ Data access — duy nhất nơi chạm AppDbContext
│   ├── Interfaces/
│   │   ├── IBookingRepository.cs
│   │   └── ...
│   └── Implementations/
│       ├── BookingRepository.cs
│       └── ...
│
├── Models/                             # ❹ Dữ liệu thuần
│   ├── Entities/                       # Map 1-1 với bảng DB (snake_case theo Pomelo)
│   │   ├── booking.cs
│   │   ├── boat.cs
│   │   └── ...
│   ├── DTOs/                           # Request/Response, gom theo module
│   │   ├── Billing/
│   │   │   └── FinancialSummaryResponse.cs
│   │   ├── Booking/
│   │   │   ├── BookingRequests.cs
│   │   │   └── BookingResponses.cs
│   │   ├── Promotions/
│   │   │   └── PromotionModels.cs
│   │   └── ...
│   └── (KHÔNG có Services/ hoặc Repositories/ ở đây)
│
├── Common/                             # ❺ Cross-cutting, dùng được từ mọi layer
│   ├── Constants/
│   │   ├── BookingStatuses.cs
│   │   ├── PromotionStatuses.cs
│   │   ├── DashboardBuckets.cs
│   │   └── ... (1 file = 1 nhóm constant theo domain)
│   ├── Identity/
│   │   ├── ICurrentUser.cs             # Thay GetCurrentUserId() ở mọi controller
│   │   └── CurrentUser.cs
│   ├── Exceptions/
│   │   ├── ErrorCode.cs                # const int code + const string message
│   │   ├── AppException.cs             # + NotFoundException/ValidationException/...
│   │   └── GlobalExceptionMiddleware.cs
│   ├── Responses/
│   │   ├── ApiResponse.cs              # { code, result } — success wrapper
│   │   ├── ApiErrorResponse.cs         # { code, message, fieldErrors }
│   │   └── PagedResponse.cs
│   ├── Localization/
│   │   ├── IErrorMessageLocalizer.cs
│   │   └── ErrorMessageLocalizer.cs
│   └── Validators/
│       └── (FluentValidation custom rules)
│
├── Configurations/                     # ❻ Options pattern + DI bootstrap
│   ├── DependencyInjection.cs          # AddProjectDependencies() — register mọi service/repo
│   ├── JwtOptions.cs
│   ├── BillingOptions.cs               # Có [Required]/[Range] + ValidateOnStart
│   ├── PayOSOptions.cs
│   └── ...
│
├── Data/                               # EF Core
│   └── AppDbContext.cs
│
├── Migrations/                         # EF migrations auto-gen
│
├── Hubs/                               # SignalR — gọi Service, KHÔNG gọi DbContext
│   ├── BillingHub.cs
│   └── ChatHub.cs
│
├── Extensions/                         # Builder extensions cho Program.cs
│   ├── JwtBearerExtensions.cs
│   └── SwaggerExtensions.cs
│
├── Resources/                          # i18n
│   ├── TourResources.cs
│   ├── TourResources.resx
│   └── TourResources.vi.resx
│
└── docs/
    └── (project-specific docs — gitignored hiện tại)
```

**Quy ước đặt file**:
- 1 interface = 1 file. KHÔNG nhét nhiều interface vào 1 file (trừ khi 1 cặp đôi cùng module bắt buộc đi chung như `IOwnerPromotionsService` + `IAdminPromotionsService`).
- 1 impl = 1 file. Mapper/helper private có thể nằm cùng file impl (vd `PromotionMapper` trong `PromotionsService.cs`).
- DTO gom theo module: `{Module}Models.cs` chứa nhiều DTO liên quan, HOẶC tách `{Module}Requests.cs` + `{Module}Responses.cs` nếu nhiều.
- Constants: 1 file = 1 nhóm domain. Đặt nhiều `public static class` trong 1 file nếu liên quan (vd `OwnerProfileStatuses` + `BoatStatuses` + `RoleNames` cùng `OwnerVerificationStatuses.cs` vì cùng phục vụ flow xác thực owner).

---

## 1. Kiến trúc 3-layer (MVC)

**Quy tắc phụ thuộc 1 chiều, không vi phạm:**

```
Controllers → Services → Repositories → Data (AppDbContext)
       ↓            ↓             ↓
       └──────→ Models (DTOs + Entities)
       └──────→ Common (Constants/Identity/Exceptions/Responses)
```

| Layer | Trách nhiệm | CẤM |
|---|---|---|
| **Controllers/** | HTTP routing, model binding, `[Authorize]`, trả `IActionResult` | `using DDMS.Backend.Data`, chạm `AppDbContext`, business logic, try/catch |
| **Services/** | Business logic, validate nghiệp vụ, orchestrate transaction | `using Data` (chỉ qua repository), EF Core query trực tiếp |
| **Repositories/** | EF Core query, persistence | Business logic, gọi service khác |
| **Models/** | Entities + DTOs (dữ liệu thuần) | Hành vi I/O, gọi DB/HTTP |

**Test nhanh**: nếu mở controller mà thấy `Include().ThenInclude()` hoặc `_context.x.Add()` → SAI.

### 1.1. Quy tắc gọi giữa các module (cross-module dependency)

**Cho phép**:
- ✅ Controller A inject **Service A** (cùng module) — luôn luôn.
- ✅ Service A inject **Repository A** (cùng module) — luôn luôn.
- ✅ Service A inject **Service B của module khác** — CHỈ khi cần orchestrate use-case cross-module. Vd `OwnerServicesRegistrationService` inject `ITourService` để tạo tour kèm theo.
- ✅ Service A inject **Repository B của module khác** — CHỈ khi B là *shared infrastructure* không có service riêng (vd `IWalletRepository` — wallet không có business riêng, chỉ là persistence). Phải có lý do rõ ràng.
- ✅ Controller A inject **nhiều Service** (cùng audience prefix Admin/Owner/Public) — vd `OwnerBoatsController` inject `IBoatService` + `IBoatImageService` + `IBoatMaintenanceService`.

**CẤM**:
- ❌ Controller A inject **Repository** (kể cả cùng module). Repository chỉ qua Service.
- ❌ Repository A inject **Repository B**. Repository không có dependency ngang. Cần data từ nhiều repo → ghép ở Service.
- ❌ Repository A inject **Service B**. Hướng ngược chiều dependency.
- ❌ Service A inject **Service A** (tự tham chiếu) hoặc cycle (A → B → A).
- ❌ Hub (SignalR) inject **Repository / AppDbContext**. Hub là transport, gọi Service.

### 1.2. Khi nào dùng *shared repository*?

`IWalletRepository`, `IPromotionsRepository` là ví dụ shared. Tiêu chí:
- Entity của module đó **không có business logic riêng** đáng kể (chỉ CRUD trên 1 bảng).
- Nhiều service khác nhau đụng cùng entity (vd `BookingService.RefundToWallet` + `OwnerToursDashboardService.RefundToWallet` + `AdminWithdrawalsService.Refund` đều đụng `user_wallet`).

→ Tách `IWalletRepository` riêng, **không** tạo `IWalletService`. Các service nghiệp vụ tự dùng repo.

Ngược lại, nếu module có business rule riêng (`IBillingService` tính commission, validate PayOS), **phải** có service che repository, các module khác gọi qua service.

### 1.3. DTO cross-module

- DTO của module A nằm ở `Models/DTOs/{ModuleA}/`. Module B **có thể** `using DDMS.Backend.Models.DTOs.ModuleA` để dùng — DTOs là *type*, không phải dependency nghiệp vụ.
- Nếu thấy 2 module dùng cùng 1 DTO **giống hệt** → cân nhắc move sang `Models/DTOs/Shared/` hoặc tạo base class chung. KHÔNG copy-paste.
- Vd: `MessageResponse` đang ở `Models/DTOs/Auth/` nhưng được dùng bởi nhiều module — nên move sang `Shared/` (tech debt còn lại).

### 1.4. Constants cross-module

- Constants ở `Common/Constants/` được mọi module dùng.
- Nếu chỉ 1 module dùng → vẫn để `Common/Constants/`. Constants **không** scope theo module.
- Constants với cùng concept ở các module khác nhau → đặt cùng namespace nhưng tên rõ ràng (`BoatMaintenanceStatuses.Pending` vs `BookingStatuses.Pending` — chấp nhận trùng `"pending"` string vì nghiệp vụ riêng).

### 1.5. Dependency graph mẫu (module Billing)

```
OwnerBillingController
  └─→ IBillingService ────→ IBillingRepository ──→ AppDbContext
        │                  └→ IBookingRepository (cross-module, read-only)
        ├─→ PayOSClient (3rd-party)
        ├─→ IHubContext<BillingHub> (SignalR)
        └─→ IOptions<BillingOptions> (config)
```

Hợp lệ vì:
- Controller chỉ inject Service.
- Service ghép nhiều repository (cùng + cross-module read-only).
- Repository chỉ chạm DbContext.
- PayOS/Hub/Options là dependency *external/infrastructure*, không phải layer khác.

---

## 2. Controller — chỉ HTTP

```csharp
// ✅ ĐÚNG
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePromotionRequest req, CancellationToken ct)
{
    var id = await _svc.CreateAsync(_user.Id, req, ct);
    return Ok(ApiResponse<object>.Ok(new { success = true, id }));
}

// ❌ SAI
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreatePromotionRequest req)
{
    try
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return BadRequest(new { message = "Mã giảm giá không được để trống." });

        var promo = new promotion { ... };
        _context.promotions.Add(promo);
        await _context.SaveChangesAsync();
        return Ok(...);
    }
    catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
}
```

**Không bao giờ** trong controller:
- `try/catch (Exception ex)` + `StatusCode(500, ...)` → để `GlobalExceptionMiddleware` lo
- `BadRequest(new { message = "..." })` hardcode → throw `AppException(ErrorCode.X)`
- `_context.X.Add()` / `Include()` → đẩy về repository
- `private Guid GetCurrentUserId()` → inject `ICurrentUser`
- `class XxxRequest { ... }` inline cuối file → tách ra `Models/DTOs/`

**Action method** nên fit 1–10 dòng. Nếu dài hơn → có logic ở sai layer.

---

## 3. Identity — `ICurrentUser`, KHÔNG `GetCurrentUserId()`

```csharp
// ✅ ĐÚNG
public class XxxController(IXxxService svc, ICurrentUser user) : ControllerBase
{
    [HttpGet] public Task<...> Get() => svc.GetAsync(user.Id);
}

// ❌ SAI — sao chép private helper khắp nơi
private Guid GetCurrentUserId()
{
    var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? ...;
    if (!Guid.TryParse(...)) throw new UnauthorizedException();
    return userId;
}
```

`ICurrentUser` đã có sẵn ở [Common/Identity/](../Common/Identity/). Inject là dùng.

---

## 4. Error — `ErrorCode` + `AppException`, KHÔNG hardcode message

**Mọi error message phải có code tương ứng:**

```csharp
// 1. Thêm code vào ErrorCode.cs
public const int PromotionNotFound = 1602;

public static class Messages
{
    public const string PromotionNotFound = "Không tìm thấy mã giảm giá.";
}

// 2. Service throw
var promo = await _repo.FindAsync(id, ct)
    ?? throw new NotFoundException(
        ErrorCode.PromotionNotFound,
        ErrorCode.Messages.PromotionNotFound);

// 3. Controller KHÔNG cần try/catch — GlobalExceptionMiddleware tự map:
//    NotFoundException → 404 + { code: 1602, message: "..." }
//    AppException → 400 + { code, message }
//    ValidationException → 400 + { code, message, fieldErrors }
//    UnauthorizedException → 401
//    ForbiddenException → 403
```

**CẤM:**
- `BadRequest(new { message = "Số tiền phải > 0" })` — hardcode chuỗi VN trong controller
- `StatusCode(500, new { message = ex.Message })` — leak stacktrace
- `throw new AppException(9999, "Lỗi rồi")` — code 9999 (UncategorizedError) là **last resort**, phải tạo code riêng

**Quy tắc đánh số code** (xem [ErrorCode.cs](../Common/Exceptions/ErrorCode.cs)):

| Dải | Module |
|---|---|
| 1000 | Success |
| 1100–1209 | Auth/Validation |
| 1300–1305 | Token |
| 1401–1404 | Generic 401/403/404 |
| 1500–1799 | Resource (Tour/Schedule/Route/Boat/Dock/Promotion/Withdrawal/Maintenance...) |
| 2100–2605 | Tour module (có localization qua `TourResources.resx`) |
| 9999 | UncategorizedError |

Khi thêm module mới: chọn dải 1800–1999 hoặc 2700+.

---

## 5. Magic strings/numbers → `Common/Constants/`

```csharp
// ❌ SAI
if (booking.status == "confirmed" || booking.status == "paid") { ... }
var commission = totalPrice * 0.08m;
var rentalCost = 5_000_000m;

// ✅ ĐÚNG — đã có constants/options
if (BookingStatuses.IsPaidLike(booking.status)) { ... }
var commission = totalPrice * _billing.Commission;   // BillingOptions
var rentalCost = _billing.MonthlyDockRental;
```

| File | Chứa |
|---|---|
| [BookingStatuses.cs](../Common/Constants/BookingStatuses.cs) | pending/confirmed/paid/completed/cancelled |
| [PromotionStatuses.cs](../Common/Constants/PromotionStatuses.cs) | pending/approved/rejected + DiscountTypes |
| [DashboardBuckets.cs](../Common/Constants/DashboardBuckets.cs) | status sets cho aggregation |
| [RoleNames.cs](../Common/Constants/OwnerVerificationStatuses.cs) | admin/owner role names |
| ... | tự thêm nếu module mới có status/code khác |

---

## 6. Config thay vì hardcode — Options pattern

**Giá trị nghiệp vụ có thể đổi theo env/role/tenant → KHÔNG hardcode trong code:**

```csharp
// ❌ SAI
public static class BillingRates
{
    public const decimal Commission = 0.08m;
    public const string PayOSReturnUrl = "http://localhost:5173/owner/revenue?payment=success";
}

// ✅ ĐÚNG — Options pattern + validate on startup
public class BillingOptions
{
    public const string SectionName = "Billing";

    [Range(0, 1)] public decimal Commission { get; set; }
    [Required, Url] public string PayOSReturnUrl { get; set; } = null!;
}

// Program.cs
builder.Services.AddOptions<BillingOptions>()
    .Bind(builder.Configuration.GetSection(BillingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Service
public BillingService(IOptions<BillingOptions> opts) { _billing = opts.Value; }
```

Override qua appsettings hoặc env var `Billing__Commission=0.10` (double underscore).

---

## 7. DTOs ở `Models/DTOs/{Module}/`, KHÔNG inline trong controller

```csharp
// ❌ SAI — DTO ở cuối file controller
public class WalletController : ControllerBase { ... }

public class WithdrawRequest
{
    public decimal Amount { get; set; }
}

// ✅ ĐÚNG
// Controllers/WalletController.cs
public class WalletController : ControllerBase { ... }

// Models/DTOs/Wallet/WalletModels.cs
namespace DDMS.Backend.Models.DTOs.Wallet;
public class WithdrawRequest { ... }
public class WalletBalanceResponse { ... }
```

**Quy tắc:**
- 1 module = 1 folder `Models/DTOs/{Module}/`
- Request: `{Verb}{Resource}Request` — `CreateBookingRequest`, `UpdateProfileRequest`
- Response: `{Resource}{Purpose}Response` — `BookingDetailResponse`, `FinancialSummaryResponse`
- DTO `record` cho immutable (vd `WebhookHandleResult`), `class` khi cần `set`

---

## 8. i18n — BE KHÔNG hardcode tiếng Việt cho display

```csharp
// ❌ SAI — BE quyết display string
CreatorName = creator?.full_name ?? "Hệ thống",
CreatorRole = ... ? "admin" : "owner",

// ✅ ĐÚNG — BE trả null, FE quyết display theo locale
public string? CreatorName { get; set; }   // null khi không xác định
public string? CreatorRole { get; set; }   // null khi không xác định
```

**Ngoại lệ**: error message trong `ErrorCode.Messages` — đã được middleware route qua `TourResources.resx` (cho Tour module) hoặc trả thẳng (module khác). Khi i18n hoá toàn bộ thì mở rộng `IErrorMessageLocalizer`.

---

## 9. Async / CancellationToken

- **Mọi method service/repository chạm I/O** phải `async Task<T>` + nhận `CancellationToken`.
- **Mọi action controller** nhận `CancellationToken ct` và truyền xuống service.
- Tên: hậu tố `Async`.

```csharp
// ✅
public Task<List<BookingItem>> GetUserBookingsAsync(Guid userId, CancellationToken ct) =>
    _db.bookings.Where(...).ToListAsync(ct);

// ❌ — không có CT, request kéo dài không hủy được
public Task<List<BookingItem>> GetUserBookingsAsync(Guid userId) =>
    _db.bookings.Where(...).ToListAsync();
```

---

## 10. Naming convention

| Loại | Pattern | Ví dụ |
|---|---|---|
| Controller | `{Audience}{Resource}Controller` | `OwnerBillingController`, `AdminPromotionsController` |
| Service interface | `I{Resource}Service` | `IBillingService`, `IAdminPromotionsService` |
| Service impl | `{Resource}Service` | `BillingService` |
| Repository interface | `I{Entity}Repository` | `IBookingRepository` |
| Repository impl | `{Entity}Repository` | `BookingRepository` |
| Constants | `{Domain}{Concept}` | `BookingStatuses`, `BillingRates` |
| Options | `{Domain}Options` | `BillingOptions`, `JwtOptions` |
| Error code field | PascalCase, mô tả ngắn | `PromotionCodeRequired`, `WithdrawInsufficientBalance` |

---

## 11. DI registration

- 1 module = 2 dòng kế tiếp nhau trong [`Configurations/DependencyInjection.cs`](../Configurations/DependencyInjection.cs):
  ```csharp
  services.AddScoped<IBillingRepository, BillingRepository>();
  services.AddScoped<IBillingService, BillingService>();
  ```
- KHÔNG đăng ký rải rác trong `Program.cs` nữa (legacy DI đang được dọn dần).
- Scope mặc định = `Scoped` (per request). Singleton chỉ cho stateless service không chạm DI Scope.

---

## 12. Secrets

**TUYỆT ĐỐI KHÔNG commit:**
- `appsettings.json` chứa giá trị thật của PayOS/Cloudinary/DB password/Jwt secret
- `appsettings.Development.json` với secrets
- `Properties/launchSettings.json` với env vars chứa secret
- File `.env`, `secrets.json`, `*.pem`, `*.key`

**Cách dùng đúng:**
- HEAD commit: `appsettings.json` có **placeholder rỗng** (`""`), section structure đầy đủ
- Local dev: secrets qua **User Secrets** (`dotnet user-secrets set "PayOS:ClientId" "..."`)
- Production: secrets qua **env var** (`PayOS__ClientId=...`)

---

## ✅ PR Checklist (paste vào PR description)

```markdown
- [ ] Controller mỏng (≤10 dòng/action, không try/catch, không hardcode message)
- [ ] Không chạm `AppDbContext` trong Controller / Service
- [ ] Identity qua `ICurrentUser`, không có `GetCurrentUserId()` private
- [ ] DTOs ở `Models/DTOs/{Module}/`, không inline trong controller
- [ ] Mọi error message có `ErrorCode` + `ErrorCode.Messages.X` tương ứng
- [ ] Service throw `AppException`/`NotFoundException`/`ValidationException`/`ForbiddenException` — không `BadRequest(new {...})` trong controller
- [ ] Magic strings/numbers → `Common/Constants/` hoặc `IOptions<XxxOptions>`
- [ ] Async methods nhận `CancellationToken`
- [ ] DI register trong `Configurations/DependencyInjection.cs`
- [ ] File đặt đúng folder (`Services/Interfaces/`, `Repositories/Implementations/`, `Models/DTOs/{Module}/`, `Common/Constants/`)
- [ ] Cross-module dependency hợp lệ (Controller→Service, Service→Repository; KHÔNG Controller→Repository, KHÔNG Repo→Repo, KHÔNG Repo→Service)
- [ ] Build xanh, 0 error, không phát sinh warning mới
- [ ] Không commit secrets (`appsettings*.json` chỉ placeholder)
```
