﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: CustomAuthenticationProvider.cs
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
using System.Linq;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Cookies;
using Nancy.Cryptography;
using Nancy.Extensions;
using Nancy.Helpers;
using Nancy.Security;
using Ombi.Core;

namespace Ombi.UI.Authentication
{
    public class CustomAuthenticationProvider
    {
        private static string formsAuthenticationCookieName = "_ncfa";
        private static CustomAuthenticationConfiguration currentConfiguration;

        /// <summary>Gets or sets the forms authentication cookie name</summary>
        public static string FormsAuthenticationCookieName
        {
            get
            {
                return CustomAuthenticationProvider.formsAuthenticationCookieName;
            }
            set
            {
                CustomAuthenticationProvider.formsAuthenticationCookieName = value;
            }
        }

        /// <summary>Enables forms authentication for the application</summary>
        /// <param name="pipelines">Pipelines to add handlers to (usually "this")</param>
        /// <param name="configuration">Forms authentication configuration</param>
        public static void Enable(IPipelines pipelines, CustomAuthenticationConfiguration configuration)
        {
            if (pipelines == null)
                throw new ArgumentNullException("pipelines");
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            if (!configuration.IsValid)
                throw new ArgumentException("Configuration is invalid", "configuration");
            CustomAuthenticationProvider.currentConfiguration = configuration;
            pipelines.BeforeRequest.AddItemToStartOfPipeline(CustomAuthenticationProvider.GetLoadAuthenticationHook(configuration));
            if (configuration.DisableRedirect)
                return;
            pipelines.AfterRequest.AddItemToEndOfPipeline(CustomAuthenticationProvider.GetRedirectToLoginHook(configuration));
        }

        /// <summary>Enables forms authentication for a module</summary>
        /// <param name="module">Module to add handlers to (usually "this")</param>
        /// <param name="configuration">Forms authentication configuration</param>
        public static void Enable(INancyModule module, CustomAuthenticationConfiguration configuration)
        {
            if (module == null)
                throw new ArgumentNullException("module");
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            if (!configuration.IsValid)
                throw new ArgumentException("Configuration is invalid", "configuration");
            module.RequiresAuthentication();
            CustomAuthenticationProvider.currentConfiguration = configuration;
            module.Before.AddItemToStartOfPipeline(CustomAuthenticationProvider.GetLoadAuthenticationHook(configuration));
            if (configuration.DisableRedirect)
                return;
            module.After.AddItemToEndOfPipeline(CustomAuthenticationProvider.GetRedirectToLoginHook(configuration));
        }
        /// <summary>
        /// Creates a response that sets the authentication cookie and redirects
        /// the user back to where they came from.
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="userIdentifier">User identifier guid</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <param name="fallbackRedirectUrl">Url to redirect to if none in the querystring</param>
        /// <returns>Nancy response with redirect.</returns>
        public static Response UserLoggedInRedirectResponse(NancyContext context, Guid userIdentifier, DateTime? cookieExpiry = null, string fallbackRedirectUrl = null)
        {
            var redirectUrl = fallbackRedirectUrl;

            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = context.Request.Url.BasePath;
            }

            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = "/";
            }

            string redirectQuerystringKey = GetRedirectQuerystringKey(currentConfiguration);

            if (context.Request.Query[redirectQuerystringKey].HasValue)
            {
                var queryUrl = (string)context.Request.Query[redirectQuerystringKey];

                if (context.IsLocalUrl(queryUrl))
                {
                    redirectUrl = queryUrl;
                }
            }

            var response = context.GetRedirect(redirectUrl);
            var authenticationCookie = BuildCookie(userIdentifier, cookieExpiry, currentConfiguration);
            response.WithCookie(authenticationCookie);

