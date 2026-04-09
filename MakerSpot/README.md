# 🚀 MakerSpot

**MakerSpot** là website cộng đồng giúp khám phá và chia sẻ sản phẩm công nghệ, lấy cảm hứng từ [Product Hunt](https://www.producthunt.com).  
Dự án được xây dựng bằng **ASP.NET Core MVC** + **Entity Framework Core** + **SQL Server** + **Bootstrap 5**.

---

## 📋 Mục lục

- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Hướng dẫn cài đặt](#hướng-dẫn-cài-đặt)
- [Tài khoản mẫu](#tài-khoản-mẫu)
- [Chức năng chính](#chức-năng-chính)
- [Phân quyền](#phân-quyền)
- [Kỹ thuật nổi bật](#kỹ-thuật-nổi-bật)

---

## ⚙️ Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Backend | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| CSDL | SQL Server (LocalDB / Express) |
| Frontend | Razor Views + Bootstrap 5 + Bootstrap Icons |
| Authentication | Cookie Authentication (Claims-based) |
| Password Hashing | ASP.NET Identity `PasswordHasher<User>` |

---

## 📁 Cấu trúc thư mục

```
MakerSpot/
├── Areas/
│   └── Admin/                 # Khu vực quản trị
│       ├── Controllers/       # DashboardController, ProductsController, UsersController
│       └── Views/             # Giao diện Admin (Dashboard, Products, Users)
├── Controllers/               # Controllers chính
│   ├── AuthController.cs      # Đăng nhập / Đăng ký / Đăng xuất
│   ├── HomeController.cs      # Trang chủ (Feed sản phẩm, phân trang)
│   ├── ProductController.cs   # Chi tiết SP, Upvote, Comment, Submit
│   ├── UserController.cs      # Hồ sơ cá nhân, Maker Stats
│   ├── CollectionController.cs # Bộ sưu tập
│   ├── FollowController.cs    # Follow/Unfollow user
│   └── NotificationController.cs # Thông báo
├── Models/                    # Entity classes (EF Core)
│   ├── MakerSpotContext.cs    # DbContext
│   ├── Product.cs, User.cs, Comment.cs, ...
├── ViewModels/                # Data transfer objects cho Views
├── Views/                     # Razor Views (.cshtml)
├── wwwroot/                   # Static files (CSS, JS, images)
├── Program.cs                 # Entry point & Middleware pipeline
├── appsettings.json           # Connection string & config
└── MakerSpot.csproj           # Project file
```

---

## 🛠️ Hướng dẫn cài đặt

### Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) hoặc LocalDB
- SQL Server Management Studio (SSMS) — khuyến khích

### Các bước

1. **Clone repository:**
   ```bash
   git clone <repo-url>
   cd MakerSpot
   ```

2. **Cấu hình Connection String:**  
   Mở `appsettings.json`, sửa `DefaultConnection` cho đúng Server Name của bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TEN_MAY\\SQLEXPRESS;Database=MakerSpot;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Tạo Database:**  
   Mở SSMS, import file SQL Script (nếu có) hoặc chạy Migration:
   ```bash
   dotnet ef database update
   ```

4. **Chạy Project:**
   ```bash
   dotnet run
   ```
   Truy cập: **http://localhost:5002**

---

## 👤 Tài khoản mẫu

| Role | Username | Password | Ghi chú |
|---|---|---|---|
| **Admin** | `admin` | `admin123` | Full quyền quản trị |
| **Moderator** | `mod1` | `mod123` | Duyệt sản phẩm, quản lý nội dung |
| **Member** | `user_*` | `123456` | Tài khoản người dùng mẫu (50 tài khoản) |

> **Lưu ý:** Mật khẩu được hash bằng `PasswordHasher<User>`, không lưu plaintext trong DB.

---

## 🎯 Chức năng chính

### 🏠 Trang chủ (Home Feed)
- Xem danh sách sản phẩm đã duyệt
- Sắp xếp: **Trending** (theo Upvote) / **Mới nhất** (theo ngày)
- Tìm kiếm theo tên và tagline
- Lọc theo Chủ đề (Topics)
- **Phân trang 20 sản phẩm/trang**

### 📦 Chi tiết Sản phẩm
- Xem thông tin đầy đủ: mô tả, media, topics, makers
- Upvote / Bỏ Upvote (toggle, cập nhật atomic)
- Bình luận và trả lời bình luận (nested comments)
- Thêm vào Bộ sưu tập cá nhân

### 🚀 Gửi sản phẩm (Submit)
- Form đăng sản phẩm mới với validation đầy đủ
- Chọn Topics, điền URL, mô tả
- Sản phẩm vào trạng thái **Pending** chờ Admin/Mod duyệt
- Tự động gán user hiện tại làm **Founder**

### 👤 Hồ sơ cá nhân (Profile)
- Xem danh sách sản phẩm đã đăng, đã upvote
- Xem Bộ sưu tập (Collections)
- **Maker Stats:** Tổng Upvotes nhận, Tổng Comments, Sản phẩm xịn nhất
- Follow / Unfollow user khác
- Social links (Website, Twitter, LinkedIn)

### 🔔 Thông báo (Notifications)
- Thông báo khi có người Upvote hoặc Comment sản phẩm
- Đánh dấu đã đọc (từng cái hoặc tất cả)

### 📂 Bộ sưu tập (Collections)
- Tạo bộ sưu tập cá nhân (public/private)
- Thêm/xóa sản phẩm vào collection
- Xem chi tiết collection

---

## 🔐 Phân quyền (Role-Based)

| Chức năng | Member | Moderator | Admin |
|---|:---:|:---:|:---:|
| Xem/Tìm kiếm sản phẩm | ✅ | ✅ | ✅ |
| Upvote / Comment | ✅ | ✅ | ✅ |
| Submit sản phẩm | ✅ | ✅ | ✅ |
| Follow / Collections | ✅ | ✅ | ✅ |
| Truy cập Admin Panel | ❌ | ✅ | ✅ |
| Duyệt / Từ chối sản phẩm | ❌ | ✅ | ✅ |
| Quản lý Users | ❌ | ❌ | ✅ |
| Xem tất cả sản phẩm | ❌ | ❌ | ✅ |
| Dashboard thống kê đầy đủ | ❌ | ✅ | ✅ |

### Admin Dashboard bao gồm:
- Tổng số Users, Products, Upvotes, Comments, Collections
- Biểu đồ trạng thái sản phẩm (Approved/Pending/Rejected/Hidden)
- Top 5 Topics phổ biến nhất
- Bảng xếp hạng: Top Products, Top Makers, Top Followed Users

---

## 🏗️ Kỹ thuật nổi bật

| Pattern | Mô tả |
|---|---|
| **PRG (Post-Redirect-Get)** | Tất cả form POST đều redirect sau khi xử lý, tránh duplicate submission |
| **Atomic Counter Update** | `ExecuteUpdateAsync` cho UpvoteCount/CommentCount — tránh race condition |
| **AsNoTracking** | Dùng cho tất cả trang chỉ đọc để giảm overhead EF Core |
| **Phân trang** | Home Feed và Admin Product List phân trang 20 items/page |
| **Anti-CSRF** | `[ValidateAntiForgeryToken]` cho mọi POST action |
| **Open Redirect Prevention** | Kiểm tra `Url.IsLocalUrl()` trước khi redirect |
| **Double Submit Prevention** | JavaScript disable nút submit sau click đầu tiên |
| **Cookie Authentication** | Claims-based auth với Role support |
| **Soft Delete** | Comments dùng `IsDeleted` flag thay vì xóa vật lý |

---

## 📄 License

Dự án được phát triển phục vụ mục đích học tập (Đồ án môn học).
