namespace Api.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string? Salt { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiresAt { get; set; }

        public List<Device>? Devices { get; set; }
    }
}
