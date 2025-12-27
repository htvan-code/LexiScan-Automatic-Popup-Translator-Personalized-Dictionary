using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LexiScan.Core;
using LexiScan.Core.Services;

namespace LexiScan.Core.Services // Đảm bảo đúng namespace
{
    public class ClipboardHookService : IHookService
    {
        public event Action<string>? OnTextCaptured;
        public event Action<string>? OnError;

        // Mã phím Modifier
        public const int MOD_NONE = 0x0000;
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;

        const int HOTKEY_ID = 9000;
        const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        // Mã phím ảo để giả lập Copy
        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const byte VK_MENU = 0x12; // Phím ALT
        private const byte VK_SHIFT = 0x10; // Phím Shift
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr _windowHandle;
        private bool _isRegistered = false;

        public void Register(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public void UpdateHotkey(string hotkeyString)
        {
            // 1. Gỡ bỏ hotkey cũ
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }

            if (string.IsNullOrEmpty(hotkeyString)) return;

            int modifier = MOD_NONE;
            int key = 0;

            // Tách chuỗi linh hoạt (chấp nhận cả dấu + và dấu cách)
            // Ví dụ: "Ctrl + 1" -> ["Ctrl", "1"]
            var parts = hotkeyString.Split(new[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                string p = part.Trim();
                if (string.IsNullOrEmpty(p)) continue;

                switch (p)
                {
                    case "Ctrl": modifier |= MOD_CONTROL; break;
                    case "Alt": modifier |= MOD_ALT; break;
                    case "Shift": modifier |= MOD_SHIFT; break;
                    case "Win": modifier |= MOD_WIN; break;
                    default:
                        // [QUAN TRỌNG] Xử lý lỗi phím số (1 -> D1, 2 -> D2...)
                        // Nếu là số 0-9, thêm chữ 'D' đằng trước để khớp với Enum Keys.D1
                        if (int.TryParse(p, out int digit))
                        {
                            p = "D" + digit;
                        }

                        // Cố gắng ép kiểu sang Enum Keys
                        if (Enum.TryParse(p, true, out Keys k))
                        {
                            key = (int)k;
                        }
                        else if (p == "Esc") key = (int)Keys.Escape;
                        else if (p == "Del" || p == "Delete") key = (int)Keys.Delete;
                        else if (p == "Enter") key = (int)Keys.Enter;
                        else if (p == "Space") key = (int)Keys.Space;
                        else if (p == "Tab") key = (int)Keys.Tab;
                        else if (p == "Backspace") key = (int)Keys.Back;
                        break;
                }
            }

            if (key != 0)
            {
                bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, key);
                if (success)
                {
                    _isRegistered = true;
                }
                else
                {
                    OnError?.Invoke($"Không thể đăng ký phím tắt '{hotkeyString}'. Có thể bị trùng.");
                }
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

        public void ProcessWindowMessage(int msg, IntPtr param)
        {
            if (msg == WM_HOTKEY && param.ToInt64() == HOTKEY_ID)
            {
                PerformCopy();
            }
        }

        private void PerformCopy()
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    try { Clipboard.Clear(); } catch { }

                    keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, 0);   
                    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);  
                    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0); 

                    Thread.Sleep(50); 

                    keybd_event(VK_CONTROL, 0, 0, 0);
                    keybd_event(VK_C, 0, 0, 0);
                    Thread.Sleep(50);
                    keybd_event(VK_C, 0, KEYEVENTF_KEYUP, 0);
                    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);

                    Thread.Sleep(150); 

                    string text = GetClipboardTextWithRetry();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        OnTextCaptured?.Invoke(text);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke("Lỗi Copy: " + ex.Message);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private string GetClipboardTextWithRetry()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (Clipboard.ContainsText()) return Clipboard.GetText();
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