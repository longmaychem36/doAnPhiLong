# MakerSpot - Phase 3 Admin Portal

Toàn bộ code cho Phase 3 đã được cập nhật thành công, tập trung vào khu vực Quản trị dành cho Admin để duyệt sản phẩm.

## Luồng chức năng Admin:
1. **Bảo mật (Authorization):**
   - Chỉ account có Role `Admin` mới truy cập được khu vực `/Admin`.
   - Các account thường (`Member`) sẽ không thấy link và bị cấm truy cập.
2. **UI & Layout riêng:**
   - Tách biệt Layout Admin hoàn toàn so với trang người dùng. 
   - Có Sidebar điều hướng: Dashboard, Sản phẩm chờ duyệt, Tất cả sản phẩm.
3. **Dashboard:**
   - Hiển thị thống kê tổng quan (Tổng SP, SP Chờ duyệt, Tổng User, Tổng Comment).
   - Danh sách nhanh 5 sản phẩm Pending mới nhất.
4. **Quản lý & Trạng thái Sản phẩm:**
   - **Tất cả sản phẩm**: Hiển thị mọi sản phẩm kèm badge trạng thái tương ứng (Approved, Pending, Rejected, Hidden).
   - **Chờ duyệt**: Lọc riêng các sản phẩm Pending.
   - Nút **Hành động**: Admin có thể bấm *Đổi trạng thái* ngay tại grid hoặc bấm vào *Xem Chi tiết*. Trang chi tiết được thiết kế riêng mượt mà với đầy đủ thông tin để admin dễ dàng kiểm duyệt. Nút thay đổi trạng thái cập nhật trực tiếp DB và hiển thị lại bằng thông báo xanh lá nhỏ (Toast Alert).

## Hướng dẫn chạy & Demo Phase 3
1. **Thiết lập Account Admin:** 
   Bạn cần có 1 tài khoản với Role `Admin` trong database. Nếu chưa có, bạn đi vào DB SQL Server thưc thi lệnh sau để gán quyền Admin cho account của bạn (Giả sử bạn có Username là `namnguyen` và ở bảng Roles đã có Role Admin ID = 1):
   ```sql
   INSERT INTO UserRoles (UserId, RoleId)
   VALUES ((SELECT UserId FROM Users WHERE Username = 'namnguyen'), (SELECT RoleId FROM Roles WHERE RoleName = 'Admin'))
   ```
2. **Chạy ứng dụng:**
   Trong Terminal, gõ `dotnet run`.
3. **Trải nghiệm:**
   - Đăng nhập bằng tài khoản Admin ở trên. 
   - Bạn sẽ thấy nút "Trang Admin" xuất hiện trong menu dropdown góc phải trên cùng.
   - Vào đó để duyệt những sản phẩm bạn vừa test "Submit Product" ở Phase 2 (chúng đang ở trạng thái Pending). Khi bạn bấm Approve, ra trang Home sẽ thấy nó hiện hữu!
