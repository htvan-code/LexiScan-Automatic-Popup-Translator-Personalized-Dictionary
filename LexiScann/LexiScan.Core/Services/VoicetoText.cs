using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;

namespace LexiScan.Core.Services
{
    public class VoicetoText : IDisposable
    {
        private WaveInEvent _waveIn;
        private MemoryStream _audioStream;
        private WhisperFactory _whisperFactory;
        private WhisperProcessor _processor;

        // Đường dẫn tới file model anh vừa tải
        private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "ggml-tiny.bin");

        public event Action<string> TextRecognized;
        public event Action SpeechStarted;
        public event Action SpeechEnded;
        public event Action<int> AudioLevelUpdated;

        public bool IsRecording { get; private set; } = false;

        public VoicetoText()
        {
            // Khởi tạo engine Whisper
            if (File.Exists(_modelPath))
            {
                _whisperFactory = WhisperFactory.FromPath(_modelPath);
                _processor = _whisperFactory.CreateBuilder()
                    .WithLanguage("en") // Để tiếng Anh cho chuẩn từ đơn
                    .Build();
            }

            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = new WaveFormat(16000, 1); // Whisper chuẩn 16kHz
            _waveIn.DataAvailable += OnAudioDataAvailable;
        }

        private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
        {
            if (!IsRecording) return;
            _audioStream?.Write(e.Buffer, 0, e.BytesRecorded);

            // Tính âm lượng cho Elip nhảy
            long sum = 0;
            for (int i = 0; i < e.BytesRecorded; i += 2) sum += Math.Abs(BitConverter.ToInt16(e.Buffer, i));
            AudioLevelUpdated?.Invoke((int)(sum / (e.BytesRecorded / 2) / 300));
        }

        public void StartListening()
        {
            if (IsRecording || _processor == null) return;

            _audioStream = new MemoryStream();
            _waveIn.StartRecording();
            IsRecording = true;
            SpeechStarted?.Invoke();
        }

        public async void StopListening()
        {
            if (!IsRecording) return;
            IsRecording = false;
            _waveIn.StopRecording();

            if (_audioStream?.Length > 0)
            {
                var audioData = _audioStream.ToArray();

                // Chạy nhận diện trên luồng riêng để giao diện không bị đứng
                await Task.Run(async () => {
                    try
                    {
                        using var wavStream = new MemoryStream();
                        using (var reader = new RawSourceWaveStream(new MemoryStream(audioData), _waveIn.WaveFormat))
                        {
                            WaveFileWriter.WriteWavFileToStream(wavStream, reader);
                        }
                        wavStream.Position = 0;

                        string fullText = "";
                        await foreach (var result in _processor.ProcessAsync(wavStream))
                        {
                            fullText += result.Text;
                        }

                        if (!string.IsNullOrWhiteSpace(fullText))
                        {
                            // Whisper đôi khi nhận diện nhầm tiếng động thành dấu ngoặc hoặc [Music]
                            string cleaned = fullText.Trim().Replace(".", "").Replace("[", "").Replace("]", "");
                            TextRecognized?.Invoke(cleaned);
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine("Whisper Error: " + ex.Message); }
                });
            }
            SpeechEnded?.Invoke();
        }

        public void Dispose()
        {
            _waveIn?.Dispose();
            _audioStream?.Dispose();
            _processor?.Dispose();
            _whisperFactory?.Dispose();
        }
    }
}