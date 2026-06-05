# DDMS Backend — Tài liệu triển khai & giải thích thiết kế

> Tài liệu này mô tả cách code phần API Tour/Schedule/Search/Content của module **KIỆT**, tuân thủ **DDMS Backend Rule Checklist v2**, và lý do chọn từng quyết định kỹ thuật.

---

## 1. Tổng quan kiến trúc

### 1.1 Flow chuẩn

```
React (FE)
   ↓ HTTP Request (JSON)
Controller        ← nhận request, validate (FluentValidation), gọi service, trả response
   ↓
Service           ← business logic, throw exception có errorCode
   ↓
Repository        ← query DB qua EF Core (AppDbContext)
   ↓
MySQL (boat_tour)
   ↓
GlobalExceptionMiddleware ← bắt exception, trả JSON lỗi chuẩn (giống nhánh Auth)
```

### 1.2 Vì sao phân tầng như vậy?

| Lý do | Giải thích |
|-------|------------|
| **Tách trách nhiệm** | Mỗi layer chỉ làm một việc → dễ đọc, dễ test, dễ review PR |
| **Team scale** | Nhiều dev có thể sửa Service/Repository song song mà ít conflict |
| **Đổi DB/API độc lập** | Đổi schema DB chỉ ảnh hưởng Repository; đổi contract API chỉ ảnh hưởng DTO + Controller |
| **Rule team** | Checklist v2 bắt buộc không vi phạm layering |

---

## 2. Cấu trúc thư mục

```
DDMS.Backend/
├── Controllers/              # API endpoints (mỏng, không business logic)
├── Services/
│   ├── Interfaces/           # Contract cho business layer
│   └── Implementations/      # Logic nghiệp vụ
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/      # Truy vấn EF Core
├── Models/
│   ├── Entities/             # Map bảng DB (scaffold sẵn, không expose ra API)
│   └── DTOs/                 # Request/Response cho API
├── Common/
│   ├── Exceptions/           # ErrorCode, AppException, GlobalExceptionMiddleware
│   ├── Responses/            # ApiResponse<T>, ApiErrorResponse, PagedResponse
│   └── Validators/           # FluentValidation cho từng DTO
├── Configurations/           # DI, Swagger, Validation
├── Data/                     # AppDbContext
└── Program.cs                # Startup pipeline
```

### Vì sao không đặt Service/Repository trong `Models/`?

Rule team gợi ý MVC, nhưng project này là **Web API** (không có View). Tách `Services/` và `Repositories/` ở root giúp:

- Phân biệt rõ **data contract (DTO)** vs **business** vs **data access**
- Tránh nhầm Entity DB với object API

---

## 3. Chuẩn response API (align nhánh Auth)

### 3.1 Response thành công

```json
{
  "code": 1000,
  "result": { ... }
}
```

- FE check `code === 1000` là thành công
- Dữ liệu nằm trong `result` (TourResponse, List<TourResponse>, ...)

### 3.2 Response lỗi

```json
{
  "code": 1500,
  "message": "Tour not found"
}
```

Validation:

```json
{
  "code": 1100,
  "message": "Validation failed",
  "fieldErrors": {
    "name": ["Tour name is required"]
  }
}
```

**Vì sao dùng `code` int:**

- Đồng bộ với nhánh Auth của leader (`1000` success, `1100` validation, `15xx` tour)
- FE map `code` → i18n / UX
- `message` là fallback tiếng Anh cho dev/debug

**File liên quan:**

- `Common/Exceptions/ErrorCode.cs` — mã lỗi + message tập trung
- `Common/Responses/ApiErrorResponse.cs` — model JSON lỗi

---

## 4. Xử lý exception tập trung

### 4.1 Không try-catch trong Controller

```csharp
// Đúng — Controller chỉ gọi service
public async Task<IActionResult> Create(CreateTourRequest request, ...)
{
    var data = await _tourService.CreateAsync(request, cancellationToken);
    return Ok(ApiResponse<TourResponse>.Ok(data));
}
```

