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

        // --- HÀM 1: Dùng cho App Chính (DictionaryViewModel gọi) ---
        public async Task ExecuteSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(text.Trim());

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
                    IsFromClipboard = false 
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