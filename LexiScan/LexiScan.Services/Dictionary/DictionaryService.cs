using System.Net.Http.Json;
using LexiScan.Core.Models;

namespace LexiScan.Services.Dictionary;

public class DictionaryService
{
    private readonly HttpClient _http = new();

    public async Task<DictionaryResult> LookupAsync(string word)
    {
        var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
        var data = await _http.GetFromJsonAsync<List<dynamic>>(url);

        return new DictionaryResult
        {
            Word = word,
            PartOfSpeech = data![0].meanings[0].partOfSpeech,
            Definition = data[0].meanings[0].definitions[0].definition
        };
    }
}