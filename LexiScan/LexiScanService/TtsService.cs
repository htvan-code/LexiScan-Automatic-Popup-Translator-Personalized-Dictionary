using System.Speech.Synthesis; // Dùng thư viện chuẩn của .NET

namespace LexiScanService
{
    public class TtsService
    {
        private readonly SpeechSynthesizer _synthesizer;

        public TtsService()
        {
            _synthesizer = new SpeechSynthesizer();
        }

        public void ReadText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // Gọi hàm SpeakAsync của System.Speech
                _synthesizer.SpeakAsync(text);
            }
        }
    }
}