using System.Globalization;
using System.Speech.Synthesis;

namespace LexiScanService
{
    public class TtsService
    {
        private readonly SpeechSynthesizer _synthesizer;

        public TtsService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();

            // 🔥 ÉP GIỌNG TIẾNG ANH
            _synthesizer.SelectVoiceByHints(
                VoiceGender.NotSet,
                VoiceAge.NotSet,
                0,
                new CultureInfo("en-US")
            );
        }

        public void ReadText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                _synthesizer.SpeakAsyncCancelAll();
                _synthesizer.SpeakAsync(text);
            }
        }
    }
}
