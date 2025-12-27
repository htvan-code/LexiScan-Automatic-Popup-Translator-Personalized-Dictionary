using System;

namespace LexiScan.Core.Services
{
    // Đây là bản hợp đồng
    public interface IHookService
    {
        event Action<string>? OnTextCaptured;
        event Action<string>? OnError;

        void Register(IntPtr windowHandle);
        void Unregister();
        void UpdateHotkey(string hotkeyString);
        // [SỬA] Đổi int param -> IntPtr param
        void ProcessWindowMessage(int msg, IntPtr param);
    }
}