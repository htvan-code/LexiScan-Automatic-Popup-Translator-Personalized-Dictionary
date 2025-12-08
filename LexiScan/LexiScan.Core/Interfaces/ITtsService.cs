using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScan.Core.Interfaces
{
    public interface ITtsService
    {
        Task SpeakAsync(string text, string language);
    }
}