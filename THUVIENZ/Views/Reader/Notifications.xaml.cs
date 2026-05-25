using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FontAwesome.Sharp;

namespace THUVIENZ.Views
{
    public partial class Notifications : UserControl
    {
        // Sự kiện bắn tín hiệu lên MainWindow để clear dấu chấm đỏ ở sidebar
        public event Action? OnNotificationsViewed;

        public Notifications()
        {
            InitializeComponent();
        }

        // Kích hoạt ngay khi màn hình thông báo vừa hiển thị
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNotificationsFromDbAsync();
            OnNotificationsViewed?.Invoke();
        }

        // Tải thông báo từ cơ sở dữ liệu động
        private async Task LoadNotificationsFromDbAsync()
        {
            var username = Core.UserSession.UserID;
            if (string.IsNullOrEmpty(username)) return;

            try
            {
                NotificationContainer.Children.Clear();

                using var context = new DAL.LmsDbContext();
                // Lấy danh sách thông báo của user
                var list = await context.ThongBaos
                    .Where(t => t.TenDangNhap == username)
                    .OrderByDescending(t => t.NgayThongBao)
                    .ToListAsync();

                if (list.Count == 0)
                {
                    var emptyText = new TextBlock
                    {
                        Text = "Bạn không có thông báo nào.",
                        FontSize = 16,
                        Foreground = (Brush)new BrushConverter().ConvertFrom("#6B7280"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 50, 0, 0)
                    };
                    NotificationContainer.Children.Add(emptyText);
                }
                else
                {
                    foreach (var noti in list)
                    {
                        var type = Models.NotificationType.Info;
                        if (Enum.TryParse<Models.NotificationType>(noti.LoaiThongBao, true, out var parsedType))
                        {
                            type = parsedType;
                        }

                        var card = new Components.NotificationCard
                        {
                            NotiType = type,
                            Title = noti.TieuDe,
                            Message = noti.NoiDung,
                            Timestamp = FormatTimestamp(noti.NgayThongBao),
                            Tag = noti.MaThongBao // Lưu lại mã thông báo
                        };

                        card.OnCloseRequested += NotificationCard_OnCloseRequested;
                        card.OnCardClicked += NotificationCard_OnCardClicked;

                        NotificationContainer.Children.Add(card);
                    }
                }

                // Cập nhật toàn bộ thông báo chưa đọc thành đã đọc
                var unread = list.Where(t => !t.DaDoc).ToList();
                if (unread.Any())
                {
                    foreach (var u in unread)
                    {
                        u.DaDoc = true;
                    }
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách thông báo: {ex.Message}", "Lỗi tải dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Định dạng mốc thời gian thân thiện với người dùng
        private string FormatTimestamp(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalSeconds < 60) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            return $"{dt:dd/MM/yyyy}";
        }

        // Hàm xử lý nút X: Xóa Card thông báo ra khỏi StackPanel & CSDL
        private async void NotificationCard_OnCloseRequested(object sender, RoutedEventArgs e)
        {
            if (sender is Components.NotificationCard card && card.Tag is int notiId)
            {
                NotificationContainer.Children.Remove(card);

                try
                {
                    using var context = new DAL.LmsDbContext();
                    var noti = await context.ThongBaos.FindAsync(notiId);
                    if (noti != null)
                    {
                        context.ThongBaos.Remove(noti);
                        await context.SaveChangesAsync();
                    }
                }
                catch
                {
                    // Lỗi ngầm định không crash
                }
            }
            else if (sender is UIElement cardEl)
            {
                NotificationContainer.Children.Remove(cardEl);
            }
        }

        // Hàm xử lý khi Click vào Card: Nạp dữ liệu và mở Popup chi tiết
        private void NotificationCard_OnCardClicked(object sender, RoutedEventArgs e)
        {
            if (sender is THUVIENZ.Views.Components.NotificationCard card)
            {
                // Đổ text từ Card sang Popup
                DetailPopup.PopupTitle = card.Title;
                DetailPopup.PopupMessage = card.Message;
                DetailPopup.PopupTimestamp = card.Timestamp;

                // Đồng bộ hóa màu sắc và Icon của Popup khớp với loại thông báo trên Card
                switch (card.NotiType)
                {
                    case Models.NotificationType.Success:
                        DetailPopup.PopupIcon = IconChar.CheckCircle;
                        DetailPopup.IconColor = (Brush)new BrushConverter().ConvertFrom("#137333");
                        DetailPopup.IconBgColor = (Brush)new BrushConverter().ConvertFrom("#E6F4EA");
                        break;
                    case Models.NotificationType.Failure:
                        DetailPopup.PopupIcon = IconChar.TimesCircle;
                        DetailPopup.IconColor = (Brush)new BrushConverter().ConvertFrom("#C5221F");
                        DetailPopup.IconBgColor = (Brush)new BrushConverter().ConvertFrom("#FCE8E6");
                        break;
                    case Models.NotificationType.Warning:
                        DetailPopup.PopupIcon = IconChar.ExclamationTriangle;
                        DetailPopup.IconColor = (Brush)new BrushConverter().ConvertFrom("#B06000");
                        DetailPopup.IconBgColor = (Brush)new BrushConverter().ConvertFrom("#FEF7E0");
                        break;
                    case Models.NotificationType.Info:
                    default:
                        DetailPopup.PopupIcon = IconChar.InfoCircle;
                        DetailPopup.IconColor = (Brush)new BrushConverter().ConvertFrom("#1A73E8");
                        DetailPopup.IconBgColor = (Brush)new BrushConverter().ConvertFrom("#E8F0FE");
                        break;
                }

                // Hiển thị lớp Popup mặt nạ lên màn hình
                DetailPopup.Visibility = Visibility.Visible;
            }
        }
    }
}