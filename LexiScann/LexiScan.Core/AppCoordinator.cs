using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScanService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LexiScan.Core
{
    public class AppCoordinator
    {
        private readonly TranslationService _translationService;
        private readonly VoicetoText _voiceToTextService;
        private readonly TtsService _ttsService;

        // Sự kiện dành cho Dictionary (Giao diện chính)
        public event Action<TranslationResult>? SearchResultReady;
        public event Action<TranslationResult>? TranslationCompleted;

        // Sự kiện dành cho Popup (Dịch nhanh từ Clipboard/Hotkey)
        public event Action<TranslationResult>? OnPopupResultReceived;

        // Các sự kiện Voice (giữ nguyên)
        public event Action<string>? VoiceSearchCompleted;
        public event Action<string>? TranslationVoiceRecognized;
        public event Action? VoiceRecognitionStarted;
        public event Action? VoiceRecognitionEnded;
        public event Action<int>? AudioLevelUpdated;

        public AppCoordinator(TranslationService translationService, VoicetoText voiceToTextService, TtsService ttsService)
        {
            _translationService = translationService;
            _voiceToTextService = voiceToTextService;
            _ttsService = ttsService;

            _voiceToTextService.SpeechStarted += () => VoiceRecognitionStarted?.Invoke();
            _voiceToTextService.SpeechEnded += () => VoiceRecognitionEnded?.Invoke();

            _voiceToTextService.TextRecognized += (text) =>
            {
                string cleanedText = text.Trim().Replace(".", "").ToLower();
                if (!string.IsNullOrWhiteSpace(cleanedText))
                {
                    VoiceSearchCompleted?.Invoke(cleanedText);
                    TranslationVoiceRecognized?.Invoke(cleanedText);
                }
            };

            _voiceToTextService.AudioLevelUpdated += (level) =>
            {
                AudioLevelUpdated?.Invoke(level);
            };
        }

        public void Speak(string text, double speed, string accent)
        {
            _ttsService.Speak(text, speed, accent);
        }

        public async Task<List<string>> GetRecommendWordsAsync(string prefix)
        {
            return await _translationService.GetGoogleSuggestionsAsync(prefix);
        }

        // --- HÀM 1: CHỈ DÀNH CHO APP CHÍNH (Tra cứu thủ công) ---
        public async Task ExecuteSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(text.Trim());
                result.IsFromClipboard = false;

                // Kích hoạt các sự kiện dành riêng cho màn hình chính
                SearchResultReady?.Invoke(result);
                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                var errorResult = new TranslationResult
                {
                    OriginalText = text,
                    Status = ServiceStatus.ApiError,
                    ErrorMessage = ex.Message,
                    IsFromClipboard = false
                };
                SearchResultReady?.Invoke(errorResult);
                TranslationCompleted?.Invoke(errorResult);
            }
        }

        // --- HÀM 2: CHỈ DÀNH CHO POPUP (Dịch nhanh qua Clipboard/Hotkey) ---
        // Note: [QUAN TRỌNG] Hàm này KHÔNG gọi các sự kiện của Dictionary chính
        public async Task HandleClipboardTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;
            try
            {
                // 1. Thực hiện dịch
                var result = await _translationService.ProcessTranslationAsync(rawText.Trim());
                if (result == null) return;

                result.IsFromClipboard = true;

                // 2. CHỈ gửi dữ liệu cho sự kiện Popup hiển thị
                OnPopupResultReceived?.Invoke(result);

                // 3. TUYỆT ĐỐI không gọi SearchResultReady hay TranslationCompleted ở đây
                // để tránh làm thay đổi nội dung đang tra cứu trong màn hình Dictionary chính.
            }
            catch (Exception ex)
            {
                OnPopupResultReceived?.Invoke(new TranslationResult
                {
                    OriginalText = rawText,
                    Status = ServiceStatus.InternalError,
                    ErrorMessage = ex.Message,
                    IsFromClipboard = true
                });
            }
        }

        public void StartVoiceSearch()
        {
            _voiceToTextService.StartListening();
        }

        public async Task<TranslationResult> TranslateGeneralAsync(string text, string sl, string tl)
        {
            return await _translationService.TranslateForMainApp(text, sl, tl);
        }
    }
}