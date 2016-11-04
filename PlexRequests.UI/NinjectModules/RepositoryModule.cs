#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: RepositoryModule.cs
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
using Ninject.Modules;

using PlexRequests.Core;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Jobs;
using PlexRequests.Store;
using PlexRequests.Store.Repository;

namespace PlexRequests.UI.NinjectModules
{
    public class RepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository<UsersModel>>().To<UserRepository<UsersModel>>();
            Bind(typeof(IRepository<>)).To(typeof(GenericRepository<>));

            Bind<IRequestService>().To<JsonRequestModelRequestService>();
            Bind<IRequestRepository>().To<RequestJsonRepository>();
            Bind<IIssueService>().To<IssueJsonService>();
            Bind<ISettingsRepository>().To<SettingsJsonRepository>();
            Bind<IJobRecord>().To<JobRecord>();

            Bind<IUserRepository>().To<UserRepository>();
        }

    }
}