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

        public AppCoordinator(TranslationService translationService, VoicetoText voiceToTextService, TtsService ttsService)
        {
            _translationService = translationService;
            _voiceToTextService = voiceToTextService;
            _ttsService = ttsService;

            _voiceToTextService.TextRecognized += (text) =>
            {
                VoiceSearchCompleted?.Invoke(text);
                TranslationVoiceRecognized?.Invoke(text);
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

        public async Task ExecuteSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(text.Trim());
                SearchResultReady?.Invoke(result);
                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                var errorResult = new TranslationResult { OriginalText = text, Status = ServiceStatus.ApiError, ErrorMessage = ex.Message };
                SearchResultReady?.Invoke(errorResult);
                TranslationCompleted?.Invoke(errorResult);
            }
        }

        public async Task HandleClipboardTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(rawText.Trim());
                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                TranslationCompleted?.Invoke(new TranslationResult { OriginalText = rawText, Status = ServiceStatus.InternalError, ErrorMessage = ex.Message });
            }
        }

        public void StartVoiceSearch()
        {
            _voiceToTextService.StartListening();
        }
    }
}