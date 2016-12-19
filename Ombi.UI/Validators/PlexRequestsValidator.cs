#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: PlexRequestsValidator.cs
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
using FluentValidation;
using Ombi.Core.SettingModels;

namespace Ombi.UI.Validators
{
    public class PlexRequestsValidator : AbstractValidator<PlexRequestSettings>
    {
        public PlexRequestsValidator()
        {
            RuleFor(x => x.BaseUrl).NotEqual("requests",StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'requests' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("admin", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'admin' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("search", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'search' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("issues", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'issues' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("userlogin", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'userlogin' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("login", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'login' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("test", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'test' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("approval", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'approval' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("layout", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'layout' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("usermanagement", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'usermanagement' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("api", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'api' as this is reserved by the application.");
            RuleFor(x => x.BaseUrl).NotEqual("landing", StringComparer.CurrentCultureIgnoreCase).WithMessage("You cannot use 'landing' as this is reserved by the application.");
        }
    }
}