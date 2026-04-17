using System.Text.RegularExpressions;

namespace THUVIENZ.Core
{
    public static class InputValidator
    {
        /// <summary>
        /// Kiểm tra Tên: Chỉ chứa chữ cái (hỗ trợ tiếng Việt có dấu) và khoảng trắng. 
        /// Không có số hay ký tự đặc biệt.
        /// </summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            // \p{L} đại diện cho mọi ký tự chữ cái trong Unicode (bao gồm Tiếng Việt)
            // \s đại diện cho khoảng trắng
            return Regex.IsMatch(name, @"^[\p{L}\s]+$");
        }

        /// <summary>
        /// Kiểm tra ID/Username: Không được chứa khoảng trắng.
        /// </summary>
        public static bool IsValidId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return !id.Contains(" ");
        }

        /// <summary>
        /// Kiểm tra Mật khẩu: Ít nhất 6 ký tự và không chứa khoảng trắng.
        /// </summary>
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return password.Length >= 6 && !password.Contains(" ");
        }

        /// <summary>
        /// Kiểm tra hai mật khẩu có trùng khớp không.
        /// </summary>
        public static bool IsPasswordMatch(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }
    }
}