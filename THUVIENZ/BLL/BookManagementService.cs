using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using THUVIENZ.BLL.Base;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service quản lý các hoạt động cho sách.
    /// Kế thừa BaseService và tuân thủ các quy tắc của Tech Lead.
    /// </summary>
    public class BookManagementService : BaseService<Sach>
    {
        private readonly SachRepository _sachRepository;

        public BookManagementService() : this(new SachRepository(new LmsDbContext()))
        {
        }

        public BookManagementService(SachRepository repository) : base(repository)
        {
            _sachRepository = repository;
        }

        /// <summary>
        /// Thêm sách mới kèm theo kiểm tra tính hợp lệ.
        /// </summary>
        public async Task AddBookAsync(Sach book)
        {
            if (string.IsNullOrWhiteSpace(book.TenSach))
                throw new ArgumentException("Tên sách không được để trống.");

            await AddAsync(book);
        }

        /// <summary>
        /// Xóa sách nếu sách không đang trong trạng thái được mượn.
        /// </summary>
        public async Task DeleteBookAsync(int maSach)
        {
            // Quy tắc nghiệp vụ: Không thể xóa sách đang được mượn
            if (_sachRepository.IsBookCurrentlyBorrowed(maSach))
            {
                throw new InvalidOperationException("Quy tắc nghiệp vụ: Sách đang ở trạng thái 'Đang mượn', không thể xóa khỏi hệ thống.");
            }

            await DeleteAsync(maSach);
        }
    }
}
