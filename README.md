# 📚 Librix - Library Management System

Librix là một hệ thống quản lý thư viện hiện đại được xây dựng dưới dạng ứng dụng Desktop. Dự án cung cấp giải pháp toàn diện để quản lý danh mục sách, theo dõi quá trình mượn/trả, xử lý vi phạm và cung cấp trải nghiệm tìm kiếm, đặt trước sách trực quan cho độc giả.

## ✨ Tính năng nổi bật 

### 👨‍💼 Dành cho Quản trị viên (Admin)
- **Quản lý Sách (Book Inventory):** Thêm, sửa, xóa và theo dõi số lượng tồn kho theo thời gian thực.
- **Quản lý Độc giả (Reader Management):** Phê duyệt tài khoản đăng ký mới, khóa/mở khóa tài khoản vi phạm.
- **Quản lý Mượn/Trả (Circulation):** - Giao diện Split-view tối ưu để duyệt/từ chối các yêu cầu đặt mượn sách.
  - Quét và ghi nhận trả sách, tự động tính toán tiền phạt nếu trễ hạn.
- **Thiết lập Quy định (Settings):** Tùy chỉnh các tham số hệ thống (hạn mức mượn, số ngày mượn tối đa, phí phạt trễ hạn).
- **Báo cáo & Thống kê (Dashboard):** Trực quan hóa dữ liệu mượn trả bằng biểu đồ.

### 👤 Dành cho Độc giả (Reader)
- **Khám phá sách (Discovery):** Tìm kiếm sách nhanh chóng và thêm vào danh sách yêu thích (Favorites).
- **Yêu cầu mượn sách (Borrow Request):** Tự động tạo phiếu yêu cầu giữ sách trước khi đến thư viện lấy.
- **Lịch sử & Thông báo (History & Notifications):** Theo dõi trạng thái thẻ mượn và nhận thông báo nhắc nhở hạn trả.

## 🛠 Công nghệ & Kiến trúc 
- **Framework:** .NET 8.0 (WPF - Windows Presentation Foundation)
- **Architecture Pattern:** MVVM (Model - View - ViewModel) kết hợp Service - Repository pattern.
- **Database:** SQL Server
- **Bảo mật:** BCrypt (Mã hóa mật khẩu an toàn)
- **UI/UX:** Thiết kế theo phong cách Material/Modern Desktop, hỗ trợ Data Binding và ControlTemplate.
- **Biểu đồ: ** LiveCharts.Wpf.NetCore3

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống 
- .NET 8.0 SDK
- SQL Server (bản Express hoặc Developer) cùng SQL Server Management Studio (SSMS)
- Trình soạn thảo: Visual Studio 2022 hoặc Visual Studio Code.

### 1. Khởi tạo Cơ sở dữ liệu
Vào thư mục `Database` của dự án và chạy lần lượt 4 file script SQL sau trong SQL Server (SSMS) để tạo bảng, thiết lập logic và nạp dữ liệu mẫu:
1. `QL_ThuVien_init.sql` (Khởi tạo cấu trúc bảng)
2. `03_Advanced_Logic.sql` (Khởi tạo các Trigger, Stored Procedures, Functions)
3. `04_Setup_Admin.sql` (Tạo tài khoản Quản trị viên mặc định)
4. `seed_dummy_data.sql` (Nạp dữ liệu mẫu để test)

### 2. Cấu hình & Chạy dự án
1. Clone repository về máy:
   ```bash
   git clone https://github.com/your-username/Librix.git
   ```

2. Mở file `THUVIENZ/App.config` và thay đổi chuỗi kết nối (Connection String) sao cho khớp với cấu hình SQL Server trên máy bạn. Ví dụ:
   ```xml
   <connectionStrings>
       <add name="DefaultConnection" connectionString="Server=YOUR_SERVER_NAME;Database=QL_ThuVien;Trusted_Connection=True;" providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

3. Mở terminal tại thư mục gốc và chạy lệnh khôi phục các gói NuGet (LiveCharts, BCrypt...):
   ```bash
   dotnet restore
   ```

4. Build và chạy dự án (Nhấn `F5` hoặc `Ctrl + Shift + B` trong Visual Studio).

## 📁 Cấu trúc thư mục (Folder Structure)
```text
THUVIENZ/
├── Database/       # Các script SQL khởi tạo hệ thống
├── Models/         # Các thực thể dữ liệu (Reader, Book, BorrowCard...)
├── Views/          # Giao diện XAML (AdminBorrowing, Login, MainWindow...)
│   └── Components/ # Các UI Controls tái sử dụng
├── ViewModels/     # Logic điều khiển, ICommand và kết nối View - Service
├── Services/       # Business logic (kiểm tra hạn mức, mã hóa mật khẩu...)
├── Repositories/   # Data Access Layer giao tiếp với SQL Server
└── App.config      # File cấu hình chứa Connection String
```
