using System;
using System.Threading.Tasks;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScan.Core.Enums;

namespace LexiScan.Core
{
    public class AppCoordinator
    {
        private readonly TranslationService _translationService;
        private readonly VoicetoText _voiceToTextService;
        private readonly TtsService _ttsService;

        public event Action<TranslationResult>? SearchResultReady;
        
        // Event cho P3 điền chữ từ Micro vào ô Search (Chú đã có)
        public event Action<string>? VoiceSearchCompleted;
        public event Action? VoiceRecognitionStarted;
        public event Action? VoiceRecognitionEnded;

        public AppCoordinator(TranslationService translationService, VoicetoText voiceToTextService, TtsService ttsService)
        {
            _translationService = translationService;
            _voiceToTextService = voiceToTextService;
            _ttsService = ttsService;

            _voiceToTextService.SpeechStarted += () => VoiceRecognitionStarted?.Invoke();
            _voiceToTextService.SpeechEnded += () => VoiceRecognitionEnded?.Invoke();

            _voiceToTextService.TextRecognized += (text) =>
            {
                VoiceSearchCompleted?.Invoke(text);
            };
        }

        public void Speak(string text, double speed, string accent)
        {
            // speed: thường từ 0.5 đến 2.0
            // accent: "US" hoặc "UK"
            _ttsService.Speak(text, speed, accent); //
        }
        public async Task<List<string>> GetRecommendWordsAsync(string prefix)
        {
            return await _translationService.GetGoogleSuggestionsAsync(prefix);
        }

        public async Task ExecuteSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                var result = await _translationService.ProcessTranslationAsync(text.Trim());

                SearchResultReady?.Invoke(result);

                // [SỬA] THÊM DÒNG NÀY ĐỂ BÁO CHO DICTIONARY VIEW HIỆN KẾT QUẢ
                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                var errorResult = new TranslationResult
                {
                    OriginalText = text,
                    Status = ServiceStatus.ApiError,
                    ErrorMessage = ex.Message
                };
                SearchResultReady?.Invoke(errorResult);
                // Có thể gọi TranslationCompleted ở đây nếu muốn hiện lỗi lên màn hình chính
                TranslationCompleted?.Invoke(errorResult);
            }
        }

        public event Action<TranslationResult>? TranslationCompleted;

        public async Task HandleClipboardTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;

            try
            {
                var result = await _translationService
                    .ProcessTranslationAsync(rawText.Trim());

                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                TranslationCompleted?.Invoke(new TranslationResult
                {
                    OriginalText = rawText,
                    Status = ServiceStatus.InternalError,
                    ErrorMessage = ex.Message
                });
            }
        }
        public void StartVoiceSearch()
        {
            _voiceToTextService.StartListening();
        }
    }
}
