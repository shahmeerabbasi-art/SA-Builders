namespace SA_Builders.Models
{
    public class ActivationToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}