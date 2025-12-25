using System;

namespace LexiScan.Core.Utils
{
    public static class GlobalEvents
    {
        // Sự kiện mở Settings (đã có)
        public static event Action? OnRequestOpenSettings;
        public static void RaiseRequestOpenSettings() => OnRequestOpenSettings?.Invoke();

        // [THÊM] Sự kiện báo hiệu Từ điển cá nhân đã thay đổi
        public static event Action? OnPersonalDictionaryUpdated;
        public static void RaisePersonalDictionaryUpdated() => OnPersonalDictionaryUpdated?.Invoke();
    }
}