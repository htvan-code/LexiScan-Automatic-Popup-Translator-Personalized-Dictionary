using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ScreenTranslator
{
    public class ClipboardHookService
    {
        public event Action<string>? OnTextCaptured;
        public event Action<string>? OnError;

        // --- CÁC HẰNG SỐ MODIFIER ---
        // Person 3 sẽ gửi số này vào cho bạn
        public const int MOD_NONE = 0x0000;
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;

        const int HOTKEY_ID = 9000;
        const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Lưu giữ Handle cửa sổ để dùng lại khi đổi Hotkey
        private IntPtr _windowHandle;
        private bool _isRegistered = false;

        // --- HÀM 1: ĐĂNG KÝ LẦN ĐẦU ---
        public void Register(IntPtr windowHandle, int modifier, int key)
        {
            _windowHandle = windowHandle;
            UpdateHotkey(modifier, key);
        }

        // --- HÀM 2: ĐỔI HOTKEY (QUAN TRỌNG NHẤT) ---
        public void UpdateHotkey(int modifier, int key)
        {
            // 1. Nếu đang có hotkey cũ thì hủy đi
            if (_isRegistered)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _isRegistered = false;
            }

            // 2. Đăng ký hotkey mới
            bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, key);

            if (success)
            {
                _isRegistered = true;
                // (Tùy chọn) Báo ra ngoài là đổi thành công
                // OnError?.Invoke($"Đã đổi Hotkey sang: Mod={modifier}, Key={key}");
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
                Clipboard.Clear();
                SendKeys.SendWait("^c");
                Thread.Sleep(200);

                if (Clipboard.ContainsText())
                {
                    OnTextCaptured?.Invoke(Clipboard.GetText());
                }
                else
                {
                    OnError?.Invoke("Clipboard rỗng (Không copy được).");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke("Lỗi: " + ex.Message);
            }
        }
    }
}