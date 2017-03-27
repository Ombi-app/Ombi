﻿#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: ApiModule.cs
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
using Ombi.Api;
using Ombi.Api.Interfaces;

namespace Ombi.UI.NinjectModules
{
    public class ApiModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICouchPotatoApi>().To<CouchPotatoApi>();
            Bind<IPushbulletApi>().To<PushbulletApi>();
            Bind<IPushoverApi>().To<PushoverApi>();
            Bind<ISickRageApi>().To<SickrageApi>();
            Bind<ISonarrApi>().To<SonarrApi>();
            Bind<IPlexApi>().To<PlexApi>();
            Bind<IMusicBrainzApi>().To<MusicBrainzApi>();
            Bind<IHeadphonesApi>().To<HeadphonesApi>();
            Bind<ISlackApi>().To<SlackApi>();
            Bind<IApiRequest>().To<ApiRequest>();
            Bind<IWatcherApi>().To<WatcherApi>();
            Bind<INetflixApi>().To<NetflixRouletteApi>();
            Bind<IDiscordApi>().To<DiscordApi>();
            Bind<IRadarrApi>().To<RadarrApi>();
            Bind<ITraktApi>().To<TraktApi>();
            Bind<IEmbyApi>().To<EmbyApi>();
            Bind<IAppveyorApi>().To<AppveyorApi>();
        }
    }
}