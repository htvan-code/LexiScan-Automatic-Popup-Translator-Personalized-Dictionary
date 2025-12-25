using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services; // Cần thêm cái này để dùng SettingsService
using LexiScanData.Services;
using LexiScanData.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LexiScan.App.ViewModels
{
    public class TranslationViewModel : BaseViewModel
    {
        private DatabaseServices? _dbService;
        private readonly AppCoordinator _coordinator;
        private readonly SettingsService _settingsService; // [THÊM]

        private string _sourceText = "";
        private string _translatedText = "Bản dịch";
        private int _currentCharCount = 0;
        private string _sourceLang = "en";
        private string _targetLang = "vi";
        private string _sourceLangName = "Anh";
        private string _targetLangName = "Việt";
        private CancellationTokenSource _translationCts;
        private string _lastSavedText = "";

        // ... Các Command (SpeakSourceCommand, SpeakTargetCommand...) giữ nguyên ...
        public ICommand SpeakSourceCommand { get; }
        public ICommand SpeakTargetCommand { get; }
        public ICommand SwapLanguageCommand { get; }
        public ICommand SaveTranslationCommand { get; }
        public ICommand StartVoiceCommand { get; }

        public Visibility IsSourceEnglishVisible => _sourceLang == "en" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsTargetEnglishVisible => _targetLang == "en" ? Visibility.Visible : Visibility.Collapsed;

        public TranslationViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            _settingsService = new SettingsService(); // [THÊM] Khởi tạo SettingsService

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid))
            {
                _dbService = new DatabaseServices(uid);
            }

            SwapLanguageCommand = new RelayCommand(obj => ExecuteSwap());
            SpeakSourceCommand = new RelayCommand(obj => _coordinator.Speak(SourceText, 1.0, _sourceLang));
            SpeakTargetCommand = new RelayCommand(obj => _coordinator.Speak(TranslatedText, 1.0, _targetLang));

            SaveTranslationCommand = new RelayCommand(async (obj) =>
            {
                await ExecuteSaveHistory(forceSave: true); // Nút lưu thủ công luôn lưu
            });

            StartVoiceCommand = new RelayCommand(obj =>
            {
                _coordinator.StartVoiceSearch(VoiceSource.Translation);
            });

            _coordinator.VoiceSearchCompleted += (text) =>
            {
                if (_coordinator.CurrentVoiceSource == VoiceSource.Translation)
                {
                    SourceText = text;
                }
            };

            _coordinator.TranslationVoiceRecognized += (text) =>
            {
                if (_coordinator.CurrentVoiceSource == VoiceSource.Translation)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Cộng dồn chữ vào khung nhập liệu
                        if (string.IsNullOrWhiteSpace(SourceText))
                            SourceText = text;
                        else
                            SourceText += " " + text;
                    });
                }
            };

            _coordinator.VoiceRecognitionStarted += () => OnPropertyChanged("IsListening");
            _coordinator.VoiceRecognitionEnded += () => OnPropertyChanged("IsListening");
        }

        // ... Các Property (SourceLangName, TargetLangName...) giữ nguyên ...
        public string SourceLangName { get => _sourceLangName; set { _sourceLangName = value; OnPropertyChanged(); } }
        public string TargetLangName { get => _targetLangName; set { _targetLangName = value; OnPropertyChanged(); } }
        public string SourceText { get => _sourceText; set { if (_sourceText != value) { _sourceText = value; OnPropertyChanged(); CurrentCharCount = _sourceText?.Length ?? 0; TriggerAutoTranslate(); } } }
        public string TranslatedText { get => _translatedText; set { _translatedText = value; OnPropertyChanged(); } }
        public int CurrentCharCount { get => _currentCharCount; set { _currentCharCount = value; OnPropertyChanged(); } }

        private void TriggerAutoTranslate()
        {
            _translationCts?.Cancel();
            _translationCts = new CancellationTokenSource();
            var token = _translationCts.Token;
            Task.Run(async () => {
                try
                {
                    await Task.Delay(500, token);
                    if (string.IsNullOrWhiteSpace(SourceText)) { TranslatedText = "Bản dịch"; return; }
                    var result = await _coordinator.TranslateGeneralAsync(SourceText, _sourceLang, _targetLang);
                    if (result != null && !token.IsCancellationRequested)
                    {
                        TranslatedText = result.TranslatedText;

                        // [LOGIC MỚI]: Sau khi dịch xong, kiểm tra settings để tự động lưu
                        var settings = _settingsService.LoadSettings();
                        if (settings.AutoSaveHistoryToDictionary)
                        {
                            await ExecuteSaveHistory(forceSave: false);
                        }
                    }
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        private async Task ExecuteSaveHistory(bool forceSave = false)
        {
            // Kiểm tra: Text rỗng thì bỏ qua
            if (string.IsNullOrWhiteSpace(SourceText)) return;

            // Nếu không phải lưu thủ công (forceSave) và text giống lần trước thì bỏ qua (tránh lưu lặp)
            if (!forceSave && SourceText == _lastSavedText) return;

            // Tự kết nối lại nếu bị null
            if (_dbService == null && !string.IsNullOrEmpty(SessionManager.CurrentUserId))
            {
                _dbService = new DatabaseServices(SessionManager.CurrentUserId);
            }

            if (_dbService != null)
            {
                try
                {
                    await _dbService.AddHistoryAsync(new Sentences
                    {
                        SourceText = SourceText,
                        TranslatedText = TranslatedText,
                        CreatedDate = DateTime.Now
                    });

                    // Cập nhật text đã lưu
                    _lastSavedText = SourceText;
                }
                catch { }
            }
        }

        private void ExecuteSwap()
        {
            var tempL = _sourceLang; _sourceLang = _targetLang; _targetLang = tempL;
            var tempN = SourceLangName; SourceLangName = TargetLangName; TargetLangName = tempN;
            var oldS = SourceText;
            SourceText = (TranslatedText == "Bản dịch") ? "" : TranslatedText;
            TranslatedText = string.IsNullOrWhiteSpace(oldS) ? "Bản dịch" : oldS;

            OnPropertyChanged(nameof(IsSourceEnglishVisible));
            OnPropertyChanged(nameof(IsTargetEnglishVisible));

            TriggerAutoTranslate();
        }
    }
}