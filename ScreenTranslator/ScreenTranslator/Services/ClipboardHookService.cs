using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LexiScan.Core;

namespace ScreenTranslator
{
    public class ClipboardHookService
    {
        public event Action<string>? OnTextCaptured;
        public event Action<string>? OnError;

        public const int MOD_NONE = 0x0000;
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;

        const int HOTKEY_ID = 9000;
        const int WM_HOTKEY = 0x0312;

        // [SỬA] Thêm import keybd_event để giả lập phím bấm tầng thấp (Win32 API)
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        // [SỬA] Định nghĩa mã phím cho Win32 API
        private const byte VK_CONTROL = 0x11;   // Mã phím Ctrl
        private const byte VK_C = 0x43;         // Mã phím C
        private const uint KEYEVENTF_KEYUP = 0x0002; // Cờ báo nhả phím

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _windowHandle;
        private bool _isRegistered = false;

        public void Register(IntPtr windowHandle, int modifier, int key)
        {
            _windowHandle = windowHandle;
            UpdateHotkey(modifier, key);
        }

        public void UpdateHotkey(int modifier, int key)
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }

            bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, key);

            if (success)
            {
                _isRegistered = true;
            }
            else
            {
                OnError?.Invoke("Lỗi: Không thể đăng ký Hotkey này (Có thể bị trùng)!");
            }
        }

        public void Unregister()
        {
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }
        }

        public void ProcessWindowMessage(int msg, int param)
        {
            if (msg == WM_HOTKEY && param == HOTKEY_ID)
            {
                PerformCopy();
            }
        }

        private void PerformCopy()
        {
            try
            {
                // Xóa clipboard để tránh lấy dữ liệu cũ
                try { Clipboard.Clear(); } catch { }

                // [SỬA] Thay thế SendKeys bằng keybd_event để chạy ổn định khi ẩn App
                // Cách cũ (hay lỗi): SendKeys.SendWait("^c");

                // Cách mới:
                // 1. Nhấn giữ Ctrl
                keybd_event(VK_CONTROL, 0, 0, 0);
                // 2. Nhấn giữ C
                keybd_event(VK_C, 0, 0, 0);

                // Đợi 50ms cho hệ điều hành nhận tín hiệu
                Thread.Sleep(50);

                // 3. Nhả C
                keybd_event(VK_C, 0, KEYEVENTF_KEYUP, 0);
                // 4. Nhả Ctrl
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

                // [SỬA] Tăng thời gian chờ lên 150ms để đảm bảo text đã vào Clipboard
                Thread.Sleep(150);

                string text = GetClipboardTextWithRetry();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    OnTextCaptured?.Invoke(text);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Clipboard rỗng hoặc bị khóa.");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Lỗi Copy: " + ex.Message);
            }
        }

        private string GetClipboardTextWithRetry()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (Clipboard.ContainsText())
                    {
                        return Clipboard.GetText();
                    }
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    Thread.Sleep(50);
                }
            }
            return string.Empty;
        }
    }
}