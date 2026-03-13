# MakerSpot - Phase 2

Toàn bộ code cho Phase 2 đã được cập nhật thành công, mở rộng từ Phase 1.

## Luồng chức năng mới bổ sung:
1. **Tìm kiếm (Search):**
   - Đã gõ form search vào Header.
   - Hoạt động ngay trên trang chủ, tìm theo `ProductName` hoặc `Tagline` của các sản phẩm đã duyệt (`Approved`).
2. **Lọc theo chủ đề (Topic Filter):**
   - Trang chủ có thêm Right Sidebar hiển thị tất cả các Topic.
   - Nhấn vào Topic sẽ lọc danh sách sản phẩm theo Chủ đề tương ứng.
3. **Đăng sản phẩm mới (Submit Product):**
   - Nút `Submit` trên Header (yêu cầu Login).
   - Form chuẩn với validation: Tên, Tagline, DemoUrl, Slug (tự động tạo từ tên sản phẩm), v.v.
   - Chọn nhiều topics.
   - Sản phẩm nộp vào sẽ mặc định Status `Pending` và tự gán role `Founder` cho người đăng vào bảng Maker.
4. **Trang cá nhân (User Profile):**
   - Truy cập qua tab dropdown ở Header `Hồ sơ của tôi`.
   - Hiển thị thông tin cá nhân cơ bản và lượt đếm.
   - **2 Tabs:** 
     - *Sản phẩm đã nộp*: (Hiển thị tất cả kể cả Pending nếu đang xem chính profile của mình).
     - *Sản phẩm đã Upvote*: (Chỉ hiện các sản phẩm upvote của người đó).

## Hướng dẫn chạy

1. Do bạn đã thay chuỗi kết nối ở `appsettings.json`, đảm bảo CSDL `MakerSpot` đang available trong SQL Express.
2. Terminal chạy `dotnet run` (dự án đã build thành công).
3. Đăng nhập hệ thống (bằng acc tự tạo ở Phase 1 hoặc dùng acc mồi).
4. Khám phá Search/Topic filter, tạo thử Product mới và xem User Profile.

**Lưu ý:**
- Bất kỳ sản phẩm nào bạn Submit từ giao diện sẽ có tem `Pending Approval` ở Profile và CHƯA BÀY RA NGOÀI TRANG CHỦ (do trang chủ chỉ lấy `Approved`).
- Chúng ta sẽ tiếp tục cho hệ Admin Approval ở các phase sau!
