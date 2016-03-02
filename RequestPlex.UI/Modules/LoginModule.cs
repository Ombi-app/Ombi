using System;
using System.Dynamic;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;

using RequestPlex.Core;

namespace RequestPlex.UI.Modules
{
    public class LoginModule : NancyModule
    {
        public LoginModule()
        {
            Get["/login"] = _ =>
            {
                {
                    dynamic model = new ExpandoObject();
                    model.Errored = Request.Query.error.HasValue;

                    return View["Login/Index", model];
                }
                
            };

            Get["/logout"] = x => this.LogoutAndRedirect("~/");

            Post["/login"] = x =>
            {
                var userId = UserMapper.ValidateUser((string)Request.Form.Username, (string)Request.Form.Password);

                if (userId == null)
                {
                    return Context.GetRedirect("~/login?error=true&username=" + (string)Request.Form.Username);
                }
                DateTime? expiry = null;
                if (Request.Form.RememberMe.HasValue)
                {
                    expiry = DateTime.Now.AddDays(7);
                }
                return this.LoginAndRedirect(userId.Value, expiry);
            };

            Get["/register"] = x => {
                {
                    dynamic model = new ExpandoObject();
                    model.Errored = Request.Query.error.HasValue;

                    return View["Login/Register", model];
                }

            };

            Post["/register"] = x =>
            {
                var exists = UserMapper.DoUsersExist();
                if (exists)
                {
                    return Context.GetRedirect("~/register?error=true&username=" + (string)Request.Form.Username);
                }
                var userId = UserMapper.CreateUser(Request.Form.Username, Request.Form.Password);
                return this.LoginAndRedirect((Guid)userId);
            };
        }
    }
}