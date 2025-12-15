namespace LexiScan.Core.Utils
{
    public static class VoiceMapper
    {
        public static string Map(string accent)
        {
            return accent switch
            {
                "UK" => "Microsoft Hazel Desktop",
                "US" => "Microsoft Zira Desktop",
                _ => null
            };
        }
    }
}
