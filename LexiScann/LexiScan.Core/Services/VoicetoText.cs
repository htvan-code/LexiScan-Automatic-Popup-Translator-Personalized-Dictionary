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
       public VoicetoText()
       {
            try
            {     
                _recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));

                _recognizer.LoadGrammar(new DictationGrammar());

                _recognizer.SpeechRecognized += (s, e) =>
                {
                    if (e.Result != null && e.Result.Confidence > 0.5)
                    {
                        TextRecognized?.Invoke(e.Result.Text);
                    }
                    else
                    {
                        ErrorOccurred?.Invoke("Âm thanh không rõ ràng, vui lòng thử lại.");
                    }
                };
                _recognizer.SetInputToDefaultAudioDevice();
            }
            catch (Exception ex)
            {
                // Nếu máy chưa cài gói ngôn ngữ English, nó sẽ văng Exception ở đây
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
                _recognizer.Dispose(); // Giải phóng Micro cho các ứng dụng khác dùng
            }
        }
    }
}
