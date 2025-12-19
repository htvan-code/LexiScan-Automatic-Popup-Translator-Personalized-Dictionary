using System;
using System.Speech.Recognition;
using System.Globalization;

namespace LexiScan.Core.Services
{
    public class VoicetoText : IDisposable
    {
        private readonly SpeechRecognitionEngine _recognizer;
        public event Action<string> TextRecognized;
        public event Action<string> ErrorOccurred;

        public event Action SpeechStarted;
        public event Action SpeechEnded;
        public VoicetoText()
        {
            try
            {
                _recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));
                _recognizer.LoadGrammar(new DictationGrammar());

                // Khi máy bắt đầu nhận diện thấy có tiếng người nói
                _recognizer.SpeechDetected += (s, e) => SpeechStarted?.Invoke();

                _recognizer.SpeechRecognized += (s, e) =>
                {
                    SpeechEnded?.Invoke(); // Báo cho P3 tắt hiệu ứng nháy
                    if (e.Result != null && e.Result.Confidence > 0.5)
                    {
                        TextRecognized?.Invoke(e.Result.Text);
                    }
                    else
                    {
                        ErrorOccurred?.Invoke("Âm thanh không rõ ràng, vui lòng thử lại.");
                    }
                };

                // Trường hợp người dùng bấm nhưng không nói gì hoặc lỗi engine
                _recognizer.RecognizeCompleted += (s, e) => SpeechEnded?.Invoke();

                _recognizer.SetInputToDefaultAudioDevice();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khởi tạo VoiceToText: " + ex.Message);
            }
        }
        public void StartListening()
        {
            try
            {
                _recognizer.RecognizeAsync(RecognizeMode.Single);
            }
            catch (Exception ex)
            {
                SpeechEnded?.Invoke();
                ErrorOccurred?.Invoke("Không thể khởi động Micro: " + ex.Message);
            }
        }

        public void StopListening()
        {
            _recognizer.RecognizeAsyncCancel();
        }
        public void Dispose()
        {
            if (_recognizer != null)
            {
                _recognizer.Dispose();
            }
        }
    }
}
