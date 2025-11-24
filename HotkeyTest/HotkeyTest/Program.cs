using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms; // Cần thiết để dùng Clipboard và SendKeys

class Program
{
    // --- PHẦN 1: KHAI BÁO HOTKEY (GIỐNG TUẦN 1) ---
    const int MOD_CONTROL = 0x0002;
    const int WM_HOTKEY = 0x0312;
    const int HOTKEY_ID = 1;

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

    // --- PHẦN 2: MAIN ---

    // Bắt buộc phải có dòng này để truy cập Clipboard
    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine("--- TEXT SELECTION TOOL ---");
        Console.WriteLine("B1: Boi den mot doan text bat ky (tren Chrome, Word...).");
        Console.WriteLine("B2: Nhan to hop phim: Ctrl + Space");

        // Đăng ký Ctrl + Space (0x20)
        if (RegisterHotKey(IntPtr.Zero, HOTKEY_ID, MOD_CONTROL, 0x20))
        {
            Console.WriteLine("Da san sang! Hay thu boi den text va bam Hotkey.");
        }
        else
        {
            Console.WriteLine("Loi dang ky Hotkey!");
            return;
        }

        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
        {
            if (msg.message == WM_HOTKEY)
            {
                // Khi bấm Hotkey, ta gọi hàm xử lý
                HandleSelection();
            }
        }

        UnregisterHotKey(IntPtr.Zero, HOTKEY_ID);
    }

    // --- PHẦN 3: LOGIC LẤY TEXT (CORE) ---
    static void HandleSelection()
    {
        // 1. Xóa Clipboard cũ để tránh lấy nhầm dữ liệu cũ
        // (Bỏ qua bước này nếu muốn giữ lịch sử, nhưng nên làm để sạch sẽ)
        try { Clipboard.Clear(); } catch { }

        // 2. Giả lập nhấn Ctrl + C
        // SendWait: Gửi phím và chờ xử lý xong mới chạy tiếp
        // "^c": Trong SendKeys, dấu ^ đại diện cho Ctrl, c là phím C
        SendKeys.SendWait("^c");

        // 3. Chờ một chút xíu để hệ điều hành kịp chép vào Clipboard
        // (Windows cần vài mili-giây để xử lý việc copy)
        Thread.Sleep(150);

        // 4. Lấy dữ liệu từ Clipboard ra
        if (Clipboard.ContainsText())
        {
            string selectedText = Clipboard.GetText();

            // In ra kết quả
            Console.WriteLine("\n---------------------------------");
            Console.WriteLine("NOI DUNG BAN VUA CHON LA:");
            Console.WriteLine("---------------------------------");
            Console.WriteLine(selectedText);
            Console.WriteLine("---------------------------------");

            // SAU NÀY: Thay vì Console.WriteLine, bạn sẽ gửi text này vào Google Translate API
        }
        else
        {
            Console.WriteLine("\n[!] Khong lay duoc text (Co the ban chua boi den?)");
        }
    }
}