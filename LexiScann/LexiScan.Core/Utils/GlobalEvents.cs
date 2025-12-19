using System;
namespace LexiScan.Core.Utils
{
    public static class GlobalEvents
    {
        public static event Action? OnRequestOpenSettings;

        public static void RaiseRequestOpenSettings()
        {
            OnRequestOpenSettings?.Invoke();
        }
    }
}