Service throw khi lỗi:

```csharp
throw new NotFoundException(ErrorCode.TourNotFound, ErrorCode.Messages.TourNotFound);
throw new AppException(ErrorCode.DockScheduleOverlap, ErrorCode.Messages.DockScheduleOverlap);
```

**Vì sao:**

- Tránh duplicate try-catch ở mọi action
- Format lỗi luôn đồng nhất
- Controller mỏng, dễ đọc (≤ 200 dòng)

### 4.2 GlobalExceptionMiddleware

`Common/Exceptions/GlobalExceptionMiddleware.cs`:

1. Bắt mọi exception chưa xử lý
2. Map `AppException` / `NotFoundException` → status + `code` + `message`
3. FluentValidation fail → `InvalidModelStateResponseFactory` trả `1100` + `fieldErrors`
4. Exception khác → `9999` UncategorizedError (500)

---

## 5. Validation request (FluentValidation)

### 5.1 Hai lớp validation

| Lớp | Vị trí | Ví dụ |
|-----|--------|-------|
| **Form/field validation** | `Common/Validators/*` | name required, price >= 0, status enum |
| **Business validation** | Service | tour tồn tại?, dock overlap?, boat tồn tại? |

**Vì sao tách:**

- Validator chạy **trước** khi vào Service (rule: validate trước business)
- Business rule cần query DB → chỉ Repository/Service mới làm được

### 5.2 Cấu hình

`Configurations/ValidationConfiguration.cs`:

- `AddFluentValidationAutoValidation()` — tự validate khi request vào action
- `InvalidModelStateResponseFactory` — trả `ApiErrorResponse` chuẩn khi fail

**Ví dụ validator:** `Common/Validators/Tour/CreateTourRequestValidator.cs`

---

## 6. DTO bắt buộc — không dùng Entity trực tiếp

### 6.1 Request DTO

`CreateTourRequest`, `UpdateTourRequest`, `TourFilterRequest`, ...

### 6.2 Response DTO

`TourResponse` — chỉ field FE cần, không lộ navigation property EF.

**Vì sao:**

- Entity (`tour`, `booking`, ...) có quan hệ phức tạp, dễ circular JSON
- API contract ổn định khi DB đổi (thêm cột internal không lộ ra API)
- Rule team cấm `Create(User user)` — bắt buộc DTO

### 6.3 Map Entity ↔ DTO

Map trong **Service** (private method hoặc `TourContentMapper`):

```csharp
private static TourResponse MapTour(tour source) => new() { ... };
```

**Vì sao không AutoMapper ngay từ đầu:** Scope nhỏ, map tay rõ ràng; team có thể thêm AutoMapper sau nếu DTO nhiều.

---

## 7. Repository layer

### 7.1 Trách nhiệm

- Chỉ CRUD/query EF Core
- Không validate nghiệp vụ
- Không format response API

### 7.2 Ví dụ

`TourRepository`:

- `GetListAsync(status, location)` — filter + `AsNoTracking()` cho read-only
- `AddAsync`, `Update`, `Delete`, `SaveChangesAsync`

`TourSearchRepository`:

- Join `tour_schedules` + `tours` + `boats`
- Tính `booked_people`, `remaining_capacity`
- Sort theo `price` / `rating`

`TourContentRepository`:

- `HasOverlapAsync` — query overlap thời gian dock (business rule gọi từ Service)

**Vì sao `AsNoTracking()`:** List/Search không cần track change → nhanh hơn, ít memory.

---

## 8. Service layer

### 8.1 Trách nhiệm

- Orchestrate: gọi repository, map DTO
- Business rules: tồn tại tour/boat/dock, overlap dock, invalid status filter
- Throw `AppException` với `ErrorCodes`

### 8.2 Giới hạn dòng (rule v2)

