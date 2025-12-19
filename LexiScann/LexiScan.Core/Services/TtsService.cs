using System;
using System.Linq;
using System.Speech.Synthesis;
using LexiScan.Core.Utils;

namespace LexiScan.Core.Services
{
    public class TtsService
    {
        private readonly SpeechSynthesizer _synth = new();

        // Lấy danh sách tất cả giọng Anh-Anh, Anh-Mỹ có trong máy
        public List<InstalledVoice> GetInstalledVoices()
        {
            return _synth.GetInstalledVoices().ToList();
        }
        public void Speak(string text, double speed, string accent)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            _synth.SpeakAsyncCancelAll();
            _synth.Rate = Math.Clamp((int)((speed - 1) * 10), -10, 10);

            var voiceName = VoiceMapper.Map(accent);
            if (!string.IsNullOrEmpty(voiceName))
            {
                try
                {
                    _synth.SelectVoice(voiceName);
                }
                catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"TTS Warning: Không tìm thấy giọng {voiceName}. Hệ thống sẽ dùng giọng mặc định. Chi tiết: {ex.Message}");
                }
            }
            _synth.SpeakAsync(text);
        }
        public void Stop() => _synth.SpeakAsyncCancelAll();
    }
}