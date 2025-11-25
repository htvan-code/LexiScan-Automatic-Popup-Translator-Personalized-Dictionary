// Project: LexiScan.Services
// File: TtsService.cs
using System.Speech.Synthesis;
public class TtsService
{
    private readonly SpeechSynthesizer _synthesizer;

    public TtsService()
    {
        // Cần tham chiếu tới System.Speech
        _synthesizer = new SpeechSynthesizer();
    }

    public void ReadText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _synthesizer.SpeakAsync(text);
        }
    }
}