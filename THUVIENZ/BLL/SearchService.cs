using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using THUVIENZ.DAL;
using THUVIENZ.Models;
using THUVIENZ.Models.DTOs;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý logic tìm kiếm và gợi ý sách cho người dùng.
    /// </summary>
    public class SearchService
    {
        private readonly LmsDbContext _context;

        public SearchService() : this(new LmsDbContext())
        {
        }

        public SearchService(LmsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thực hiện tìm kiếm sách đầy đủ (Kết quả chính).
        /// </summary>
        public async Task<IEnumerable<Sach>> SearchAsync(string? keyword)
        {
            string cleanKeyword = keyword?.Trim() ?? string.Empty;
            
            return await _context.Sachs
                .AsNoTracking()
                .Include(s => s.TheLoaiSach)
                .Where(s => s.TenSach.Contains(cleanKeyword) || (s.TacGia != null && s.TacGia.Contains(cleanKeyword)))
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách gợi ý nhanh khi người dùng đang nhập từ khóa (Search Suggestions).
        /// Giới hạn kết quả Top 10 và tối ưu hiệu năng bằng AsNoTracking.
        /// </summary>
        public async Task<IEnumerable<SuggestionDto>> GetSuggestionsAsync(string? query)
        {
            string cleanQuery = query?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(cleanQuery)) return Enumerable.Empty<SuggestionDto>();

            return await _context.Sachs
                .AsNoTracking()
                .Where(s => s.TenSach.Contains(cleanQuery) || (s.TacGia != null && s.TacGia.Contains(cleanQuery)))
                .Take(10) // Giới hạn Top 10 theo yêu cầu của Lead
                .Select(s => new SuggestionDto
                {
                    MaSach = s.MaSach,
                    DisplayText = s.TenSach,
                    SecondaryText = s.TacGia ?? string.Empty,
                    // Nếu từ khóa khớp với tên sách thì để loại là Title, ngược lại là Author
                    Loai = s.TenSach.Contains(cleanQuery) ? "Title" : "Author"
                })
                .ToListAsync();
        }
    }
}