- Service ≤ 250 dòng
- `TourContentService` tách mapper ra `TourContentMapper.cs` để giữ file gọn

### 8.3 Ví dụ business rule

**TourScheduleService** — trước khi insert:

- `ExistsTourAsync`, `ExistsBoatAsync`, `ExistsDockAsync`

**TourContentService** — dock schedule:

- `HasOverlapAsync` → `DOCK_SCHEDULE_OVERLAP`

**TourService** — filter list:

- Nếu `status` query không hợp lệ → `TOUR_INVALID_STATUS`

---

## 9. Controller layer

### 9.1 Trách nhiệm

1. Nhận HTTP request (`[FromBody]`, `[FromQuery]`, route `{id}`)
2. FluentValidation tự chạy (không code validate trong controller)
3. Gọi `_service.MethodAsync`
4. `return Ok(ApiResponse.Ok(...))`

### 9.2 Không làm trong Controller

- Không `_dbContext.tours...`
- Không try-catch (trừ case đặc biệt có lý do)
- Không hardcode error string

---

## 10. Swagger documentation

### 10.1 Vấn đề

Nhóm trưởng hỏi: *"API không có description?"*

### 10.2 Giải pháp

Dùng **attribute**, không dùng comment `//` hay `///`:

```csharp
[SwaggerTag("Tour Management — CRUD tours...")]
public class TourController
{
    [SwaggerOperation(
        Summary = "Create a new tour",
        Description = "Creates tour with name, price...")]
    [HttpPost]
    public async Task<IActionResult> Create(...)
}
```

**Package:** `Swashbuckle.AspNetCore.Annotations`  
**Config:** `SwaggerConfiguration.cs` → `options.EnableAnnotations()`

**Vì sao không dùng XML `///`:**

- Team yêu cầu xóa comment `//` trong code
- Attribute gắn trực tiếp endpoint → Swagger UI hiển thị rõ, dễ maintain

---

## 11. Dependency Injection

`Configurations/DependencyInjection.cs`:

```csharp
services.AddScoped<ITourRepository, TourRepository>();
services.AddScoped<ITourService, TourService>();
// ...
```

**Vì sao Scoped:**

- Mỗi HTTP request một scope DbContext
- Repository/Service dùng chung `AppDbContext` trong request

`Program.cs` đăng ký:

- `AddDbContext<AppDbContext>`
- `AddProjectDependencies()`
- `AddRequestValidation()`
- `AddSwaggerDocumentation()`
- `UseMiddleware<ExceptionMiddleware>()`

---

## 12. Danh sách API theo task KIỆT

### 12.1 Tour Management — `api/tours`

| Method | Path | Mô tả |
|--------|------|-------|
| POST | `/api/tours` | Create tour (name, price, description, duration, location, cancel_policy) |
| PUT | `/api/tours/{id}` | Update tour |
| DELETE | `/api/tours/{id}` | Delete tour |
| GET | `/api/tours/{id}` | Detail |
| GET | `/api/tours?status=&location=` | List + filter status |

### 12.2 Schedule — `api/tour-schedules`

| Method | Path | Mô tả |
|--------|------|-------|
| POST | `/api/tour-schedules` | Add schedule, assign boat/dock |
| PUT | `/api/tour-schedules/{id}` | Edit schedule |
| DELETE | `/api/tour-schedules/{id}` | Delete |
| GET | `/api/tour-schedules/tour/{tourId}` | List by tour |

### 12.3 Route — `api/routes`

| Method | Path | Mô tả |
|--------|------|-------|
| POST | `/api/routes` | Create route (start/end, sort_order) |
| PUT | `/api/routes/{id}` | Update |
| DELETE | `/api/routes/{id}` | Delete |
| GET | `/api/routes/tour/{tourId}` | List by tour |

### 12.4 Search — `api/tour-search`

| Method | Path | Query |
|--------|------|-------|
| GET | `/api/tour-search` | location, min_price, max_price, date, status, duration, sort_by, sort_desc |

