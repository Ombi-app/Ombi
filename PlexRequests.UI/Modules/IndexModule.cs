#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: IndexModule.cs
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
using System.Threading.Tasks;

using Nancy;
using Nancy.Extensions;
using Nancy.Linker;
using Nancy.Responses;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
    public class IndexModule : BaseAuthModule
    {
        public IndexModule(ISettingsService<PlexRequestSettings> pr, ISettingsService<LandingPageSettings> l, IResourceLinker rl) : base(pr)
        {
            LandingPage = l;
            Linker = rl;
            Get["Index", "/", true] = async (x, ct) => await Index();

            Get["/Index", true] = async (x, ct) => await Index();
        }
        private ISettingsService<LandingPageSettings> LandingPage { get; }
        private IResourceLinker Linker { get; }

        public async Task<RedirectResponse> Index()
        {
            var settings = await LandingPage.GetSettingsAsync();
            if (settings.Enabled)
            {
                if (settings.BeforeLogin) // Before login
                {
                    if (!string.IsNullOrEmpty(Username))
                    {
                        // They are not logged in
                        return Context.GetRedirect(Linker.BuildAbsoluteUri(Context, "LandingPageIndex").ToString());
                    }
                    return Context.GetRedirect(Linker.BuildAbsoluteUri(Context, "SearchIndex").ToString());
                }

                // After login
                if (string.IsNullOrEmpty(Username))
                {
                    // Not logged in yet
                    return Context.GetRedirect(Linker.BuildAbsoluteUri(Context, "UserLoginIndex").ToString());
                }
                // Send them to landing
                var landingUrl = Linker.BuildAbsoluteUri(Context, "LandingPageIndex").ToString();
                return Context.GetRedirect(landingUrl);
            }

            return Context.GetRedirect(Linker.BuildAbsoluteUri(Context, "UserLoginIndex").ToString());
        }
    }
}