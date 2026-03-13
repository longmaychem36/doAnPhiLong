# MakerSpot - Phase 1

Toàn bộ code cho Phase 1 đã được tạo trong thư mục `e:\doAnPhiLong\MakerSpot`.

## Cấu trúc dự án MVC
- **Models**: Các DB Entity class (`User`, `Role`, `Product`, ...) và `MakerSpotContext` mapping 100% theo DB schema chuẩn.
- **ViewModels**: `LoginViewModel`, `RegisterViewModel`, `HomeViewModel`, `ProductDetailViewModel`.
- **Controllers**: 
  - `HomeController` (Feed, Sorting Newest/Trending).
  - `AuthController` (Đăng ký, Đăng nhập, Đăng xuất dùng Cookie Auth).
  - `ProductController` (Trang chi tiết, Upvote, Post comment/reply comment).
- **Views**: UI gọn gàng, hiện đại bằng Bootstrap 5 + các icon Bootstrap + Custom CSS (được lưu tại `wwwroot/css/site.css`).
- **appsettings.json**: Chứa chuỗi kết nối vào LocalDB (vui lòng sửa lại nếu bạn xài SQL Server cài trực tiếp thay vì LocalDB).
- **Program.cs**: Cấu hình DB Context, Cookie Authentication, Pipeline.

## Cách chạy dự án

1. Mở Terminal / PowerShell hoặc Command Prompt tại `e:\doAnPhiLong\MakerSpot`:
```bash
cd e:\doAnPhiLong\MakerSpot
```

2. Kiểm tra lại chuỗi kết nối trong `appsettings.json` (hiện đang trỏ vào `(localdb)\mssqllocaldb` với tên DB là `MakerSpot`). Nếu SQL của bạn có user/pass, hãy đổi lại:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=MakerSpot;User Id=sa;Password=yourpassword;TrustServerCertificate=True"
```

3. Chạy project:
```bash
dotnet run
```

4. Truy cập theo URL hiển thị trên terminal (ví dụ `https://localhost:7193` hoặc `http://localhost:5xxx`).

## Demo Database Sample

Bạn có thể đăng nhập ngay với các tài khoản trong DB có sẵn (password ở dữ liệu fake là string trơn `hashed_password_nam`, `hashed_password_linh` theo đúng file SQL).
- Username: `namnguyen` / Password: `hashed_password_nam`
- Username: `linhtran` / Password: `hashed_password_linh`
- Có thể dùng form để đăng ký luôn thành viên mới. Role mặc định là `Member`.

Toàn bộ các yêu cầu từ Comment, Upvote, Auth, Sorting trên DB có sẵn đã hoàn thành và sẵn sàng mở rộng cho Phase sau!
