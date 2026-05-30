# Cấu hình bí mật (không commit lên Git)

Tài liệu này hướng dẫn cấu hình **Gmail SMTP**, **JWT**, **database** bằng **.NET User Secrets** (chỉ lưu trên máy bạn, không vào Git).

---

## 1. Tạo Gmail App Password

1. Đăng nhập Google Account → [Bảo mật](https://myaccount.google.com/security)
2. Bật **Xác minh 2 bước** (bắt buộc)
3. Tìm **Mật khẩu ứng dụng** (App passwords)
4. Tạo mật khẩu cho ứng dụng **Mail** / thiết bị **Other (DDMS)**
5. Google trả về **16 ký tự** (dạng `xxxx xxxx xxxx xxxx`) — đây là `smtpPassword`, **không** dùng mật khẩu đăng nhập Gmail thường

---

## 2. Lưu cấu hình bằng User Secrets (khuyến nghị)

Mở terminal tại thư mục `DDMS.Backend`:

```powershell
cd D:\PROJECT_CAPSTONE_2026\BE\ddms-backend\DDMS.Backend

# Gmail SMTP
dotnet user-secrets set "Email:useSmtp" "true"
dotnet user-secrets set "Email:fromAddress" "your.email@gmail.com"
dotnet user-secrets set "Email:fromName" "DDMS"
dotnet user-secrets set "Email:smtpHost" "smtp.gmail.com"
dotnet user-secrets set "Email:smtpPort" "587"
dotnet user-secrets set "Email:smtpUser" "your.email@gmail.com"
dotnet user-secrets set "Email:smtpPassword" "abcdefghijklmnop"

# JWT (dev) — chuỗi dài, ngẫu nhiên
dotnet user-secrets set "Jwt:secretKey" "your-long-random-dev-secret-key"

# MySQL (bắt buộc nếu ConnectionStrings trong appsettings để trống)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=boat_tour;user=root;password=YOUR_DB_PASSWORD;"
```

Xem secrets đã lưu (không hiện trong repo):

```powershell
dotnet user-secrets list
```

File thật nằm ngoài project, ví dụ:

`%APPDATA%\Microsoft\UserSecrets\ddms-backend-dev-secrets\secrets.json`

---

## 3. Chạy API

- F5 / `dotnet run` profile **https**
- Đăng ký tài khoản → email xác thực gửi tới Gmail (kiểm tra cả **Spam**)
- Khi `useSmtp: true`, API **không** trả `verificationLink` trong response (chỉ gửi mail)

---

## 4. KHÔNG được commit lên Git

| Thứ | Lý do | Cách lưu an toàn |
|-----|--------|------------------|
| `Email:smtpPassword` | Gmail App Password | User Secrets |
| `Jwt:secretKey` | Ký JWT, lộ = giả mạo token | User Secrets / biến môi trường server |
| `ConnectionStrings` (password DB) | Truy cập database | User Secrets / env production |
| File `.env` (FE) | `VITE_*` có thể lộ client | `.gitignore` (đã có) — chỉ commit `.env.example` |
| `secrets.json` tự tạo trong project | Backup secrets | Không commit; dùng User Secrets |
| `appsettings.Local.json` | Override local | Thêm vào `.gitignore` |
| Mật khẩu trong `appsettings.Development.json` | Dễ lộ qua Git history | Xóa khỏi file, chuyển User Secrets |

### Được commit (không nhạy cảm hoặc public)

| Thứ | Ghi chú |
|-----|---------|
| `appsettings.json` | Cấu hình mẫu, `secretKey` để trống |
| `appsettings.Development.example.json` | Chỉ placeholder |
| `smtpHost`, `smtpPort` | `smtp.gmail.com`, `587` |
| `Google:clientId` | OAuth client ID (public trên FE) |
| `VITE_GOOGLE_CLIENT_ID` trong `.env.example` | Client ID public |

### Frontend (`ddms-frontend`)

| Commit? | File |
|---------|------|
| ❌ | `.env`, `.env.local`, `.env.*.local` |
| ✅ | `.env.example` (không có secret thật) |

---

## 5. Nếu đã lỡ commit secret

1. **Đổi ngay** App Password / JWT / DB password
2. Xóa secret khỏi file commit
3. Cân nhắc `git filter-repo` hoặc rotate credentials (history vẫn có thể còn secret cũ)

---

## 6. Production

Dùng biến môi trường trên server (không User Secrets):

```bash
Email__useSmtp=true
Email__smtpHost=smtp.gmail.com
Email__smtpUser=...
Email__smtpPassword=...
Jwt__SecretKey=...
ConnectionStrings__DefaultConnection=...
```

Hoặc Azure Key Vault / AWS Secrets Manager.
