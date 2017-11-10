namespace Ombi.Models
{
    public class CreateUserWizardModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UsePlexAdminAccount { get; set; }
    }
}