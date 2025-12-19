namespace LexiScan.Core.Utils
{
    public static class VoiceMapper
    {
        public static string Map(string accent)
        {
            return accent switch
            {
                "en-GB" => "Microsoft Hazel Desktop", // Giọng Anh - Anh
                "en-US" => "Microsoft Zira Desktop",  // Giọng Anh - Mỹ
                _ => null
            };
        }
    }
}