using System;

namespace LexiScan.Core.Services
{
    public interface IHookService
    {
        event Action<string>? OnTextCaptured;
        event Action<string>? OnError;

        void Register(IntPtr windowHandle);
        void Unregister();
        void UpdateHotkey(string hotkeyString);
        void ProcessWindowMessage(int msg, IntPtr param);
    }
}