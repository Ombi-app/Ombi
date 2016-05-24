using System;

namespace PlexRequests.Helpers
{
	public class UserClaims
	{
		public const string Admin = "Admin"; // Can do everything including creating new users and editing settings
		public const string PowerUser = "PowerUser"; // Can only manage the requests, approve etc.
		public const string User = "User"; // Can only request
	}
}

