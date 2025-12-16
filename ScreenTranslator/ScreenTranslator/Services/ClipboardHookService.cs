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