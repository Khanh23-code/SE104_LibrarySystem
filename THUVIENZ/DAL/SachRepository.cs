using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repository xử lý dữ liệu liên quan đến đầu sách (CRUD).
    /// Đã được tái cấu trúc từ ADO.NET sang Entity Framework Core.
    /// </summary>
    public class SachRepository
    {
        private readonly LmsDbContext _context;

        public SachRepository()
        {
            _context = new LmsDbContext();
        }

        /// <summary>
        /// Lấy toàn bộ danh sách sách từ CSDL bằng LINQ.
        /// </summary>
        public List<Sach> GetAllBooks()
        {
            try
            {
                return _context.Sachs.OrderByDescending(s => s.MaSach).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF GetAllBooks: " + ex.Message);
                return new List<Sach>();
            }
        }

        /// <summary>
        /// Tìm kiếm sách theo từ khóa (Tên hoặc Mã).
        /// Sử dụng Raw SQL thông qua EF Core để tối ưu các truy vấn LIKE phức tạp.
        /// </summary>
        public List<Sach> SearchBooks(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return _context.Sachs.OrderByDescending(s => s.MaSach).Take(50).ToList();
                }

                // Thực thi SQL có tham số an toàn thông qua EF Core
                string sql = "SELECT * FROM SACH WHERE TenSach LIKE @kw OR CAST(MaSach AS VARCHAR) LIKE @kw";
                var kwParam = new Microsoft.Data.SqlClient.SqlParameter("@kw", "%" + keyword + "%");

                return _context.Sachs.FromSqlRaw(sql, kwParam).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF SearchBooks: " + ex.Message);
                return new List<Sach>();
            }
        }

        /// <summary>
        /// Thêm sách mới vào CSDL.
        /// </summary>
        public bool AddBook(Sach newBook)
        {
            try
            {
                // Thiết lập trạng thái mặc định nếu trống
                if (string.IsNullOrWhiteSpace(newBook.TinhTrang))
                    newBook.TinhTrang = "Sẵn sàng";

                _context.Sachs.Add(newBook);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF AddBook: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Cập nhật thông tin sách hiện có.
        /// </summary>
        public bool UpdateBook(Sach updatedBook)
        {
            try
            {
                // Kiểm tra xem thực thể có đang được track không, nếu không thì attach và update
                var existing = _context.Sachs.Local.FirstOrDefault(s => s.MaSach == updatedBook.MaSach);
                if (existing != null)
                {
                    _context.Entry(existing).State = EntityState.Detached;
                }

                _context.Sachs.Update(updatedBook);
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF UpdateBook: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Xóa sách khỏi hệ thống.
        /// </summary>
        public bool DeleteBook(int maSach)
        {
            try
            {
                var book = _context.Sachs.Find(maSach);
                if (book != null)
                {
                    _context.Sachs.Remove(book);
                    return _context.SaveChanges() > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF DeleteBook: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem sách có đang được mượn không (Dựa trên bảng Chi Tiết Phiếu Mượn).
        /// </summary>
        public bool IsBookCurrentlyBorrowed(int maSach)
        {
            try
            {
                return _context.ChiTietPhieuMuons.Any(ct => ct.MaSach == maSach && ct.TrangThai == "Đang mượn");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi EF IsBookCurrentlyBorrowed: " + ex.Message);
                return false;
            }
        }
    }
}
