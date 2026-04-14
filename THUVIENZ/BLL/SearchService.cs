using System.Collections.Generic;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý logic tìm kiếm và lọc sách.
    /// </summary>
    public class SearchService
    {
        private readonly SachRepository _sachRepository;

        public SearchService()
        {
            _sachRepository = new SachRepository();
        }

        /// <summary>
        /// Thực hiện tìm kiếm sách.
        /// </summary>
        public List<Sach> Search(string? keyword)
        {
            // Trình xử lý: Xóa khoảng trắng thừa
            string cleanKeyword = keyword?.Trim() ?? string.Empty;

            // Chuyển tiếp yêu cầu xuống DAL
            return _sachRepository.SearchBooks(cleanKeyword);
        }
    }
}
