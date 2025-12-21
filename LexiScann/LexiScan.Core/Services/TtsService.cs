using System;
using System.Globalization;
using System.Speech.Synthesis;

namespace LexiScanService
{
    public class TtsService
    {
        // [QUAN TRỌNG] Static = Chỉ 1 bộ đọc duy nhất trong toàn app
        private static readonly SpeechSynthesizer _synthesizer;

        static TtsService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }

        public void Speak(string text, double speed, string accent)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                _synthesizer.SpeakAsyncCancelAll(); // Ngắt lời cũ ngay lập tức

                // Ép kiểu sang int chuẩn của Windows (-10 đến 10)
                int rate = (int)speed;
                if (rate > 10) rate = 10;
                if (rate < -10) rate = -10;
                _synthesizer.Rate = rate;

                try
                {
                    _synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, new CultureInfo(accent));
                }
                catch
                {
                    _synthesizer.SelectVoiceByHints(VoiceGender.NotSet, VoiceAge.NotSet, 0, new CultureInfo("en-US"));
                }

                _synthesizer.SpeakAsync(text);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public void ReadText(string text) => Speak(text, 0, "en-US");
        public void Stop() => _synthesizer.SpeakAsyncCancelAll();
    }
}