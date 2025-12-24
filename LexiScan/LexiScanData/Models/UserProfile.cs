namespace LexiScanData.Models
{
    public class UserProfile
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsPremium { get; set; } = false;
    }
}