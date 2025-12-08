using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScan.Core.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateAsync(
            string input,
            string fromLang,
            string toLang);
    }
}