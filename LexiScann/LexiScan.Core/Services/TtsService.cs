using System;
using System.Linq;
using System.Speech.Synthesis;
using LexiScan.Core.Utils;

namespace LexiScan.Core.Services
{
    public class TtsService
    {
        private readonly SpeechSynthesizer _synth = new();

        public void Speak(string text, double speed, string accent)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            _synth.SpeakAsyncCancelAll();
            _synth.Rate = Math.Clamp((int)((speed - 1) * 10), -10, 10);

            var voiceName = VoiceMapper.Map(accent);
            if (voiceName != null)
            {
                try
                {
                    _synth.SelectVoice(voiceName);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine($"TTS Error: Could not select voice '{voiceName}'. Using default voice.");
                }
            }

            _synth.SpeakAsync(text);
        }

        public void Stop() => _synth.SpeakAsyncCancelAll();
    }
}