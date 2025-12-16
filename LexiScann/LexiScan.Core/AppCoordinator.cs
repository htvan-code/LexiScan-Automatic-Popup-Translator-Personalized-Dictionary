using System;
using System.Threading.Tasks;
using LexiScan.Core.Models;
using LexiScan.Core.Services;
using LexiScan.Core.Enums;

namespace LexiScan.Core
{
    public class AppCoordinator
    {
        private readonly TranslationService _translationService;

        public AppCoordinator(TranslationService translationService)
        {
            _translationService = translationService;
        }

        public event Action<TranslationResult>? TranslationCompleted;

        public async Task HandleClipboardTextAsync(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;

            try
            {
                var result = await _translationService
                    .ProcessTranslationAsync(rawText.Trim());

                TranslationCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                TranslationCompleted?.Invoke(new TranslationResult
                {
                    OriginalText = rawText,
                    Status = ServiceStatus.InternalError,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
