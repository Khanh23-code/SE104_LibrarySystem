using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using THUVIENZ.BLL;
using THUVIENZ.Core;
using THUVIENZ.Models;

namespace THUVIENZ.ViewModels
{
    /// <summary>
    /// ViewModel cho màn hình Duyệt tài khoản Độc giả (Admin).
    /// Quản lý danh sách các tài khoản đang chờ phê duyệt.
    /// </summary>
    public class AccountApprovalViewModel : ObservableObject
    {
        private ObservableCollection<TaiKhoan> _pendingAccounts = new ObservableCollection<TaiKhoan>();
        public ObservableCollection<TaiKhoan> PendingAccounts
        {
            get => _pendingAccounts;
            set
            {
                _pendingAccounts = value;
                OnPropertyChanged();
            }
        }

        private TaiKhoan? _selectedAccount;
        public TaiKhoan? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadPendingCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }

        private readonly AccountApprovalService _approvalService;

        public AccountApprovalViewModel()
        {
            _approvalService = new AccountApprovalService();
            
            // Khởi tạo các Commands
            LoadPendingCommand = new RelayCommand(_ => LoadData());
            ApproveCommand = new RelayCommand(_ => ExecuteApprove());
            RejectCommand = new RelayCommand(_ => ExecuteReject());

            // Tải dữ liệu ban đầu
            LoadData();
        }

        /// <summary>
        /// Nạp danh sách các tài khoản đang ở trạng thái Pending.
        /// </summary>
        private void LoadData()
        {
            try
            {
                var accounts = _approvalService.GetPendingAccounts();
                PendingAccounts = new ObservableCollection<TaiKhoan>(accounts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Duyệt tài khoản đang chọn.
        /// </summary>
        private void ExecuteApprove()
        {
            if (SelectedAccount == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản từ danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_approvalService.ApproveAccount(SelectedAccount.TenDangNhap))
            {
                MessageBox.Show($"Tài khoản '{SelectedAccount.TenDangNhap}' đã được kích hoạt thành công.", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Làm mới danh sách
            }
            else
            {
                MessageBox.Show("Không thể duyệt tài khoản này. Vui lòng thử lại sau.", "Lỗi thực thi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Từ chối và xóa thông tin tài khoản đang chọn.
        /// </summary>
        private void ExecuteReject()
        {
            if (SelectedAccount == null)
            {
                MessageBox.Show("Vui lòng chọn một tài khoản từ danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn TỪ CHỐI tài khoản '{SelectedAccount.TenDangNhap}'? Toàn bộ thông tin độc giả liên quan sẽ bị xóa khỏi hệ thống.", 
                                        "Xác nhận từ chối", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (confirm == MessageBoxResult.No) return;

            if (_approvalService.RejectAccount(SelectedAccount.TenDangNhap))
            {
                MessageBox.Show("Đã từ chối và xóa bỏ thông tin tài khoản thành công.", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Làm mới danh sách
            }
            else
            {
                MessageBox.Show("Lỗi trong quá trình xóa dữ liệu.", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
