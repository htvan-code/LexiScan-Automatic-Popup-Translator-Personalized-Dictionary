using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LexiScan.Core.Models;
using LexiScan.Core.Enums;

namespace LexiScan.Core.Services
{
    public class TranslationService
    {
        private static readonly HttpClient _http = new();

        public async Task<TranslationResult> ProcessTranslationAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new TranslationResult { Status = ServiceStatus.InternalError, ErrorMessage = "Không có văn bản để xử lý." };
            }

            string cleanedText = text.Trim();
            var type = DetectInputType(cleanedText);

            // 1. Dịch thuật trực tiếp (Google Translate) cho mọi input
            var result = await TranslateSentence(cleanedText);
            result.InputType = type;

            // 2. Làm giàu dữ liệu từ điển nếu là từ đơn và dịch thành công
            if (type == InputType.SingleWord && result.Status == ServiceStatus.Success)
            {
                var dictResult = await DictionaryLookup(cleanedText);

                // Chỉ lấy Phonetic và Meanings, không ghi đè TranslatedText
                result.Phonetic = dictResult.Phonetic;
                result.Meanings = dictResult.Meanings;
            }

            return result;
        }

        public InputType DetectInputType(string text)
        {
            var wc = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (wc == 1)
            {
                return InputType.SingleWord;
            }
            return InputType.PhraseOrSentence;
        }

        public async Task<TranslationResult> TranslateSentence(string text, string sl = "en")
        {
            try
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sl}&tl=vi&dt=t&q={Uri.EscapeDataString(text)}";
                var raw = await _http.GetStringAsync(url);
                var json = JsonConvert.DeserializeObject<dynamic>(raw);

                string translated = "";
                foreach (var seg in json[0])
                    translated += seg[0];

                return new TranslationResult
                {
                    OriginalText = text,
                    TranslatedText = translated,
                    InputType = InputType.PhraseOrSentence, // Sẽ được cập nhật lại ở ProcessTranslationAsync
                    Status = ServiceStatus.Success
                };
            }
            catch (Exception ex)
            {
                return new TranslationResult
                {
                    OriginalText = text,
                    InputType = InputType.PhraseOrSentence,
                    Status = ServiceStatus.NetworkError,
                    ErrorMessage = $"Lỗi kết nối API dịch: {ex.Message}"
                };
            }
        }

        private async Task<TranslationResult> DictionaryLookup(string word)
        {
            try
            {
                var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
                var raw = await _http.GetStringAsync(url);

                if (raw.Contains("message"))
                {

                    return new TranslationResult { OriginalText = word, InputType = InputType.SingleWord, Status = ServiceStatus.NotFound };
                }

                var data = JsonConvert.DeserializeObject<dynamic[]>(raw);
                var firstEntry = data.First();

                var result = new TranslationResult
                {
                    OriginalText = word,
                    InputType = InputType.SingleWord,
                    Status = ServiceStatus.Success,
                    Phonetic = firstEntry.phonetics != null && firstEntry.phonetics.Count > 0 ? (string)firstEntry.phonetics[0].text : null,
                    Meanings = new List<Meaning>()
                };

                foreach (var meaningEntry in firstEntry.meanings)
                {
                    var partOfSpeech = (string)meaningEntry.partOfSpeech;
                    foreach (var definitionEntry in meaningEntry.definitions)
                    {
                        var definition = (string)definitionEntry.definition;
                        if (string.IsNullOrWhiteSpace(definition)) continue;

                        List<string> examples = new List<string>();
                        if (definitionEntry.example != null)
                        {
                            examples.Add((string)definitionEntry.example);
                        }

                        result.Meanings.Add(new Meaning
                        {
                            PartOfSpeech = partOfSpeech,
                            Definition = definition,
                            Examples = examples
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new TranslationResult
                {
                    OriginalText = word,
                    InputType = InputType.SingleWord,
                    Status = ServiceStatus.ApiError,
                    ErrorMessage = $"Lỗi xử lý API từ điển: {ex.Message}"
                };
            }
        }
    }
}