#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ConfigurationModule.cs
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
using Mono.Data.Sqlite;

using Nancy;
using Nancy.Authentication.Forms;

using Ninject.Modules;

using PlexRequests.Core;
using PlexRequests.Core.Migration;
using PlexRequests.Helpers;
using PlexRequests.Services.Interfaces;
using PlexRequests.Services.Notification;
using PlexRequests.Store;

namespace PlexRequests.UI.NinjectModules
{
    public class ConfigurationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICacheProvider>().To<MemoryCacheProvider>().InSingletonScope();
            Bind<ISqliteConfiguration>().To<DbConfiguration>().WithConstructorArgument("provider", new SqliteFactory());
            Bind<IPlexDatabase>().To<PlexDatabase>().WithConstructorArgument("provider", new SqliteFactory());
            Bind<IPlexReadOnlyDatabase>().To<PlexReadOnlyDatabase>();
            Bind<IMigrationRunner>().To<MigrationRunner>();


            Bind<IUserMapper>().To<UserMapper>();
            Bind<ICustomUserMapper>().To<UserMapper>();

            Bind<INotificationService>().To<NotificationService>().InSingletonScope();
            Bind<INotificationEngine>().To<NotificationEngine>();
        }
    }
}