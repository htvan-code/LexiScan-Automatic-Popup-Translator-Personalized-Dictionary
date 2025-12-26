using System;

namespace LexiScan.Core.Utils
{
    public static class GlobalEvents
    {
        public static event Action? OnRequestOpenSettings;
        public static void RaiseRequestOpenSettings() => OnRequestOpenSettings?.Invoke();

        public static event Action? OnPersonalDictionaryUpdated;
        public static void RaisePersonalDictionaryUpdated() => OnPersonalDictionaryUpdated?.Invoke();
    }
} 