using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScanService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LexiScan.Core
{
    // [THÊM] Enum để phân biệt nguồn gọi Voice
    public enum VoiceSource { Dictionary, Translation }

    public class AppCoordinator
    {
        private readonly TranslationService _translationService;
        private readonly VoicetoText _voiceToTextService;
        private readonly TtsService _ttsService;

        // [THÊM] Thuộc tính để biết ai đang dùng Mic
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

            _voiceToTextService.TextRecognized += (text) =>
            {
                string cleanedText = text.Trim().Replace(".", "").ToLower();
                if (!string.IsNullOrWhiteSpace(cleanedText))
                {
                    // [QUAN TRỌNG] Gửi tín hiệu Voice dựa trên nguồn đã lưu
                    if (CurrentVoiceSource == VoiceSource.Dictionary)
                    {
                        VoiceSearchCompleted?.Invoke(cleanedText);
                    }
                    else if (CurrentVoiceSource == VoiceSource.Translation)
                    {
                        TranslationVoiceRecognized?.Invoke(cleanedText);
                    }
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

        // [CẬP NHẬT] Hàm bắt đầu nghe phải nhận vào Nguồn gọi
        public void StartVoiceSearch(VoiceSource source)
        {
            CurrentVoiceSource = source; // Lưu lại ai đang gọi
            _voiceToTextService.StartListening();
        }

        public async Task<TranslationResult> TranslateGeneralAsync(string text, string sl, string tl)
        {
            return await _translationService.TranslateForMainApp(text, sl, tl);
        }
        // --- [MỚI] HÀM NÀY ĐỂ PHỤC VỤ HISTORY VIEW MODEL ---
        // Lấy kết quả dịch đầy đủ và trả về trực tiếp (thay vì bắn Event)
        public async Task<TranslationResult?> TranslateAndGetResultAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                // Tái sử dụng logic dịch có sẵn
                return await _translationService.ProcessTranslationAsync(text.Trim());
            }
            catch
            {
                return null;
            }
        }
        // [BỔ SUNG] Hàm lưu vào từ điển cá nhân (Đã hứa ở các bước trước)
        /*
        public void SaveToPersonalDictionary(string word, string meaning, string phonetic = "")
        {
            // Logic lưu dữ liệu của bạn sẽ gọi vào DataService ở đây
            System.Diagnostics.Debug.WriteLine($"Đang lưu: {word} - {meaning}");
        }*/
    }
}