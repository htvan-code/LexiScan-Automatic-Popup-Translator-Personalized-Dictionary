using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using LexiScan.Core.Models;
using LexiScan.Core.Enums;

namespace LexiScan.Core.Services
{
    public class TranslationService
    {
        private static readonly HttpClient _http;

        static TranslationService()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            );
        }
        public async Task<TranslationResult> TranslateForMainApp(string text, string sl, string tl)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            return await TranslateWithGoogleFull(text.Trim(), sl, tl);
        }

        public async Task<TranslationResult> ProcessTranslationAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new TranslationResult
                {
                    Status = ServiceStatus.InternalError,
                    ErrorMessage = "Không có văn bản."
                };
            }
            string cleaned = text.Trim();
            return await TranslateWithGoogleFull(cleaned);
        }

        private async Task<TranslationResult> TranslateWithGoogleFull(
            string text, string sl = "en", string tl = "vi")
        {
            try
            {
                var url =
                    $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sl}&tl={tl}&dt=t&dt=bd&dt=rm&q={Uri.EscapeDataString(text)}";

                var response = await _http.GetStringAsync(url);
                var json = JArray.Parse(response);

                var result = new TranslationResult
                {
                    OriginalText = text,
                    Status = ServiceStatus.Success,
                    Meanings = new List<Meaning>()
                };

                // ========= XÁC ĐỊNH LOẠI INPUT =========
                result.InputType = text.Contains(" ")
                    ? InputType.PhraseOrSentence
                    : InputType.SingleWord;

                // ========= 1. LẤY NGHĨA DỊCH =========
                if (json.Count > 0 &&
                    json[0] is JArray arr0 &&
                    arr0.Count > 0 &&
                    arr0[0] is JArray inner0)
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var segment in arr0)
                    {
                        if (segment is JArray seg && seg.Count > 0)
                            sb.Append(seg[0]?.ToString());
                    }
                    result.TranslatedText = sb.ToString().Trim();
                }

                // ========= 2. LẤY PHIÊN ÂM TỪ dt=rm =========
                try
                {
                    if (json.Count > 0 && json[0] is JArray phoneticArray)
                    {
                        foreach (var row in phoneticArray)
                        {
                            if (row is JArray sub && sub.Count >= 3)
                            {
                                var p3 = sub.Count > 3 ? sub[3]?.ToString() : null;
                                var p2 = sub.Count > 2 ? sub[2]?.ToString() : null;

                                if (!string.IsNullOrEmpty(p3))
                                {
                                    result.Phonetic = CleanPhonetic(p3);
                                    break;
                                }

                                if ((sub[0] == null || sub[0].Type == JTokenType.Null)
                                    && !string.IsNullOrEmpty(p2))
                                {
                                    result.Phonetic = CleanPhonetic(p2);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch { }

                // ========= 2.2 FALLBACK PHIÊN ÂM =========
                if (string.IsNullOrEmpty(result.Phonetic))
                {
                    try
                    {
                        var alt = json[0]?[1]?[3]?.ToString();
                        if (!string.IsNullOrEmpty(alt))
                            result.Phonetic = CleanPhonetic(alt);
                    }
                    catch { }
                }

                // ========= 3. LẤY ĐỊNH NGHĨA TỪ ĐIỂN =========
                if (!text.Contains(" ") && json.Count > 1 && json[1] is JArray dictArray)
                {
                    result.InputType = InputType.SingleWord;

                    foreach (var entry in dictArray)
                    {
                        var pos = entry[0]?.ToString();
                        var m = new Meaning
                        {
                            PartOfSpeech = ConvertPosToVietnamese(pos),
                            Definitions = new List<string>()
                        };

                        if (entry.Count() > 1 && entry[1] is JArray defArr)
                        {
                            foreach (var d in defArr)
                                m.Definitions.Add(d.ToString());
                        }

                        result.Meanings.Add(m);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new TranslationResult
                {
                    OriginalText = text,
                    Status = ServiceStatus.ApiError,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<string>> GetGoogleSuggestionsAsync(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return new List<string>();

            if (prefix.Contains(" ")) return new List<string>();

            string cleanPrefix = prefix.Trim().ToLower();

            try
            {
                string url = $"https://api.datamuse.com/words?sp={Uri.EscapeDataString(cleanPrefix)}*&max=15";
                var response = await _http.GetStringAsync(url);
                var jsonArray = JArray.Parse(response);
                var result = new List<string>();

                foreach (var item in jsonArray)
                {
                    var word = item["word"]?.ToString();
                    if (!string.IsNullOrEmpty(word))
                    {
                        string lowerWord = word.ToLower();

                        if (lowerWord.Contains(" ")) continue;
                        if (lowerWord.Any(c => char.IsPunctuation(c))) continue;
                        if (lowerWord.Any(c => char.IsDigit(c))) continue;

                        result.Add(lowerWord);
                    }
                }
                return result.Take(5).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
        // =================== Helpers ===================
        private string ConvertPosToVietnamese(string pos)
        {
            if (string.IsNullOrEmpty(pos))
                return "";
            return char.ToUpper(pos[0]) + pos.Substring(1);
        }

        private string CleanPhonetic(string p)
        {
            if (string.IsNullOrEmpty(p)) return "";
            return p.Replace("/", "").Trim();
        }
    }
}
