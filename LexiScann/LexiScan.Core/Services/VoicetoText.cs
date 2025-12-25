using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Whisper.net;

namespace LexiScan.Core.Services
{
    public class VoicetoText : IDisposable
    {
        private WaveInEvent _waveIn;
        private WhisperFactory? _factory;
        private WhisperProcessor? _processor;
        private MemoryStream _audioBuffer = new MemoryStream(); // Bộ đệm để gom âm thanh
        private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "ggml-tiny.bin");

        public event Action<string>? TextRecognized;
        public event Action<int>? AudioLevelUpdated;
        public event Action? SpeechStarted;
        public event Action? SpeechEnded;

        public bool IsRecording { get; private set; }

        public VoicetoText()
        {
            if (File.Exists(_modelPath))
            {
                _factory = WhisperFactory.FromPath(_modelPath);
                _processor = _factory.CreateBuilder().WithLanguage("en").Build();
            }
            _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(16000, 1) };
            _waveIn.DataAvailable += OnDataAvailable;
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (!IsRecording || _processor == null) return;

            // 1. Lưu vào bộ đệm
            _audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);

            // 2. Nhảy Elip
            long sum = 0;
            for (int i = 0; i < e.BytesRecorded; i += 2) sum += Math.Abs(BitConverter.ToInt16(e.Buffer, i));
            AudioLevelUpdated?.Invoke((int)(sum / (e.BytesRecorded / 2) / 300));

            // 3. Xử lý Streaming: Chỉ xử lý khi có đủ khoảng 0.8 giây âm thanh để tránh vụn vặt
            if (_audioBuffer.Length > 16000 * 0.8 * 2)
            {
                ProcessBuffer(false);
            }
        }

        private async void ProcessBuffer(bool isFinal)
        {
            if (_processor == null || _audioBuffer.Length == 0) return;

            try
            {
                var bytes = _audioBuffer.ToArray();
                var samples = new float[bytes.Length / 2];
                for (int i = 0; i < bytes.Length; i += 2)
                    samples[i / 2] = BitConverter.ToInt16(bytes, i) / 32768.0f;

                await foreach (var result in _processor.ProcessAsync(samples))
                {
                    if (!string.IsNullOrWhiteSpace(result.Text))
                    {
                        TextRecognized?.Invoke(result.Text);
                    }
                }

                // Nếu là Dictionary (thường bấm tắt ngay), ta xóa buffer sau khi bắn chữ
                if (isFinal) _audioBuffer.SetLength(0);
            }
            catch { }
        }

        public void StartListening()
        {
            if (IsRecording) return;
            _audioBuffer.SetLength(0); // Reset bộ đệm khi bắt đầu
            IsRecording = true;
            _waveIn.StartRecording();
            SpeechStarted?.Invoke();
        }

        public void StopListening()
        {
            if (!IsRecording) return;
            IsRecording = false;
            _waveIn.StopRecording();

            // Xử lý nốt những gì còn lại trong bộ đệm khi bấm tắt
            ProcessBuffer(true);
            SpeechEnded?.Invoke();
        }

        public void Dispose()
        {
            _waveIn?.Dispose();
            _processor?.Dispose();
            _factory?.Dispose();
            _audioBuffer.Dispose();
        }
    }
}