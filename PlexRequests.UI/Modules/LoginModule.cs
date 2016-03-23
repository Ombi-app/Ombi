#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: LoginModule.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion
using System;
using System.Dynamic;

using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Extensions;
using Nancy.Responses.Negotiation;
using Nancy.Security;

using PlexRequests.Core;
using PlexRequests.UI.Models;

namespace PlexRequests.UI.Modules
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
                    var adminCreated = UserMapper.DoUsersExist();
                    model.AdminExists = adminCreated;
                    return View["Login/Index", model];
                }

            };

            Get["/logout"] = x => this.LogoutAndRedirect("~/");

            Post["/login"] = x =>
            {
                var username = (string)Request.Form.Username;
                var password = (string)Request.Form.Password;

                var userId = UserMapper.ValidateUser(username, password);

                if (userId == null)
                {
                    return Context.GetRedirect("~/login?error=true&username=" + username);
                }
                DateTime? expiry = null;
                if (Request.Form.RememberMe.HasValue)
                {
                    expiry = DateTime.Now.AddDays(7);
                }
                Session[SessionKeys.UsernameKey] = username;
                return this.LoginAndRedirect(userId.Value, expiry);
            };

            Get["/register"] = x =>
            {
                {
                    dynamic model = new ExpandoObject();
                    model.Errored = Request.Query.error.HasValue;

                    return View["Login/Register", model];
                }
            };

            Post["/register"] = x =>
            {
                var username = (string)Request.Form.Username;
                var exists = UserMapper.DoUsersExist();
                if (exists)
                {
                    return Context.GetRedirect("~/register?error=true");
                }
                var userId = UserMapper.CreateUser(username, Request.Form.Password, new[] { "Admin" });
                Session[SessionKeys.UsernameKey] = username;
                return this.LoginAndRedirect((Guid)userId);
            };

            Get["/changepassword"] = _ => ChangePassword();
            Post["/changepassword"] = _ => ChangePasswordPost();
        }

        private Negotiator ChangePassword()
        {
            this.RequiresAuthentication();
            return View["ChangePassword"];
        }

        private Negotiator ChangePasswordPost()
        {
            var username = Context.CurrentUser.UserName;
            var oldPass = Request.Form.OldPassword;
            var newPassword = Request.Form.NewPassword;
            var newPasswordAgain = Request.Form.NewPasswordAgain;
            if (!newPassword.Equals(newPasswordAgain))
            {

            }

            var result = UserMapper.UpdateUser(username, oldPass, newPassword);
            return View["ChangePassword"];
        }
    }
}