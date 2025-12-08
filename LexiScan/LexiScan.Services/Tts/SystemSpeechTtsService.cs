using System.Speech.Synthesis;

namespace LexiScan.Services.Speech;

public class SystemSpeechTtsService
{
    private readonly SpeechSynthesizer _synth = new();

    public void Speak(string text)
    {
        _synth.Rate = 0;        // tốc độ đọc
        _synth.Volume = 100;   // âm lượng
        _synth.Speak(text);
    }
}
