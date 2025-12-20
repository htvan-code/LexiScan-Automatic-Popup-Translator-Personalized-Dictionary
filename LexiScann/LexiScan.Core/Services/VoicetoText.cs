using System;
using System.Speech.Recognition;
using System.Globalization;
using System.Diagnostics;

namespace LexiScan.Core.Services
{
    public class VoicetoText : IDisposable
    {
        private readonly SpeechRecognitionEngine _recognizer;
        public event Action<string> TextRecognized;
        public event Action<string> ErrorOccurred;
        public event Action SpeechStarted;
        public event Action SpeechEnded;
        public event Action<int> AudioLevelUpdated;

        public VoicetoText()
        {
            try
            {
                _recognizer = new SpeechRecognitionEngine(new CultureInfo("en-US"));

                _recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(1.0); 
                _recognizer.BabbleTimeout = TimeSpan.FromSeconds(0);      
                _recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(1.5);

                _recognizer.LoadGrammar(new DictationGrammar());

                _recognizer.AudioLevelUpdated += (s, e) =>
                {
                    AudioLevelUpdated?.Invoke(e.AudioLevel);
                };

                _recognizer.SpeechDetected += (s, e) => SpeechStarted?.Invoke();

                _recognizer.SpeechRecognized += (s, e) =>
                {
                    SpeechEnded?.Invoke();
                    if (e.Result != null)
                    {
                        if (e.Result.Confidence > 0.01 && !string.IsNullOrWhiteSpace(e.Result.Text))
                        {
                            TextRecognized?.Invoke(e.Result.Text);
                        }
                    }
                };

                _recognizer.SpeechRecognitionRejected += (s, e) => {
                    SpeechEnded?.Invoke();
                    if (e.Result != null && e.Result.Alternates.Count > 0)
                    {
                        TextRecognized?.Invoke(e.Result.Alternates[0].Text);
                    }
                };

                _recognizer.RecognizeCompleted += (s, e) => SpeechEnded?.Invoke();
                _recognizer.SetInputToDefaultAudioDevice();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi khởi tạo VoiceToText: " + ex.Message);
            }
        }

        public void StartListening()
        {
            try
            {
                _recognizer.RecognizeAsyncCancel();

                _recognizer.RecognizeAsync(RecognizeMode.Single);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi StartListening: " + ex.Message);
                SpeechEnded?.Invoke();
            }
        }

        public void StopListening()
        {
            try { _recognizer.RecognizeAsyncCancel(); } catch { }
        }

        public void Dispose()
        {
            _recognizer?.Dispose();
        }
    }
}