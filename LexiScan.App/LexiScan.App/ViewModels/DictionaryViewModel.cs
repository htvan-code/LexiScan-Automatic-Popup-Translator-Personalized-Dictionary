using LexiScan.App.Commands;
using LexiScan.Core;
using LexiScan.Core.Enums;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScan.Core.Utils;
using LexiScanData.Models;
using LexiScanData.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace LexiScan.App.ViewModels
{
    public class DictionaryViewModel : BaseViewModel
    {
        private DatabaseServices? _dbService;
        private readonly AppCoordinator _coordinator;
        private readonly SettingsService _settingsService;

        private string _searchText;
        private string _displayWord;
        private string _definitionText;
        private string _phoneticText;
        private string _selectedSuggestion;

        private bool _isListening;
        public bool IsListening { get => _isListening; set { _isListening = value; OnPropertyChanged(); } }

        private bool _isSpeaking;
        public bool IsSpeaking { get => _isSpeaking; set { _isSpeaking = value; OnPropertyChanged(); } }

        private double _voiceLevel;
        public double VoiceLevel
        {
            get => _voiceLevel;
            set { _voiceLevel = value; OnPropertyChanged(); }
        }
        private bool _isPinned;
        public bool IsPinned
        {
            get => _isPinned;
            set { _isPinned = value; OnPropertyChanged(); }
        }
        public ICommand PinToFirebaseCommand { get; }
        public DictionaryViewModel(AppCoordinator coordinator)
        {
            _coordinator = coordinator;
            _settingsService = new SettingsService();

            string uid = SessionManager.CurrentUserId;
            if (!string.IsNullOrEmpty(uid)) _dbService = new DatabaseServices(uid);

            _coordinator.TranslationCompleted += OnTranslationResultReceived;
            SuggestionList = new ObservableCollection<string>();

            _coordinator.VoiceRecognitionStarted += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    IsListening = true;
                    IsSpeaking = true;
                });
            };

            _coordinator.VoiceRecognitionEnded += () =>
            {
                var app = System.Windows.Application.Current;
                if (app == null) return;
                app.Dispatcher.Invoke(() => {
                    IsListening = false;
                    VoiceLevel = 0;
                    CommandManager.InvalidateRequerySuggested(); 
                });
            };

            SearchCommand = new RelayCommand(async (o) =>
            {
                if (string.IsNullOrWhiteSpace(SearchText)) return;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SuggestionList.Clear();
                });
                await _coordinator.ExecuteSearchAsync(SearchText);
            });

            StartVoiceSearchCommand = new RelayCommand((o) =>
            {
                if (!IsListening)
                {
                    _coordinator.StartVoiceSearch(VoiceSource.Dictionary);
                }
                else
                {
                    _coordinator.StopVoiceSearch();
                }
            });

            SpeakResultCommand = new RelayCommand(ExecuteSpeakResult);

            _coordinator.VoiceSearchCompleted += (text) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    if (_coordinator.CurrentVoiceSource == VoiceSource.Dictionary)
                    {
                        string cleanedText = text.Trim().Replace(".", "").Replace("[", "").Replace("]", "").ToLower();

                        if (!string.IsNullOrWhiteSpace(cleanedText))
                        {
                            SearchText = cleanedText;

                            if (SearchCommand.CanExecute(null)) SearchCommand.Execute(null);
                        }
                    }
                });
            };

            _coordinator.AudioLevelUpdated += (level) =>
            {
                double newSize = 22.0 + (level * 1.5);
                if (newSize > 35) newSize = 35;
                if (newSize < 20) newSize = 20;
                VoiceLevel = newSize;
            };

            PinToFirebaseCommand = new RelayCommand(ExecutePinToFirebase);
        }

        public ObservableCollection<string> SuggestionList { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();

                    if (!string.IsNullOrWhiteSpace(_searchText))
                    {
                        if (_selectedSuggestion != _searchText)
                        {
                            LoadSuggestions(_searchText);
                        }
                    }
                    else
                    {
                        SuggestionList.Clear();
                    }
                }
            }
        }

        public string SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                _selectedSuggestion = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(value))
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        private async void ExecutePinToFirebase(object? obj)
        {
            if (_dbService == null)
            {
                if (!string.IsNullOrEmpty(SessionManager.CurrentUserId))
                    _dbService = new DatabaseServices(SessionManager.CurrentUserId);
                else return;
            }

            string textToSave = !string.IsNullOrWhiteSpace(DisplayWord) ? DisplayWord : SearchText;

            string meaningToSave = DefinitionText;

            if (string.IsNullOrWhiteSpace(textToSave)) return;

            try
            {
                string? existingKey = await _dbService.FindSavedKeyAsync(textToSave);

                if (existingKey != null)
                {
                    await _dbService.DeleteSavedItemAsync(existingKey);
                    IsPinned = false;
                }
                else
                {
                    await _dbService.SaveSimpleVocabularyAsync(textToSave, meaningToSave);
                    IsPinned = true;
                }

                GlobalEvents.RaisePersonalDictionaryUpdated();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi lưu: " + ex.Message);
            }
        }
        public string DisplayWord { get => _displayWord; set { _displayWord = value; OnPropertyChanged(); } }
        public string DefinitionText { get => _definitionText; set { _definitionText = value; OnPropertyChanged(); } }
        public string PhoneticText { get => _phoneticText; set { _phoneticText = value; OnPropertyChanged(); } }

        public ICommand SearchCommand { get; }
        public ICommand StartVoiceSearchCommand { get; }
        public ICommand SpeakResultCommand { get; }

        private async void LoadSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) { SuggestionList.Clear(); return; }
            var suggestions = await _coordinator.GetRecommendWordsAsync(query);
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                SuggestionList.Clear();
                foreach (var s in suggestions) SuggestionList.Add(s);
            });
        }

        private async void OnTranslationResultReceived(TranslationResult result)
        {
            if (result == null) return;

            DisplayWord = result.OriginalText ?? "";
            PhoneticText = (!string.IsNullOrEmpty(result.Phonetic)) ? $"/{result.Phonetic}/" : "";

            var sb = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(result.TranslatedText))
            {
                sb.AppendLine(result.TranslatedText);
            }

            if (result.Meanings != null && result.Meanings.Count > 0)
            {
                sb.AppendLine();
                foreach (var m in result.Meanings)
                {
                    sb.AppendLine($"★ {m.PartOfSpeech}");
                    foreach (var def in m.Definitions)
                    {
                        sb.AppendLine($"    - {def}");
                    }
                    sb.AppendLine();
                }
            }

            DefinitionText = sb.ToString().Trim();

            if (_dbService == null && !string.IsNullOrEmpty(SessionManager.CurrentUserId))
            {
                _dbService = new DatabaseServices(SessionManager.CurrentUserId);
            }

            if (_dbService != null)
            {
                try
                {
                    string wordToCheck = !string.IsNullOrEmpty(DisplayWord) ? DisplayWord : SearchText;
                    // Hàm này trả về Key nếu có, null nếu không
                    string? key = await _dbService.FindSavedKeyAsync(wordToCheck);

                    // Nếu key khác null => Đã lưu => IsPinned = true
                    IsPinned = (key != null);
                }
                catch { IsPinned = false; }
            }

            var currentSettings = _settingsService.LoadSettings();

            if (currentSettings.AutoPronounceOnLookup)
            {
                ExecuteSpeakResult(null);
            }

            // [SỬA LẠI ĐOẠN NÀY] Chỉ lưu khi cài đặt BẬT
            if (currentSettings.AutoSaveHistoryToDictionary)
            {
                if (_dbService == null && !string.IsNullOrEmpty(SessionManager.CurrentUserId))
                {
                    _dbService = new DatabaseServices(SessionManager.CurrentUserId);
                }

                // --- LƯU VÀO LỊCH SỬ ---
                if (_dbService != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    {
                        try
                        {
                            await _dbService.AddHistoryAsync(new Sentences
                            {
                                SourceText = !string.IsNullOrEmpty(DisplayWord) ? DisplayWord : SearchText,
                                TranslatedText = !string.IsNullOrEmpty(result.TranslatedText) ? result.TranslatedText : "Tra từ điển",
                                CreatedDate = System.DateTime.Now
                            });
                        }
                        catch (System.Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Lỗi lưu lịch sử: " + ex.Message);
                        }
                    });
                }
            }
        }

        private void ExecuteSpeakResult(object obj)
        {
            string textToRead = !string.IsNullOrWhiteSpace(DisplayWord) ? DisplayWord : SearchText;
            if (string.IsNullOrWhiteSpace(textToRead)) return;

            var settings = _settingsService.LoadSettings();

            double speedRate = 0;
            switch (settings.Speed)
            {
                case SpeechSpeed.Slower: speedRate = -5; break;
                case SpeechSpeed.Slow: speedRate = -3; break;
                case SpeechSpeed.Normal: speedRate = 0; break;
            }

            string accent = (settings.Voice == SpeechVoice.EngUK) ? "en-GB" : "en-US";
            _coordinator.Speak(textToRead, speedRate, accent);
        }
    }
}