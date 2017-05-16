#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomAuthenticationConfiguration.cs
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

using Nancy.Cryptography;
using Ombi.Store.Models.Emby;
using Ombi.Store.Models.Plex;
using Ombi.Store.Repository;

namespace Ombi.UI.Authentication
{
    public class CustomAuthenticationConfiguration
    {
        internal const string DefaultRedirectQuerystringKey = "returnUrl";

        /// <summary>
        /// Gets or sets the forms authentication query string key for storing the return url
        /// </summary>
        public string RedirectQuerystringKey { get; set; }

        /// <summary>
        /// Gets or sets the redirect url for pages that require authentication
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>Gets or sets the username/identifier mapper</summary>
        public IUserRepository LocalUserRepository { get; set; }

        public IExternalUserRepository<PlexUsers> PlexUserRepository { get; set; }
        public IExternalUserRepository<EmbyUsers> EmbyUserRepository { get; set; }

        /// <summary>Gets or sets RequiresSSL property</summary>
        /// <value>The flag that indicates whether SSL is required</value>
        public bool RequiresSSL { get; set; }

        /// <summary>
        /// Gets or sets whether to redirect to login page during unauthorized access.
        /// </summary>
        public bool DisableRedirect { get; set; }

        /// <summary>Gets or sets the domain of the auth cookie</summary>
        public string Domain { get; set; }

        /// <summary>Gets or sets the path of the auth cookie</summary>
        public string Path { get; set; }

        /// <summary>Gets or sets the cryptography configuration</summary>
        public CryptographyConfiguration CryptographyConfiguration { get; set; }

        /// <summary>
        /// Gets a value indicating whether the configuration is valid or not.
        /// </summary>
        public virtual bool IsValid => (this.DisableRedirect || !string.IsNullOrEmpty(this.RedirectUrl)) && (this.LocalUserRepository != null && PlexUserRepository != null && this.CryptographyConfiguration != null) && (this.CryptographyConfiguration.EncryptionProvider != null && this.CryptographyConfiguration.HmacProvider != null);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.Authentication.Forms.FormsAuthenticationConfiguration" /> class.
        /// </summary>
        public CustomAuthenticationConfiguration()
      : this(CryptographyConfiguration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Nancy.Authentication.Forms.FormsAuthenticationConfiguration" /> class.
        /// </summary>
        /// <param name="cryptographyConfiguration">Cryptography configuration</param>
        public CustomAuthenticationConfiguration(CryptographyConfiguration cryptographyConfiguration)
        {
            this.CryptographyConfiguration = cryptographyConfiguration;
            this.RedirectQuerystringKey = "returnUrl";
        }
    }
}
