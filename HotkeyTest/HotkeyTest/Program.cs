using System;
using System.Runtime.InteropServices;

class Program
{
    // --- PHẦN 1: ĐỊNH NGHĨA CÁC HẰNG SỐ CƠ BẢN ---
    const int WM_HOTKEY = 0x0312;
    const int HOTKEY_ID = 1;

    // Các phím bổ trợ (Modifiers)
    const int MOD_NONE = 0x0000;
    const int MOD_ALT = 0x0001;
    const int MOD_CONTROL = 0x0002;
    const int MOD_SHIFT = 0x0004;
    const int MOD_WIN = 0x0008;

    // P/Invoke
    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int pt_x;
        public int pt_y;
    }

    static void Main(string[] args)
    {
        // --- PHẦN 2: GIẢ LẬP CÀI ĐẶT CỦA NGƯỜI DÙNG (SETTINGS) ---

        // Sau này, 2 dòng dưới đây sẽ được thay bằng code đọc từ file (ví dụ: Settings.Load())
        // Hiện tại, ta gán mặc định là Ctrl (0x0002) và Space (0x20)
        int userKeyModifier = MOD_CONTROL;
        int userKeyVCode = 0x20; // Mã Hex của phím 'S'

        
        Console.WriteLine($"Dang dang ky phim tat voi ma: Mod={userKeyModifier}, Key={userKeyVCode:X}");

        // --- PHẦN 3: ĐĂNG KÝ VỚI WINDOWS ---

        // Truyền biến số userKeyModifier và userKeyVCode vào thay vì số cứng
        if (RegisterHotKey(IntPtr.Zero, HOTKEY_ID, userKeyModifier, userKeyVCode))
        {
            Console.WriteLine("Dang ky THANH CONG!");
            Console.WriteLine("Hay thu nhan: Shift + S");
            Console.WriteLine("(Luu y: Khi dang chay, ban se KHONG go duoc chu S in hoa binh thuong)");
        }
        else
        {
            Console.WriteLine("Loi: Khong the dang ky. Co the phim tat nay dang bi trung.");
            return;
        }

        // --- PHẦN 4: LẮNG NGHE ---
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
        {
            if (msg.message == WM_HOTKEY)
            {
                // Logic xử lý khi bấm phím sẽ nằm ở đây
                Console.WriteLine($"!!! DA BAT DUOC PHIM TAT !!! (Time: {DateTime.Now.ToString("HH:mm:ss")})");
            }
        }

        UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
    }
}