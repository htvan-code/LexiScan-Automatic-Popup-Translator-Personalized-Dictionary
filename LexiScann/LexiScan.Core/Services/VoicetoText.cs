using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NAudio.Wave;
using Whisper.net;

namespace LexiScan.Core.Services
{
    public class VoicetoText : IDisposable
    {
        private WaveInEvent _waveIn;
        private WhisperProcessor? _processor;
        private WhisperFactory? _factory;
        private MemoryStream _audioBuffer = new MemoryStream();
        private MemoryStream _streamingBuffer = new MemoryStream();
        private bool _isProcessing = false;
        private bool _isStopping = false;

        private System.Timers.Timer _silenceTimer;
        private const double SilenceThreshold = 3500; // 3.5 giây im lặng là tự tắt

        public event Action<string>? TextRecognized;
        public event Action<int>? AudioLevelUpdated;
        public event Action? SpeechStarted;
        public event Action? SpeechEnded;
        public event Action? SilenceDetected;

        public bool IsRecording { get; private set; }

        public VoicetoText()
        {
            var modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "ggml-tiny.bin");
            if (File.Exists(modelPath))
            {
                _factory = WhisperFactory.FromPath(modelPath);
                // [TỐI ƯU] Dùng Greedy Sampling để bắt từ vựng chuẩn xác hơn
                _processor = _factory.CreateBuilder()
                    .WithLanguage("en")
                    .Build();
            }

            _silenceTimer = new System.Timers.Timer(SilenceThreshold);
            _silenceTimer.AutoReset = false;
            _silenceTimer.Elapsed += (s, e) => { if (IsRecording) SilenceDetected?.Invoke(); };

            _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
            _waveIn.DataAvailable += (s, e) =>
            {
                if (!IsRecording) return;

                // 1. Tính âm lượng
                long sum = 0;
                for (int i = 0; i < e.BytesRecorded; i += 2) sum += Math.Abs(BitConverter.ToInt16(e.Buffer, i));
                int level = (int)(sum / (e.BytesRecorded / 2) / 300);
                AudioLevelUpdated?.Invoke(level);

                // 2. [CỰC QUAN TRỌNG] Ngưỡng lọc tiếng ồn (Gate)
                // Chỉ nạp vào Buffer khi âm lượng > 800 để tránh rác "blankaudio"
                bool hasVoice = false;
                for (int i = 0; i < e.BytesRecorded; i += 2)
                {
                    if (Math.Abs(BitConverter.ToInt16(e.Buffer, i)) > 800)
                    {
                        hasVoice = true;
                        break;
                    }
                }

                if (hasVoice)
                {
                    lock (_audioBuffer)
                    {
                        _audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);
                        _streamingBuffer.Write(e.Buffer, 0, e.BytesRecorded);
                    }
                    _silenceTimer.Stop();
                    _silenceTimer.Start();
                }

                // Nhảy chữ Streaming cho Translation (mỗi 0.8s)
                if (_streamingBuffer.Length > 25600 && !_isProcessing)
                {
                    _ = ProcessStreaming();
                }
            };
        }

        private async Task ProcessStreaming()
        {
            if (_processor == null || _isProcessing) return;
            _isProcessing = true;
            byte[] data;
            lock (_audioBuffer) { data = _streamingBuffer.ToArray(); _streamingBuffer.SetLength(0); }

            try
            {
                var samples = new float[data.Length / 2];
                for (int i = 0; i < data.Length; i += 2) samples[i / 2] = BitConverter.ToInt16(data, i) / 32768.0f;

                await foreach (var result in _processor.ProcessAsync(samples))
                {
                    // [BỘ LỌC] Xóa sạch blankaudio và ký tự rác
                    string clean = Regex.Replace(result.Text, @"(?i)blankaudio|\[.*?\]|[^a-zA-Z0-9\s]", "").Trim();
                    if (!string.IsNullOrWhiteSpace(clean) && clean.Length > 2)
                    {
                        TextRecognized?.Invoke(clean);
                    }
                }
            }
            finally { _isProcessing = false; }
        }

        public void StartListening()
        {
            if (IsRecording || _isStopping) return;
            lock (_audioBuffer) { _audioBuffer.SetLength(0); _streamingBuffer.SetLength(0); }
            IsRecording = true;
            _waveIn.StartRecording();
            SpeechStarted?.Invoke();
        }

        public async void StopListening()
        {
            if (!IsRecording || _isStopping) return;
            _isStopping = true; _silenceTimer.Stop(); _waveIn.StopRecording(); IsRecording = false;
            await Task.Delay(400);

            // Xử lý nốt đoạn cuối chuẩn nhất cho Dictionary
            byte[] data; lock (_audioBuffer) { data = _audioBuffer.ToArray(); _audioBuffer.SetLength(0); }
            if (data.Length > 0)
            {
                var samples = new float[data.Length / 2];
                for (int i = 0; i < data.Length; i += 2) samples[i / 2] = BitConverter.ToInt16(data, i) / 32768.0f;
                await foreach (var result in _processor.ProcessAsync(samples))
                {
                    string clean = Regex.Replace(result.Text, @"(?i)blankaudio|\[.*?\]|[^a-zA-Z0-9\s]", "").Trim();
                    if (!string.IsNullOrWhiteSpace(clean)) TextRecognized?.Invoke(clean);
                }
            }
            _isStopping = false; SpeechEnded?.Invoke();
        }

        public void Dispose() { _silenceTimer?.Dispose(); _waveIn?.Dispose(); _processor?.Dispose(); _factory?.Dispose(); }
    }
}