Trả về: `remaining_capacity`, `booked_people`, `max_passengers`.

### 12.5 Content — `api/tour-content`

| Nhóm | Endpoints |
|------|-----------|
| Images | POST/PUT/DELETE `/images`, GET `/images/tour/{tourId}` |
| FAQs | POST/PUT/DELETE `/faqs`, GET `/faqs/tour/{tourId}` |
| Dock schedules | POST/PUT/DELETE `/dock-schedules`, GET `/dock-schedules/dock/{dockId}` |

---

## 13. Database & Entity

- DB: `boat_tour` (MySQL 8+)
- Entity: scaffold sẵn trong `Models/Entities/` (database-first)
- **Không scaffold lại** nếu schema không đổi
- Connection: `appsettings.Development.json` → `DefaultConnection`
- Health check: `GET /health/db`

**Vì sao không dùng Migration code-first:** Team dùng SQL script làm source of truth; tránh EF tự ý đổi DB production.

---

## 14. ErrorCode đã định nghĩa (Tour module — block 15xx)

| code | Constant | HTTP | Khi nào |
|------|----------|------|---------|
| `1000` | Success | 200 | Response thành công |
| `1100` | AuthValidationFailed | 400 | FluentValidation / ModelState |
| `1500` | TourNotFound | 404 | GET/PUT/DELETE tour không tồn tại |
| `1501` | TourInvalidStatus | 400 | Filter status sai |
| `1502` | TourNotExists | 400 | FK tour không có khi tạo schedule/route/content |
| `1503` | ScheduleNotFound | 404 | Schedule không tồn tại |
| `1504` | RouteNotFound | 404 | Route không tồn tại |
| `1505` | BoatNotExists | 400 | boat_id không hợp lệ |
| `1506` | DockNotExists | 400 | dock_id không hợp lệ |
| `1507` | TourImageNotFound | 404 | Ảnh tour không tồn tại |
| `1508` | FaqNotFound | 404 | FAQ không tồn tại |
| `1509` | DockScheduleNotFound | 404 | Dock schedule không tồn tại |
| `1510` | DockScheduleOverlap | 400 | Trùng slot dock |
| `9999` | UncategorizedError | 500 | Lỗi không mong đợi |

> Block `11xx`–`14xx` dùng chung với nhánh Auth khi merge (login, JWT, ...).

---

## 15. Cách chạy & test

```powershell
cd DDMS.Backend
dotnet build
dotnet run
```

- Swagger: `http://localhost:<port>/`
- DB health: `http://localhost:<port>/health/db`

**Lưu ý:** Dừng app (`Ctrl+C`) trước khi `dotnet build` nếu báo file bị lock.

---

## 16. Checklist review với thầy / nhóm trưởng

- [ ] Controller chỉ nhận request → gọi service → trả response
- [ ] Không query DB trong Controller
- [ ] Service có business logic; Repository chỉ data access
- [ ] DTO cho input/output; không trả Entity
- [ ] Lỗi có `code` int; FE map i18n theo code
- [ ] Validation chạy trước business (FluentValidation)
- [ ] Swagger có Summary + Description (`SwaggerOperation`)
- [ ] Không try-catch trong Controller
- [ ] GlobalExceptionMiddleware xử lý lỗi tập trung
- [ ] PR pass build + test

---

## 17. Hướng mở rộng (chưa làm)

| Hạng mục | Gợi ý |
|----------|-------|
| Auth JWT | `AuthController` + middleware `[Authorize]` |
| Upload Cloudinary thật | Service upload file → lưu `image_url` + `public_id` |
| Pagination | `page`, `pageSize` trong list/search |
| Unit test | Mock `ITourRepository`, test `TourService` |
| Integration test | WebApplicationFactory + test DB |

---

*Tài liệu cập nhật theo codebase DDMS.Backend — module Tour (KIỆT).*
