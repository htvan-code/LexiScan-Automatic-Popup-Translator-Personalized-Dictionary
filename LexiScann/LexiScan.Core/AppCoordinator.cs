using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScanService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LexiScan.Core
{
    public enum VoiceSource { Dictionary, Translation }

    public class AppCoordinator
    {
        private readonly TranslationService _translationService;
        private readonly VoicetoText _voiceToTextService;
        private readonly TtsService _ttsService;
        private string _dictionaryFullText = "";
        // Thuộc tính để biết ai đang dùng Mic
        public VoiceSource CurrentVoiceSource { get; private set; }

        // Sự kiện dành cho Dictionary (Giao diện chính)
        public event Action<TranslationResult>? SearchResultReady;
        public event Action<TranslationResult>? TranslationCompleted;

        // Sự kiện dành cho Popup (Dịch nhanh từ Clipboard/Hotkey)
        public event Action<TranslationResult>? OnPopupResultReceived;

        // Các sự kiện Voice
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

            _voiceToTextService.SpeechEnded += () =>
            {
                if (CurrentVoiceSource == VoiceSource.Dictionary && !string.IsNullOrWhiteSpace(_dictionaryFullText))
                {
                    var finalSearch = _dictionaryFullText.Trim().Replace(".", "").ToLower();
                    VoiceSearchCompleted?.Invoke(finalSearch);
                    _dictionaryFullText = ""; // Xóa thùng chứa cho lần sau
                }
                VoiceRecognitionEnded?.Invoke();
            };

            _voiceToTextService.TextRecognized += (text) =>
            {
                string clean = System.Text.RegularExpressions.Regex.Replace(text, @"(?i)blankaudio|\[.*?\]|[^a-zA-Z0-9\s]", "").Trim();
                if (string.IsNullOrWhiteSpace(clean)) return;

                if (CurrentVoiceSource == VoiceSource.Dictionary)
                {
                    string currentFull = _dictionaryFullText.Trim();
                    if (!currentFull.EndsWith(clean, StringComparison.OrdinalIgnoreCase))
                    {
                        _dictionaryFullText = (currentFull + " " + clean).Trim();
                    }
                }
                else if (CurrentVoiceSource == VoiceSource.Translation)
                {
                    // Translation: Vẫn cho nhảy chữ rào rào theo yêu cầu của anh
                    TranslationVoiceRecognized?.Invoke(clean);
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

        // --- HÀM TRA CỨU TỪ ĐIỂN ---
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

        //--- HÀM DỊCH NHANH POPUP ---
        public async Task HandleClipboardTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;
            try
            {
                var result = await _translationService.ProcessTranslationAsync(rawText.Trim());
                if (result == null) return;

                result.IsFromClipboard = true;
                OnPopupResultReceived?.Invoke(result);
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

        public void StartVoiceSearch(VoiceSource source)
        {
            CurrentVoiceSource = source; 
            _voiceToTextService.StartListening();
        }

        public void StopVoiceSearch()
        {
            if (_voiceToTextService.IsRecording)
            {
                _voiceToTextService.StopListening();
            }
        }

        public async Task<TranslationResult> TranslateGeneralAsync(string text, string sl, string tl)
        {
            return await _translationService.TranslateForMainApp(text, sl, tl);
        }

        // Lấy kết quả dịch đầy đủ và trả về trực tiếp (thay vì bắn Event)
        public async Task<TranslationResult?> TranslateAndGetResultAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                return await _translationService.ProcessTranslationAsync(text.Trim());
            }
            catch
            {
                return null;
            }
        }

    }
}