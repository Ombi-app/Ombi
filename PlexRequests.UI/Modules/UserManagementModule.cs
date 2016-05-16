using System;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Core;
using PlexRequests.UI.Models;
using PlexRequests.UI.Modules;
using PlexRequests.Helpers;
using System.Collections.Generic;


namespace PlexRequests.UI
{
	public class UserManagementModule : BaseModule
	{
		public UserManagementModule () : base("usermanagement")
		{
			this.RequiresClaims (UserClaims.Admin);
			Get["/"] = x => Load();

			Get ["/users"] = x => LoadUsers ();
		}

		public Negotiator Load()
		{
			return View ["Index"];
		}

		public Response LoadUsers()
		{
			var users = UserMapper.GetUsers ();
			var model = new List<UserManagementUsersViewModel>();
			foreach (var user in users) {
				model.Add (new UserManagementUsersViewModel {
				//Claims = ByteConverterHelper.ReturnObject<string[]>(user.Claims),
					Claims = "test",
					Id = user.Id,
					Username = user.UserName,
					//Type = UserType.LocalUser
				});
			}
			return Response.AsJson (users);
		}
	}
}

