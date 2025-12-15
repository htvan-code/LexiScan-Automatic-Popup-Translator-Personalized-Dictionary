// File: Services/AppCoordinator.cs
using LexiScan.Core.Models;
using LexiScan.Core.Services;

public class AppCoordinator
{
    // Khai báo các service của cả nhóm
    private readonly SystemHookService _hookService; // P1
    private readonly TranslationService _translationService; // P2 (em)
    private readonly DatabaseService _databaseService; // P4
    private readonly PopupService _popupService; // P4

    public AppCoordinator(
        SystemHookService hookService,
        TranslationService translationService,
        DatabaseService databaseService,
        PopupService popupService)
    {
        _hookService = hookService;
        _translationService = translationService;
        _databaseService = databaseService;
        _popupService = popupService;

        // Bắt đầu lắng nghe P1 ngay khi Coordinator được tạo ra
        _hookService.TextCaptured += HandleTextCaptured;
    }

    // Logic Phân loại Đầu vào (Input Detective)
    private bool IsSingleWord(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        string trimmedText = text.Trim();
        // Kiểm tra đơn giản: Ít khoảng trắng (<= 3) và không quá dài (<= 40 ký tự)
        int spaceCount = trimmedText.Count(c => c == ' ');
        return spaceCount <= 3 && trimmedText.Length <= 40;
    }

    // Hàm Xử lý chính khi P1 bắt được Text và Hotkey
    private async void HandleTextCaptured(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return;

        string textToProcess = rawText.Trim();
        bool isWord = IsSingleWord(textToProcess);
        TranslationResult result = null;

        try
        {
            // 1. GỌI API DỊCH/TRA TỪ 
            if (isWord)
            {
                // Gọi API Từ điển (sẽ trả về chi tiết: Phiên âm, Từ loại, v.v.)
                result = await _translationService.GetDefinitionAsync(textToProcess);
            }
            else
            {
                // Gọi API Dịch thuật (chủ yếu trả về Bản dịch)
                result = await _translationService.TranslateSentenceAsync(textToProcess);
            }

            // Đảm bảo kết quả có đầy đủ thông tin
            if (result == null) return;

            // 2. LƯU VÀO DATABASE (GỌI P4)
            await _databaseService.SaveResultAsync(result);

            // 3. YÊU CẦU POPUP HIỂN THỊ (GỌI P4)
            _popupService.ShowPopup(result);
        }
        catch (Exception ex)
        {
            // Xử lý lỗi
            _popupService.ShowErrorPopup("Lỗi xử lý. Vui lòng kiểm tra kết nối mạng.");
            // Log lỗi
            Console.WriteLine($"[ERROR] Coordinator Flow: {ex.Message}");
        }
    }
}