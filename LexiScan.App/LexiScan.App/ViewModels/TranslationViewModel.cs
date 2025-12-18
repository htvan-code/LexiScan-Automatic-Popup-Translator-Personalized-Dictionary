using LexiScan.App.ViewModels;
// ... các using khác

namespace LexiScan.App.ViewModels
{
    public class TranslationViewModel : BaseViewModel // Giả sử bạn kế thừa BaseViewModel
    {
        private string _sourceText = "";
        private int _currentCharCount = 0;

        // Property đếm số ký tự
        public int CurrentCharCount
        {
            get => _currentCharCount;
            set
            {
                _currentCharCount = value;
                OnPropertyChanged(); // Thông báo giao diện cập nhật số đếm
            }
        }

        // Property chứa nội dung cần dịch
        public string SourceText
        {
            get => _sourceText;
            set
            {
                // Logic giới hạn cứng (phòng trường hợp paste văn bản dài hơn 1000 ký tự)
                if (value.Length > 1000)
                {
                    value = value.Substring(0, 1000);
                }

                if (_sourceText != value)
                {
                    _sourceText = value;
                    OnPropertyChanged();

                    // Cập nhật số lượng ký tự ngay khi văn bản thay đổi
                    CurrentCharCount = _sourceText.Length;
                }
            }
        }

        // ... các code khác của bạn (Command dịch, v.v.)
    }
}