            return response;
        }
        /// <summary>
        /// Logs the user in.
        /// </summary>
        /// <param name="userIdentifier">User identifier guid</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <returns>Nancy response with status <see cref="HttpStatusCode.OK"/></returns>
        public static Response UserLoggedInResponse(Guid userIdentifier, DateTime? cookieExpiry = null)
        {
            var response =
                (Response)HttpStatusCode.OK;

            var authenticationCookie =
                BuildCookie(userIdentifier, cookieExpiry, currentConfiguration);

            response.WithCookie(authenticationCookie);

            return response;
        }

        /// <summary>
        /// Logs the user out and redirects them to a URL
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="redirectUrl">URL to redirect to</param>
        /// <returns>Nancy response</returns>
        public static Response LogOutAndRedirectResponse(NancyContext context, string redirectUrl)
        {
            var response = context.GetRedirect(redirectUrl);
            var authenticationCookie = BuildLogoutCookie(currentConfiguration);
            response.WithCookie(authenticationCookie);

            return response;
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        /// <returns>Nancy response</returns>
        public static Response LogOutResponse()
        {
            var response =
                (Response)HttpStatusCode.OK;

            var authenticationCookie =
                BuildLogoutCookie(currentConfiguration);

            response.WithCookie(authenticationCookie);

            return response;
        }

        /// <summary>
        /// Gets the pre request hook for loading the authenticated user's details
        /// from the cookie.
        /// </summary>
        /// <param name="configuration">Forms authentication configuration to use</param>
        /// <returns>Pre request hook delegate</returns>
        private static Func<NancyContext, Response> GetLoadAuthenticationHook(CustomAuthenticationConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return context =>
            {
                var userGuid = GetAuthenticatedUserFromCookie(context, configuration);

                if (userGuid != Guid.Empty)
                {
                    var identity = new UserIdentity();

                    var plexUsers = configuration.PlexUserRepository.GetAll();
                    var plexUser = plexUsers.FirstOrDefault(x => Guid.Parse(x.LoginId) == userGuid);
                    var embyUsers = configuration.EmbyUserRepository.GetAll();
                    var embyUser = embyUsers.FirstOrDefault(x => Guid.Parse(x.LoginId) == userGuid);

                    if (plexUser != null)
                    {
                        identity.UserName = plexUser.Username;
                    }
                    if (embyUser != null)
                    {
                        identity.UserName = embyUser.Username;
                    }

                    var localUsers = configuration.LocalUserRepository.GetAll();

                    var localUser = localUsers.FirstOrDefault(x => Guid.Parse(x.UserGuid) == userGuid);
                    if (localUser != null)
                    {
                        identity.UserName = localUser.UserName;
                    }

                    context.CurrentUser = identity;
                }

                return null;
            };
        }

        /// <summary>
        /// Gets the post request hook for redirecting to the login page
        /// </summary>
        /// <param name="configuration">Forms authentication configuration to use</param>
        /// <returns>Post request hook delegate</returns>
        private static Action<NancyContext> GetRedirectToLoginHook(CustomAuthenticationConfiguration configuration)
        {
            return context =>
            {
                if (context.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string redirectQuerystringKey = GetRedirectQuerystringKey(configuration);

                    context.Response = context.GetRedirect(
                        string.Format("{0}?{1}={2}",
                        configuration.RedirectUrl,
                        redirectQuerystringKey,
                        context.ToFullPath("~" + context.Request.Path + HttpUtility.UrlEncode(context.Request.Url.Query))));
                }
            };
        }

        /// <summary>
        /// Gets the authenticated user GUID from the incoming request cookie if it exists
        /// and is valid.
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="configuration">Current configuration</param>
        /// <returns>Returns user guid, or Guid.Empty if not present or invalid</returns>
        private static Guid GetAuthenticatedUserFromCookie(NancyContext context, CustomAuthenticationConfiguration configuration)
        {
            if (!context.Request.Cookies.ContainsKey(formsAuthenticationCookieName))
            {
                return Guid.Empty;
            }

            var cookieValueEncrypted = context.Request.Cookies[formsAuthenticationCookieName];

            if (string.IsNullOrEmpty(cookieValueEncrypted))
            {
                return Guid.Empty;
            }

            var cookieValue = DecryptAndValidateAuthenticationCookie(cookieValueEncrypted, configuration);

            Guid returnGuid;
            if (string.IsNullOrEmpty(cookieValue) || !Guid.TryParse(cookieValue, out returnGuid))
            {
                return Guid.Empty;
            }

            return returnGuid;
        }

        /// <summary>
        /// Build the forms authentication cookie
        /// </summary>
        /// <param name="userIdentifier">Authenticated user identifier</param>
        /// <param name="cookieExpiry">Optional expiry date for the cookie (for 'Remember me')</param>
        /// <param name="configuration">Current configuration</param>
        /// <returns>Nancy cookie instance</returns>
        private static INancyCookie BuildCookie(Guid userIdentifier, DateTime? cookieExpiry, CustomAuthenticationConfiguration configuration)
        {
            var cookieContents = EncryptAndSignCookie(userIdentifier.ToString(), configuration);

            var cookie = new NancyCookie(formsAuthenticationCookieName, cookieContents, true, configuration.RequiresSSL, cookieExpiry);

            if (!string.IsNullOrEmpty(configuration.Domain))
            {
                cookie.Domain = configuration.Domain;
            }

            if (!string.IsNullOrEmpty(configuration.Path))
            {
                cookie.Path = configuration.Path;
            }

            return cookie;
        }

        /// <summary>
        /// Builds a cookie for logging a user out
        /// </summary>
        /// <param name="configuration">Current configuration</param>
        /// <returns>Nancy cookie instance</returns>
        private static INancyCookie BuildLogoutCookie(CustomAuthenticationConfiguration configuration)
        {
            var cookie = new NancyCookie(formsAuthenticationCookieName, String.Empty, true, configuration.RequiresSSL, DateTime.Now.AddDays(-1));

            if (!string.IsNullOrEmpty(configuration.Domain))
            {
                cookie.Domain = configuration.Domain;
            }

            if (!string.IsNullOrEmpty(configuration.Path))
            {
                cookie.Path = configuration.Path;
            }

            return cookie;
        }

        /// <summary>
        /// Encrypt and sign the cookie contents
        /// </summary>
        /// <param name="cookieValue">Plain text cookie value</param>
        /// <param name="configuration">Current configuration</param>
        /// <returns>Encrypted and signed string</returns>
        private static string EncryptAndSignCookie(string cookieValue, CustomAuthenticationConfiguration configuration)
        {
            var encryptedCookie = configuration.CryptographyConfiguration.EncryptionProvider.Encrypt(cookieValue);
            var hmacBytes = GenerateHmac(encryptedCookie, configuration);
            var hmacString = Convert.ToBase64String(hmacBytes);

            return String.Format("{1}{0}", encryptedCookie, hmacString);
        }

        /// <summary>
        /// Generate a hmac for the encrypted cookie string
        /// </summary>
        /// <param name="encryptedCookie">Encrypted cookie string</param>
        /// <param name="configuration">Current configuration</param>
        /// <returns>Hmac byte array</returns>
        private static byte[] GenerateHmac(string encryptedCookie, CustomAuthenticationConfiguration configuration)
        {
            return configuration.CryptographyConfiguration.HmacProvider.GenerateHmac(encryptedCookie);
        }

        /// <summary>
        /// Decrypt and validate an encrypted and signed cookie value
        /// </summary>
        /// <param name="cookieValue">Encrypted and signed cookie value</param>
        /// <param name="configuration">Current configuration</param>
        /// <returns>Decrypted value, or empty on error or if failed validation</returns>
        public static string DecryptAndValidateAuthenticationCookie(string cookieValue, CustomAuthenticationConfiguration configuration)
        {
            var hmacStringLength = Base64Helpers.GetBase64Length(configuration.CryptographyConfiguration.HmacProvider.HmacLength);

            var encryptedCookie = cookieValue.Substring(hmacStringLength);
            var hmacString = cookieValue.Substring(0, hmacStringLength);

            var encryptionProvider = configuration.CryptographyConfiguration.EncryptionProvider;

            // Check the hmacs, but don't early exit if they don't match
            var hmacBytes = Convert.FromBase64String(hmacString);
            var newHmac = GenerateHmac(encryptedCookie, configuration);
            var hmacValid = HmacComparer.Compare(newHmac, hmacBytes, configuration.CryptographyConfiguration.HmacProvider.HmacLength);

            var decrypted = encryptionProvider.Decrypt(encryptedCookie);

            // Only return the decrypted result if the hmac was ok
            return hmacValid ? decrypted : string.Empty;
        }

        /// <summary>
        /// Gets the redirect query string key from <see cref="FormsAuthenticationConfiguration"/>
        /// </summary>
        /// <param name="configuration">The forms authentication configuration.</param>
        /// <returns>Redirect Querystring key</returns>
        private static string GetRedirectQuerystringKey(CustomAuthenticationConfiguration configuration)
        {
            string redirectQuerystringKey = null;

            if (configuration != null)
            {
                redirectQuerystringKey = configuration.RedirectQuerystringKey;
            }

            if (string.IsNullOrWhiteSpace(redirectQuerystringKey))
            {
                redirectQuerystringKey = CustomAuthenticationConfiguration.DefaultRedirectQuerystringKey;
            }

            return redirectQuerystringKey;
        }
    }
}
