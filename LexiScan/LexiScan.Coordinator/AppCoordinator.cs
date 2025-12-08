using LexiScan.Services.Classification;
using LexiScan.Services.Dictionary;
using LexiScan.Services.Keywords;
using LexiScan.Services.Translation;
using LexiScan.Services.Speech;

namespace LexiScan.Coordinator;

public class AppCoordinator
{
    private readonly DictionaryService _dictionary = new();
    private readonly TranslationService _translator = new();
    private readonly SystemSpeechTtsService _tts = new();

    public async Task ProcessAsync(string input)
    {
        var type = InputDetective.Detect(input);

        Console.WriteLine("\n--- RESULT ---");

        if (type == InputType.Word)
        {
            var result = await _dictionary.LookupAsync(input);

            Console.WriteLine($"WORD: {result.Word}");
            Console.WriteLine($"{result.PartOfSpeech}: {result.Definition}");
        }
        else
        {
            var translated = await _translator.TranslateAsync(input);
            Console.WriteLine($"TRANSLATION: {translated}");

            var keywords = KeywordExtractor.GetKeywords(input);
            Console.WriteLine("KEYWORDS: " + string.Join(", ", keywords));
        }

        // Đọc INPUT không đọc bản dịch
        _tts.Speak(input);
    }
}
