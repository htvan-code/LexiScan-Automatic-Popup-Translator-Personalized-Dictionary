using System.Text.Json;

namespace LexiScan.Services.Translation;

public class TranslationService
{
    private readonly HttpClient _http = new();

    public async Task<string> TranslateAsync(string text)
    {
        var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=vi&dt=t&q={Uri.EscapeDataString(text)}";
        var json = await _http.GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement[0][0][0].GetString()!;
    }
}