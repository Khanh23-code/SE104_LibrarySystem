namespace THUVIENZ.Core
{
    /// <summary>
    /// Lưu trữ thông tin phiên đăng nhập hiện tại.
    /// </summary>
    public static class UserSession
    {
        public static string? Username { get; set; }
        public static string? Role { get; set; }
    }
}
