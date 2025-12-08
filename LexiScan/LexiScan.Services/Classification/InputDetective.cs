using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexiScan.Services.Classification;

public enum InputType
{
    Word,
    Sentence
}

public static class InputDetective
{
    public static InputType Detect(string input)
    {
        input = input.Trim();

        if (input.Contains(" ") || input.Any(char.IsPunctuation))
            return InputType.Sentence;

        return InputType.Word;
    }
}