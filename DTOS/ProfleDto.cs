namespace TestApi.DTOS
{
    public class ProfleDto
    {
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? Website { get; set; }
        public string? Location { get; set; }
        public string? TwitterHandle { get; set; }
        public string? LinkedInHandle { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public string? DisplayName { get; set; }
    }
}
