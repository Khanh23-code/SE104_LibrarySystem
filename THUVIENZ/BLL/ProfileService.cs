using System.Collections.Generic;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý logic liên quan đến hồ sơ Độc giả.
    /// </summary>
    public class ProfileService
    {
        private readonly DocGiaRepository _docGiaRepository;

        public ProfileService()
        {
            _docGiaRepository = new DocGiaRepository();
        }

        /// <summary>
        /// Lấy thông tin cá nhân của Độc giả.
        /// </summary>
        public DocGia? GetReaderInfo(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return _docGiaRepository.GetReaderProfile(username);
        }

        /// <summary>
        /// Lấy danh sách sách đang mượn của Độc giả.
        /// </summary>
        public List<Sach> GetActiveBorrowedBooks(int maDocGia)
        {
            if (maDocGia <= 0) return new List<Sach>();
            return _docGiaRepository.GetBorrowedBooks(maDocGia);
        }
    }
}
