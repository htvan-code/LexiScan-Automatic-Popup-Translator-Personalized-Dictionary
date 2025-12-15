using CommunityToolkit.Mvvm.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace LexiScanUI
{
    // Class này dùng để hiển thị từng từ lên Popup cho người dùng bấm chọn
    public partial class SelectableWord : ObservableObject
    {
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private bool _isSelected;

        public SelectableWord(string text)
        {
            // Các thuộc tính Text và IsSelected sẽ được tự động sinh ra
            // nhờ vào [ObservableProperty] ở trên.
            Text = text;
            IsSelected = false;
        }
    }
}