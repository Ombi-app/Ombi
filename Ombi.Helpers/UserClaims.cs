namespace Ombi.Helpers
{
	public class UserClaims
	{
		public const string Admin = nameof(Admin); // Can do everything including creating new users and editing settings
		public const string PowerUser = nameof(PowerUser); // Can only manage the requests, approve etc.
		public const string RegularUser = nameof(RegularUser); // Can only request
	    public const string ReadOnlyUser = nameof(ReadOnlyUser); // Can only view stuff
		public const string Newsletter = nameof(Newsletter); // Has newsletter feature enabled
	}
}

