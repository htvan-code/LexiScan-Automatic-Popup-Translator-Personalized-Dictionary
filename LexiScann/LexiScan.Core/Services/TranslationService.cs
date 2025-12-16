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
            _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public async Task<TranslationResult> ProcessTranslationAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new TranslationResult { Status = ServiceStatus.InternalError, ErrorMessage = "Không có văn bản." };

            string cleanedText = text.Trim();
            return await TranslateWithGoogleFull(cleanedText);
        }

        private async Task<TranslationResult> TranslateWithGoogleFull(string text, string sl = "en", string tl = "vi")
        {
            try
            {
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sl}&tl={tl}&dt=t&dt=bd&dt=rm&q={Uri.EscapeDataString(text)}";

                var response = await _http.GetStringAsync(url);
                var json = JArray.Parse(response);

                var result = new TranslationResult
                {
                    OriginalText = text,
                    Status = ServiceStatus.Success,
                    InputType = InputType.PhraseOrSentence,
                    Meanings = new List<Meaning>()
                };

                // --- 1. LẤY NGHĨA DỊCH CHÍNH ---
                if (json.Count > 0 && json[0] is JArray mainArray && mainArray.Count > 0 && mainArray[0] is JArray firstItem)
                {
                    result.TranslatedText = firstItem[0]?.ToString();
                }

                // --- 2. LẤY PHIÊN ÂM (ĐÃ FIX LỖI CRASH) ---
                try
                {
                    if (json.Count > 0 && json[0] is JArray loopArray)
                    {
                        foreach (var item in loopArray)
                        {
                            if (item is JArray subArray && subArray.Count >= 3)
                            {
                                // Google thường trả về: [null, null, "phiên_âm", null]
                                var p3 = subArray.Count > 3 ? subArray[3]?.ToString() : null;
                                var p2 = subArray.Count > 2 ? subArray[2]?.ToString() : null;

                                if (!string.IsNullOrEmpty(p3))
                                {
                                    result.Phonetic = p3;
                                    break;
                                }

                                // Nếu phần tử đầu tiên là null thì khả năng cao index 2 là phiên âm
                                if ((subArray[0] == null || subArray[0].Type == JTokenType.Null) && !string.IsNullOrEmpty(p2))
                                {
                                    result.Phonetic = p2;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch { /* Bỏ qua lỗi phiên âm để chương trình vẫn chạy tiếp */ }

                // --- 3. LẤY ĐỊNH NGHĨA TỪ ĐIỂN ---
                if (json.Count > 1 && json[1] is JArray dictArray)
                {
                    result.InputType = InputType.SingleWord;

                    foreach (var entry in dictArray)
                    {
                        var partOfSpeech = entry[0]?.ToString();

                        var meaningObj = new Meaning
                        {
                            PartOfSpeech = ConvertPosToVietnamese(partOfSpeech),
                            Definitions = new List<string>()
                        };

                        if (entry.Count() > 1 && entry[1] is JArray defsArray)
                        {
                            foreach (var def in defsArray)
                            {
                                meaningObj.Definitions.Add(def.ToString());
                            }
                        }

                        result.Meanings.Add(meaningObj);
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

        private string ConvertPosToVietnamese(string pos)
        {
            if (string.IsNullOrEmpty(pos)) return "";
            return pos.ToLower() switch
            {
                "noun" => "Danh Từ",
                "verb" => "Động Từ",
                "adjective" => "Tính Từ",
                "adverb" => "Trạng Từ",
                "pronoun" => "Đại Từ",
                "preposition" => "Giới Từ",
                "conjunction" => "Liên Từ",
                "interjection" => "Thán Từ",
                _ => char.ToUpper(pos[0]) + pos.Substring(1)
            };
        }
    }
}