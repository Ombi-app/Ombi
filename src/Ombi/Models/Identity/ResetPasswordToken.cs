namespace Ombi.Models.Identity
{
    public class ResetPasswordToken
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}