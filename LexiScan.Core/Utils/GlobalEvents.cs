using System;

namespace LexiScan.Core.Utils
{
    public static class GlobalEvents
    {
        // Sự kiện mở Cài đặt (đã có)
        public static event Action? OnRequestOpenSettings;
        public static void RaiseRequestOpenSettings() => OnRequestOpenSettings?.Invoke();

        // Sự kiện cập nhật từ điển cá nhân (đã có)
        public static event Action? OnPersonalDictionaryUpdated;
        public static void RaisePersonalDictionaryUpdated() => OnPersonalDictionaryUpdated?.Invoke();

        // [MỚI] Sự kiện thông báo Hotkey đã thay đổi
        // Cần thêm cái này để SettingsViewModel báo cho AppCoordinator biết
        public static event Action? OnHotkeyChanged;
        public static void RaiseHotkeyChanged() => OnHotkeyChanged?.Invoke();
    }
}