#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: Program.cs
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

using Microsoft.Owin.Hosting;

using Mono.Data.Sqlite;
using Mono.Unix;
using Mono.Unix.Native;

using NLog;
using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Repository;
using System.Diagnostics;

using FluentScheduler;

using PlexRequests.Services;
using PlexRequests.UI.Jobs;

namespace PlexRequests.UI
{
    class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            var baseUrl = string.Empty;
            var port = -1;
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i].ToLowerInvariant().Substring(1);
                    switch (arg)
                    {
                        case "base":
                            i++;
                            var value = args[i];
                            Console.WriteLine($"Using a Base URL {args[i]}");
                            baseUrl = value;
                            break;
                        default:
                            int portResult;
                            if (!int.TryParse(args[i], out portResult))
                            {
                                Console.WriteLine("Didn't pass in a valid port");
                                Console.ReadLine();
                                Environment.Exit(1);
                            }
                            else
                            {
                                port = portResult;
                            }
                            break;
                    }
                }
            }
            Log.Trace("Getting product version");
            WriteOutVersion();

            var s = new Setup();
            var cn = s.SetupDb(baseUrl);
            s.CacheQualityProfiles();
            ConfigureTargets(cn);

            if (port == -1)
                port = GetStartupPort();

            var options = new StartOptions(Debugger.IsAttached ? $"http://localhost:{port}" : $"http://+:{port}")
            {
                ServerFactory = "Microsoft.Owin.Host.HttpListener"
            };
            try
            {
                using (WebApp.Start<Startup>(options))
                {
                    SetupSchedulers();

                    Console.WriteLine($"Request Plex is running on the following: http://+:{port}/");

                    if (Type.GetType("Mono.Runtime") != null)
                    {
                        Log.Trace("We are on Mono!");
                        // on mono, processes will usually run as daemons - this allows you to listen
                        // for termination signals (ctrl+c, shutdown, etc) and finalize correctly
                        UnixSignal.WaitAny(
                            new[] { new UnixSignal(Signum.SIGINT), new UnixSignal(Signum.SIGTERM), new UnixSignal(Signum.SIGQUIT), new UnixSignal(Signum.SIGHUP) });
                    }
                    else
                    {
                        Log.Trace("This is not Mono");
                        Console.WriteLine("Press any key to exit");
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                throw;
            }
        }

        private static void WriteOutVersion()
        {
            var assemblyVer = AssemblyHelper.GetProductVersion();
            Log.Info($"Version: {assemblyVer}");
            Console.WriteLine($"Version: {assemblyVer}");
        }

        private static int GetStartupPort()
        {
            Log.Trace("Getting startup Port");
            var port = 3579;
            var service = new SettingsServiceV2<PlexRequestSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var settings = service.GetSettings();
            Log.Trace("Port: {0}", settings.Port);
            if (settings.Port != 0)
            {
                port = settings.Port;
            }

            return port;
        }

        private static void ConfigureTargets(string connectionString)
        {
            LoggingHelper.ConfigureLogging(connectionString);
        }

        private static void SetupSchedulers()
        {
            TaskManager.TaskFactory = new PlexTaskFactory();
            TaskManager.Initialize(new PlexRegistry());
            TaskManager.Initialize(new MediaCacheRegistry());
        }
    }
}
