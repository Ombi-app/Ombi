using System;

namespace PlexRequests.UI
{
	public class UserManagementUsersViewModel
	{
		public string Username{get;set;}
		public string Claims{get;set;}
		public int Id {get;set;}
		public string Alias {get;set;}
		public UserType Type { get; set;}
	}

	public enum UserType
	{
		PlexUser,
		LocalUser
	}
}

