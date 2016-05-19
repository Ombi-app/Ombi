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

using CommandLine;

using PlexRequests.UI.Start;

namespace PlexRequests.UI
{
    class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {

            var result = Parser.Default.ParseArguments<StartupOptions>(args);
            var baseUrl = result.MapResult(
                o => o.BaseUrl,
                e => string.Empty);

            var port = result.MapResult(
                x => x.Port,
                e => -1);

            var updated = result.MapResult(x => x.Updated, e => false);
            //TODO
            
            PrintToConsole("Starting Up! Please wait, this can usually take a few seconds.", ConsoleColor.Yellow);
            
            Log.Trace("Getting product version");
            WriteOutVersion();

            var s = new Setup();
            var cn = s.SetupDb(baseUrl);
            s.CacheQualityProfiles();
            ConfigureTargets(cn);
            SetupLogging();

            if (port == -1 || port == 3579)
                port = GetStartupPort();

            var options = new StartOptions(Debugger.IsAttached ? $"http://localhost:{port}" : $"http://+:{port}")
            {
                ServerFactory = "Microsoft.Owin.Host.HttpListener"
            };
            try
            {
                using (WebApp.Start<Startup>(options))
                {
                    Console.WriteLine($"Plex Requests is running on the following: http://+:{port}/{baseUrl}");

                    PrintToConsole("All setup, Plex Requests is now ready!", ConsoleColor.Yellow);
                    if (Type.GetType("Mono.Runtime") != null)
                    {
                        Log.Info("We are on Mono!");

                        // on mono, processes will usually run as daemons - this allows you to listen
                        // for termination signals (ctrl+c, shutdown, etc) and finalize correctly
                        UnixSignal.WaitAny(
                            new[] { new UnixSignal(Signum.SIGINT), new UnixSignal(Signum.SIGTERM), new UnixSignal(Signum.SIGQUIT) });
                    }
                    else
                    {
                        Log.Info("This is not Mono");
                        Console.WriteLine("Press any key to exit");
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                Console.WriteLine(e);
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

        private static void SetupLogging()
        {
            var settingsService = new SettingsServiceV2<LogSettings>(new SettingsJsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var logSettings = settingsService.GetSettings();

            if (logSettings != null)
            {
                LoggingHelper.ReconfigureLogLevel(LogLevel.FromOrdinal(logSettings.Level));
            }
        }

        private static void PrintToConsole(string message, ConsoleColor colour = ConsoleColor.Gray)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
