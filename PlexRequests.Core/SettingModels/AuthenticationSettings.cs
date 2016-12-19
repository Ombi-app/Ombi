#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: AuthenticationSettings.cs
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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ombi.Core.SettingModels
{
    public sealed class AuthenticationSettings : Settings
    {
        public bool UserAuthentication { get; set; }
        public bool UsePassword { get; set; }

        [JsonProperty("PlexAuthToken")]
        [Obsolete("This should be migrated over into the Plex Settings and then removed in the next release")]
        public string OldPlexAuthToken { get; set; }


        /// <summary>
        /// A comma separated list of users.
        /// </summary>
        public string DeniedUsers { get; set; }

        [JsonIgnore]
        public List<string> DeniedUserList
        {
            get
            {
                var users = new List<string>();
                if (string.IsNullOrEmpty(DeniedUsers))
                {
                    return users;
                }

                var splitUsers = DeniedUsers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var user in splitUsers)
                {
                    if (!string.IsNullOrWhiteSpace(user))
                        users.Add(user.Trim());
                }
                return users;
            }
        }
    }
}
