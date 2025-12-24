namespace LexiScan.Core
{
    // Đây là biến toàn cục để lưu ID người dùng đang đăng nhập
    public static class SessionManager
    {
        public static string CurrentUserId { get; set; }
    }
}