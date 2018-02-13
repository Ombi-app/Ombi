using Ombi.Core.Models.UI;

namespace Ombi.Models.Identity
{
    public class UpdateLocalUserModel : UserViewModel
    {
        public string CurrentPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}