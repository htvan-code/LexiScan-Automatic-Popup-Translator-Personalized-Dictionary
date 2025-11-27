
namespace LexiScanService
{
    internal class TtsService
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

    internal class SpeechSynthesizer
    {
        internal void SpeakAsync(string text)
        {
            throw new NotImplementedException();
        }
    }
}
