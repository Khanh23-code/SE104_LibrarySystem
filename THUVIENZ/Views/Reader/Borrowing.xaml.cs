using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows.Controls;
using THUVIENZ.Core;
using THUVIENZ.DAL;
using THUVIENZ.ViewModels;

namespace THUVIENZ.Views
{
    public partial class Borrowing : UserControl
    {
        private readonly BorrowingViewModel _viewModel;

        public Borrowing()
        {
            InitializeComponent();

            _viewModel = new BorrowingViewModel();
            this.DataContext = _viewModel;

            // Khi View được tải, tra cứu MaDocGia từ UserSession rồi nạp danh sách mượn
            this.Loaded += async (_, _) => await LoadReaderIdAsync();
        }

        /// <summary>
        /// Tra cứu MaDocGia tương ứng với TenDangNhap đang đăng nhập (UserSession.UserID)
        /// và gán vào ViewModel để kích hoạt LoadMyBorrowedBooks().
        /// </summary>
        private async Task LoadReaderIdAsync()
        {
            if (string.IsNullOrEmpty(UserSession.UserID)) return;

            using var ctx = new LmsDbContext();
            var docGia = await ctx.DocGias
                .FirstOrDefaultAsync(d => d.TenDangNhap == UserSession.UserID);

            if (docGia != null)
                _viewModel.ReaderId = docGia.MaDocGia;
        }
    }
}
