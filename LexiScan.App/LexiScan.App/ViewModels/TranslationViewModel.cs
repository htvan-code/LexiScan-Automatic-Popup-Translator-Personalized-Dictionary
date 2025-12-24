using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Models;
using LexiScanData.Services;
using LexiScanData.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LexiScan.Core.Enums;

namespace LexiScan.App.ViewModels
{
    public class TranslationViewModel : BaseViewModel
    {
        private readonly DatabaseServices? _dbService;
        private readonly AppCoordinator _coordinator;
        private string _sourceText = "";
        private string _translatedText = "Bản dịch";
        private int _currentCharCount = 0;
        private string _sourceLang = "en";
        private string _targetLang = "vi";
        private string _sourceLangName = "Anh";
        private string _targetLangName = "Việt";
        private CancellationTokenSource _translationCts;

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

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid))
            {
                _dbService = new DatabaseServices(uid);
            }

            SwapLanguageCommand = new RelayCommand(obj => ExecuteSwap());
            SpeakSourceCommand = new RelayCommand(obj => _coordinator.Speak(SourceText, 1.0, _sourceLang));
            SpeakTargetCommand = new RelayCommand(obj => _coordinator.Speak(TranslatedText, 1.0, _targetLang));

            // CẬP NHẬT: Gửi kèm nguồn là Translation
            StartVoiceCommand = new RelayCommand(obj =>
            {
                _coordinator.StartVoiceSearch(VoiceSource.Translation);
            });

            // CẬP NHẬT: Nhận kết quả Voice cho trang dịch
            _coordinator.VoiceSearchCompleted += (text) =>
            {
                if (_coordinator.CurrentVoiceSource == VoiceSource.Translation)
                {
                    SourceText = text;
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
                        // --- BẮT ĐẦU ĐOẠN CODE BẮT LỖI (COPY ĐÈ VÀO ĐÂY) ---

                        // 1. Kiểm tra xem đã có ID chưa?
                        string currentId = SessionManager.CurrentUserId;
                        if (string.IsNullOrEmpty(currentId))
                        {
                            MessageBox.Show("LỖI TO: SessionManager.CurrentUserId đang bị RỖNG!\nBạn chưa đăng nhập hoặc code App.xaml.cs chưa chạy.", "Báo động đỏ");
                        }
                        else
                        {
                            // 2. Nếu có ID rồi, thử lưu và xem có lỗi gì không
                            if (_dbService != null)
                            {
                                Application.Current.Dispatcher.Invoke(async () =>
                                {
                                    try
                                    {
                                        // Hiện thông báo trước khi lưu (để biết code có chạy vào đây ko)
                                        // MessageBox.Show($"Bắt đầu lưu cho User: {currentId}", "Thông báo");

                                        await _dbService.AddHistoryAsync(new Sentences
                                        {
                                            SourceText = SourceText,
                                            TranslatedText = result.TranslatedText,
                                            CreatedDate = DateTime.Now
                                        });

                                        // Hiện thông báo nếu lưu thành công
                                        MessageBox.Show("Đã gửi lệnh lưu lên Firebase THÀNH CÔNG!", "Chúc mừng");
                                    }
                                    catch (Exception ex)
                                    {
                                        // Hiện lỗi cụ thể nếu mạng lag hoặc sai Rule
                                        MessageBox.Show($"Lưu thất bại. Lỗi: {ex.Message}", "Lỗi rồi");
                                    }
                                });
                            }
                            else
                            {
                                MessageBox.Show("Lỗi: _dbService chưa được khởi tạo (null)", "Lỗi Code");
                            }
                        }
                        // --- KẾT THÚC ĐOẠN CODE BẮT LỖI ---
                        // [THÊM 2] LỆNH LƯU VÀO LỊCH SỬ
                        if (_dbService != null)
                        {
                            _ = _dbService.AddHistoryAsync(new Sentences
                            {
                                SourceText = SourceText,
                                TranslatedText = result.TranslatedText,
                                CreatedDate = DateTime.Now
                            });
                        }
                    }
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

            OnPropertyChanged(nameof(IsSourceEnglishVisible));
            OnPropertyChanged(nameof(IsTargetEnglishVisible));

            TriggerAutoTranslate();
        }
    }
}