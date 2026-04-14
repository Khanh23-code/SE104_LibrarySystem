using System.Collections.Generic;
using THUVIENZ.DAL;
using THUVIENZ.Models;

namespace THUVIENZ.BLL
{
    /// <summary>
    /// Service xử lý nghiệp vụ duyệt và từ chối tài khoản độc giả mới.
    /// </summary>
    public class AccountApprovalService
    {
        private readonly TaiKhoanManagementRepository _repository;

        public AccountApprovalService()
        {
            _repository = new TaiKhoanManagementRepository();
        }

        /// <summary>
        /// Lấy toàn bộ danh sách tài khoản đang chờ phê duyệt.
        /// </summary>
        public List<TaiKhoan> GetPendingAccounts()
        {
            return _repository.GetPendingAccounts();
        }

        /// <summary>
        /// Phê duyệt tài khoản và chuyển sang trạng thái Active.
        /// </summary>
        public bool ApproveAccount(string username)
        {
            return _repository.UpdateAccountStatus(username, "Active");
        }

        /// <summary>
        /// Từ chối tài khoản và xóa bỏ thông tin liên quan trong hệ thống.
        /// </summary>
        public bool RejectAccount(string username)
        {
            // Theo yêu cầu: Xóa bỏ thông tin nếu bị từ chối
            return _repository.DeleteAccountAndReader(username);
        }
    }
}
