using System.Windows.Controls;
using THUVIENZ.Core;
using THUVIENZ.ViewModels;
using Microsoft.Win32;
using System;

namespace THUVIENZ.Views
{
    public partial class Profile : UserControl
    {
        private readonly ProfileViewModel _viewModel;
        private int _avatarClickCount = 0;
        private readonly System.Timers.Timer _clickTimer;

        public Profile()
        {
            InitializeComponent();
            
            // Khá»Ÿi táº¡o ViewModel vÃ  thiáº¿t láº­p DataContext cho Binding
            _viewModel = new ProfileViewModel();
            this.DataContext = _viewModel;

            // Timer để phát hiện triple-click (3 lần trong 700ms)
            _clickTimer = new System.Timers.Timer(700);
            _clickTimer.AutoReset = false;
            _clickTimer.Elapsed += (s, e) => {
                _avatarClickCount = 0;
            };

            // Tải dữ liệu hồ sơ của độc giả đang đăng nhập
            if (!string.IsNullOrEmpty(UserSession.UserID))
            {
                _ = _viewModel.LoadProfileDataAsync(UserSession.UserID);
            }
        }

        private void AvatarEllipse_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Tăng bộ đếm click
            _avatarClickCount++;

            // Khởi động/tái khởi động timer
            _clickTimer.Stop();
            _clickTimer.Start();

            if (_avatarClickCount == 3)
            {
                // Reset về avatar mặc định
                if (_viewModel.CurrentReader != null)
                {
                    _viewModel.CurrentReader.AnhDaiDien = null;
                }

                _clickTimer.Stop();
                _avatarClickCount = 0;
                return;
            }

            // Nếu chỉ 1 lần click, mở dialog chọn ảnh
            if (_avatarClickCount == 1)
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
                dlg.Title = "Chọn ảnh đại diện";

                bool? result = dlg.ShowDialog();
                if (result == true && !string.IsNullOrEmpty(dlg.FileName))
                {
                    if (_viewModel.CurrentReader != null)
                    {
                        // Lưu đường dẫn tạm thời vào model. Tùy app có thể cần copy file vào thư mục dự án.
                        _viewModel.CurrentReader.AnhDaiDien = dlg.FileName;
                    }
                }
            }
        }
    }
}
