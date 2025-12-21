using LexiScan.App.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LexiScan.Core;
using LexiScan.Core.Models;

namespace LexiScan.App.ViewModels
{
    public class TranslationViewModel : BaseViewModel
    {
        private readonly AppCoordinator _coordinator;
        private string _sourceText = "";
        private string _translatedText = "Bản dịch";
        private int _currentCharCount = 0;
        private string _sourceLang = "en"; // Trái
        private string _targetLang = "vi"; // Phải
        private string _sourceLangName = "Anh";
        private string _targetLangName = "Việt";
        private CancellationTokenSource _translationCts;

        public ICommand SpeakSourceCommand { get; }
        public ICommand SpeakTargetCommand { get; }
        public ICommand SwapLanguageCommand { get; }
        public ICommand SaveTranslationCommand { get; }
        public ICommand StartVoiceCommand { get; }

        // Logic: Hiện đầy đủ [Mic, Loa, Lưu] ở bên trái nếu là tiếng Anh
        public Visibility IsSourceEnglishVisible => _sourceLang == "en" ? Visibility.Visible : Visibility.Collapsed;

        // Logic: Hiện [Loa, Lưu] ở bên phải nếu là tiếng Anh (Mất Mic)
        public Visibility IsTargetEnglishVisible => _targetLang == "en" ? Visibility.Visible : Visibility.Collapsed;

        public TranslationViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            SwapLanguageCommand = new RelayCommand(obj => ExecuteSwap());
            SpeakSourceCommand = new RelayCommand(obj => _coordinator.Speak(SourceText, 1.0, _sourceLang));
            SpeakTargetCommand = new RelayCommand(obj => _coordinator.Speak(TranslatedText, 1.0, _targetLang));
            /*
            SaveTranslationCommand = new RelayCommand(obj => {
                // Tự động nhận diện cặp từ Anh-Việt để lưu đúng vị trí
                string eng = (_sourceLang == "en") ? SourceText : TranslatedText;
                string vie = (_sourceLang == "en") ? TranslatedText : SourceText;
                if (!string.IsNullOrWhiteSpace(eng) && vie != "Bản dịch")
                    _coordinator.SaveToPersonalDictionary(eng, vie);
            });*/

            StartVoiceCommand = new RelayCommand(obj => _coordinator.StartVoiceSearch());
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
                    if (result != null && !token.IsCancellationRequested) TranslatedText = result.TranslatedText;
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        private void ExecuteSwap()
        {
            var tempL = _sourceLang; _sourceLang = _targetLang; _targetLang = tempL;
            var tempN = SourceLangName; SourceLangName = TargetLangName; TargetLangName = tempN;
            var oldS = SourceText;
            SourceText = (TranslatedText == "Bản dịch") ? "" : TranslatedText;
            TranslatedText = string.IsNullOrWhiteSpace(oldS) ? "Bản dịch" : oldS;

            // Cập nhật lại UI để các nút nhảy theo tiếng Anh
            OnPropertyChanged(nameof(IsSourceEnglishVisible));
            OnPropertyChanged(nameof(IsTargetEnglishVisible));

            TriggerAutoTranslate();
        }
    }
}