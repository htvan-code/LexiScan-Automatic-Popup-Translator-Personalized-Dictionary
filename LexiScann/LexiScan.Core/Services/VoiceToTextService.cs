using System;
using System.Speech.Recognition; 
using System.Threading.Tasks;

namespace LexiScan.Core.Services
{
    public class VoiceToTextService
    {
        // Sử dụng SpeechRecognizer cho việc nhận dạng giọng nói liên tục
        private readonly SpeechRecognizer _recognizer = new();

        // Sự kiện để báo về cho AppCoordinator khi nhận dạng xong
        public event Action<string> VoiceRecognized;
        public event Action ListeningStopped;

        public VoiceToTextService()
        {
            // Tải Grammar: Ở đây dùng DictationGrammar để nhận dạng tự do
            _recognizer.LoadGrammar(new DictationGrammar());
            _recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
            _recognizer.StateChanged += Recognizer_StateChanged;
        }

        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Chỉ gửi kết quả nếu độ tự tin cao (ví dụ: > 70%)
            if (e.Result.Confidence > 0.7)
            {
                VoiceRecognized?.Invoke(e.Result.Text);
            }
            StopListening();
        }

        private void Recognizer_StateChanged(object sender, StateChangedEventArgs e)
        {
            if (e.RecognizerState == RecognizerState.Idle)
            {
                ListeningStopped?.Invoke();
            }
        }

        public void StartListening()
        {
            if (_recognizer.State == RecognizerState.Idle)
            {
                // Bắt đầu lắng nghe một lần (Single)
                _recognizer.RecognizeAsync(RecognizeMode.Single);
            }
        }

        public void StopListening()
        {
            _recognizer.RecognizeAsyncCancel();
        }
    }
}