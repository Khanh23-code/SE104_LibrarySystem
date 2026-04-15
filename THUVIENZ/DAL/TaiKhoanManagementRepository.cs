using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using THUVIENZ.Models;

namespace THUVIENZ.DAL
{
    /// <summary>
    /// Repository quản lý việc Duyệt/Từ chối tài khoản Độc giả.
    /// </summary>
    public class TaiKhoanManagementRepository
    {
        /// <summary>
        /// Lấy danh sách các tài khoản đang chờ duyệt (Pending).
        /// </summary>
        public List<TaiKhoan> GetPendingAccounts()
        {
            List<TaiKhoan> pending = new List<TaiKhoan>();
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "SELECT TenDangNhap, Quyen, TrangThai FROM TAIKHOAN WHERE TrangThai = 'Pending'";
                SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pending.Add(new TaiKhoan
                            {
                                TenDangNhap = reader["TenDangNhap"].ToString() ?? "",
                                Quyen = reader["Quyen"].ToString() ?? "",
                                TrangThai = reader["TrangThai"].ToString() ?? "Pending"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi GetPendingAccounts: " + ex.Message);
                }
            }
            return pending;
        }

        /// <summary>
        /// Cập nhật trạng thái cho tài khoản (Active, Rejected, v.v.).
        /// </summary>
        public bool UpdateAccountStatus(string username, string status)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                string query = "UPDATE TAIKHOAN SET TrangThai = @status WHERE TenDangNhap = @user";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@user", username);
                try
                {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        /// <summary>
        /// Xóa vĩnh viễn tài khoản và thông tin độc giả liên kết (Dùng khi Reject).
        /// </summary>
        public bool DeleteAccountAndReader(string username)
        {
            using (SqlConnection connection = DataProvider.Instance.GetConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Xóa thông tin độc giả trước (do có FK)
                        string queryDG = "DELETE FROM DOCGIA WHERE TenDangNhap = @user";
                        SqlCommand cmdDG = new SqlCommand(queryDG, connection, transaction);
                        cmdDG.Parameters.AddWithValue("@user", username);
                        cmdDG.ExecuteNonQuery();

                        // 2. Xóa tài khoản
                        string queryTK = "DELETE FROM TAIKHOAN WHERE TenDangNhap = @user";
                        SqlCommand cmdTK = new SqlCommand(queryTK, connection, transaction);
                        cmdTK.Parameters.AddWithValue("@user", username);
                        cmdTK.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Lỗi Reject account: " + ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}
