using LexiScan.App.Commands; // Đường dẫn đến file RelayCommand của chú
using System;
using System.Threading;
using System.Threading.Tasks;
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
        private string _sourceLang = "en";
        private string _targetLang = "vi";
        private CancellationTokenSource _translationCts;

        private string _sourceLangName = "Anh";
        private string _targetLangName = "Việt";

        public ICommand SpeakSourceCommand { get; }
        public ICommand SpeakTargetCommand { get; }

        public TranslationViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            SwapLanguageCommand = new RelayCommand(obj => ExecuteSwap());
            SpeakSourceCommand = new RelayCommand(obj => _coordinator.Speak(SourceText, 1.0, _sourceLang));
            SpeakTargetCommand = new RelayCommand(obj => _coordinator.Speak(TranslatedText, 1.0, _targetLang));
        }

        public string SourceLangName
        {
            get => _sourceLangName;
            set { _sourceLangName = value; OnPropertyChanged(); }
        }

        public string TargetLangName
        {
            get => _targetLangName;
            set { _targetLangName = value; OnPropertyChanged(); }
        }

        public string SourceText
        {
            get => _sourceText;
            set
            {
                if (_sourceText != value)
                {
                    _sourceText = value;
                    OnPropertyChanged();
                    CurrentCharCount = _sourceText?.Length ?? 0;
                    // Tự động dịch khi gõ
                    TriggerAutoTranslate();
                }
            }
        }

        public string TranslatedText
        {
            get => _translatedText;
            set { _translatedText = value; OnPropertyChanged(); }
        }

        public int CurrentCharCount
        {
            get => _currentCharCount;
            set { _currentCharCount = value; OnPropertyChanged(); }
        }

        public ICommand SwapLanguageCommand { get; }

        private void TriggerAutoTranslate()
        {
            _translationCts?.Cancel();
            _translationCts = new CancellationTokenSource();
            var token = _translationCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(500, token); // Đợi 500ms ngừng gõ mới dịch
                    if (string.IsNullOrWhiteSpace(SourceText))
                    {
                        TranslatedText = "Bản dịch";
                        return;
                    }

                    // Gọi qua Coordinator để lấy bản dịch
                    var result = await _coordinator.TranslateGeneralAsync(SourceText, _sourceLang, _targetLang);
                    if (result != null && !token.IsCancellationRequested)
                    {
                        TranslatedText = result.TranslatedText;
                    }
                }
                catch (OperationCanceledException) { }
            }, token);
        }

        private void ExecuteSwap()
        {
            // 1. Đảo ngôn ngữ
            var tempLang = _sourceLang;
            _sourceLang = _targetLang;
            _targetLang = tempLang;

            // 2. Đảo nội dung văn bản
            var tempText = SourceText;
            // Nếu bên phải đang là "Bản dịch" thì coi như rỗng
            SourceText = (TranslatedText == "Bản dịch") ? "" : TranslatedText;
            TranslatedText = string.IsNullOrWhiteSpace(tempText) ? "Bản dịch" : tempText;

            // [THÊM] Đảo tên hiển thị để giao diện thay đổi theo
            var tempName = SourceLangName;
            SourceLangName = TargetLangName;
            TargetLangName = tempName;

            // 3. Kích hoạt dịch lại ngay lập tức với cài đặt mới
            TriggerAutoTranslate();
        }
    }
}