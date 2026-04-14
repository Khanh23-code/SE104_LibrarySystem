using System;
using System.Collections.Generic;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service quản lý các hoạt động CRUD cho sách và áp dụng các quy tắc nghiệp vụ.
    /// </summary>
    public class BookManagementService
    {
        private readonly SachRepository _sachRepository;

        public BookManagementService()
        {
            _sachRepository = new SachRepository();
        }

        /// <summary>
        /// Lấy tất cả đầu sách trong hệ thống.
        /// </summary>
        public List<Sach> GetAllBooks()
        {
            return _sachRepository.GetAllBooks();
        }

        /// <summary>
        /// Thêm sách mới sau khi kiểm tra tính hợp lệ.
        /// </summary>
        public bool AddBook(Sach book)
        {
            if (string.IsNullOrWhiteSpace(book.TenSach))
            {
                throw new ArgumentException("Tên sách không được để trống.");
            }

            return _sachRepository.AddBook(book);
        }

        /// <summary>
        /// Cập nhật thông tin sách hiện có.
        /// </summary>
        public bool UpdateBook(Sach book)
        {
            if (book.MaSach <= 0)
            {
                throw new ArgumentException("Mã sách không hợp lệ để cập nhật.");
            }

            return _sachRepository.UpdateBook(book);
        }

        /// <summary>
        /// Xóa sách nếu sách không đang trong trạng thái được mượn.
        /// </summary>
        public bool DeleteBook(int maSach)
        {
            // Quy tắc nghiệp vụ: Không thể xóa sách đang được mượn
            if (_sachRepository.IsBookCurrentlyBorrowed(maSach))
            {
                throw new InvalidOperationException("Quy tắc nghiệp vụ: Sách đang ở trạng thái 'Đang mượn', không thể xóa khỏi hệ thống.");
            }

            return _sachRepository.DeleteBook(maSach);
        }
    }
}
