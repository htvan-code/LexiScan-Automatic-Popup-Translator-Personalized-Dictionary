using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
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
        private readonly SettingsService _settingsService; 
        
        private string _sourceText = "";
        private string _translatedText = "Bản dịch";
        private int _currentCharCount = 0;
        private string _sourceLang = "en";
        private string _targetLang = "vi";
        private string _sourceLangName = "Anh";
        private string _targetLangName = "Việt";
        private CancellationTokenSource _translationCts;
        private string _lastSavedText = "";

        private bool _isListening;
        public bool IsListening { get => _isListening; set { _isListening = value; OnPropertyChanged(); } }

        private double _voiceLevel;
        public double VoiceLevel { get => _voiceLevel; set { _voiceLevel = value; OnPropertyChanged(); } }
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
            _settingsService = new SettingsService(); //Khởi tạo SettingsService

            string uid = SessionManager.CurrentUserId;
            string token = SessionManager.CurrentAuthToken;
            if (!string.IsNullOrEmpty(uid) || !string.IsNullOrEmpty(token))
            {
                _dbService = new DatabaseServices(uid, SessionManager.CurrentAuthToken);
            }

            SwapLanguageCommand = new RelayCommand(obj => ExecuteSwap());
            SpeakSourceCommand = new RelayCommand(obj => ExecuteSpeak(SourceText, _sourceLang));
            SpeakTargetCommand = new RelayCommand(obj => ExecuteSpeak(TranslatedText, _targetLang));

            SaveTranslationCommand = new RelayCommand(async (obj) =>
            {
                await ExecuteSaveHistory(forceSave: true); 
            });

            StartVoiceCommand = new RelayCommand(obj =>
            {
                if (!IsListening)
                {
                    _coordinator.StartVoiceSearch(VoiceSource.Translation);
                }
                else
                {
                    _coordinator.StopVoiceSearch();
                }
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
                        if (string.IsNullOrWhiteSpace(SourceText))
                            SourceText = text;
                        else
                            SourceText += " " + text;
                    });
                }
            };

            _coordinator.VoiceRecognitionStarted += () =>
            {
                if (_coordinator.CurrentVoiceSource == VoiceSource.Translation)
                {
                    Application.Current.Dispatcher.Invoke(() => IsListening = true);
                }
            };

            _coordinator.VoiceRecognitionEnded += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    IsListening = false;
                    VoiceLevel = 0;
                });
            };

            _coordinator.AudioLevelUpdated += (level) =>
            {
                if (_coordinator.CurrentVoiceSource == VoiceSource.Translation)
                {
                    double newSize = 25.0 + (level * 1.8);
                    if (newSize > 45) newSize = 45;
                    if (newSize < 25) newSize = 25;
                    VoiceLevel = newSize;
                }
            };
        }

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
                        /*
                        var settings = _settingsService.LoadSettings();
                        if (settings.AutoSaveHistoryToDictionary)
                        {
                            await ExecuteSaveHistory(forceSave: false);
                        }
                        */
                    }
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        private async Task ExecuteSaveHistory(bool forceSave = false)
        {
            if (string.IsNullOrWhiteSpace(SourceText)) return;

            if (!forceSave && SourceText == _lastSavedText) return;

            if (_dbService == null && (!string.IsNullOrEmpty(SessionManager.CurrentUserId) || !string.IsNullOrEmpty(SessionManager.CurrentAuthToken)))
            {
                _dbService = new DatabaseServices(SessionManager.CurrentUserId, SessionManager.CurrentAuthToken);
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

        private void ExecuteSpeak(string text, string langCode)
        {
            if (string.IsNullOrWhiteSpace(text) || text == "Bản dịch") return;

            var settings = _settingsService.LoadSettings();

            double speedRate = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speedRate = -5; break;
                case SpeechSpeed.Slow: speedRate = -3; break;
                case SpeechSpeed.Normal: speedRate = 0; break;
            }

            string finalLang = langCode;
            if (langCode == "en")
            {
                finalLang = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";
            }

            _coordinator.Speak(text, speedRate, finalLang);
        }
    }
}