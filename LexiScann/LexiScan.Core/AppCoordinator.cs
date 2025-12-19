using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
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

        public event Action<TranslationResult>? SearchResultReady;
        public event Action<string>? VoiceSearchCompleted;
        public event Action<TranslationResult>? TranslationCompleted;

        public AppCoordinator(TranslationService translationService, VoicetoText voiceToTextService, TtsService ttsService)
        {
            _translationService = translationService;
            _voiceToTextService = voiceToTextService;
            _ttsService = ttsService;

            _voiceToTextService.TextRecognized += (text) =>
            {
                VoiceSearchCompleted?.Invoke(text);
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

        // --- HÀM 1: Dùng cho App Chính (DictionaryViewModel gọi) ---
        public async Task ExecuteSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(text.Trim());

                // [GẮN NHÃN] Đây là search từ App -> KHÔNG hiện Popup
                result.IsFromClipboard = false;

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
                    IsFromClipboard = false // Lỗi trong app thì báo trong app
                };
                SearchResultReady?.Invoke(errorResult);
                TranslationCompleted?.Invoke(errorResult);
            }
        }

        // --- HÀM 2: Dùng cho Hotkey (MainWindow/Hook gọi) ---
        public async Task HandleClipboardTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(rawText.Trim());

                // [GẮN NHÃN] Đây là search từ Hotkey -> CẦN hiện Popup
                result.IsFromClipboard = true;

                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                TranslationCompleted?.Invoke(new TranslationResult
                {
                    OriginalText = rawText,
                    Status = ServiceStatus.InternalError,
                    ErrorMessage = ex.Message,
                    IsFromClipboard = true // Lỗi từ hotkey thì hiện popup báo lỗi
                });
            }
        }

        public void StartVoiceSearch()
        {
            _voiceToTextService.StartListening();
        }
    }
}