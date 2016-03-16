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
using System.Data;

using Microsoft.Owin.Hosting;

using Mono.Data.Sqlite;

using NLog;
using NLog.Config;
using NLog.Targets;

using PlexRequests.Core;
using PlexRequests.Core.SettingModels;
using PlexRequests.Helpers;
using PlexRequests.Store;
using PlexRequests.Store.Repository;

namespace PlexRequests.UI
{
    class Program
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            var uri = string.Empty;
            if (args.Length > 0)
            {
                Log.Info("We are going to use port {0} that was passed in", args[0]);
                int portResult;
                if (!int.TryParse(args[0], out portResult))
                {
                    Console.WriteLine("Incorrect Port format. Press Any Key to shut down.");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                uri = $"http://localhost:{portResult}";
            }

            Log.Trace("Getting product version");
            WriteOutVersion();
            
            var s = new Setup();
            s.SetupDb();
            
            if(string.IsNullOrEmpty(uri))
                uri = GetStartupUri();
StartOptions options = new StartOptions();
var newPort = GetPort();
    options.Urls.Add($"http://*:{newPort}");

            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine($"Request Plex is running on {uri}");
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }

        private static void WriteOutVersion()
        {
            var assemblyVer = AssemblyHelper.GetProductVersion();
            Log.Info($"Version: {assemblyVer}");
            Console.WriteLine($"Version: {assemblyVer}");
        }

        private static string GetStartupUri()
        {
            Log.Trace("Getting startup URI");
            var uri = "http://localhost:3579/";
            var service = new SettingsServiceV2<PlexRequestSettings>(new JsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var settings = service.GetSettings();
            Log.Trace("Port: {0}", settings.Port);
            if (settings.Port != 0)
            {
                uri = $"http://localhost:{settings.Port}";
            }

            return uri;
        }

private static int GetPort()
{
 
           
            var service = new SettingsServiceV2<PlexRequestSettings>(new JsonRepository(new DbConfiguration(new SqliteFactory()), new MemoryCacheProvider()));
            var settings = service.GetSettings();

            return settings.Port;
}

        private static void ConfigureTargets(string connectionString)
        {
            LogManager.ThrowExceptions = true;
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var databaseTarget = new DatabaseTarget
            {
                CommandType = CommandType.Text,
                ConnectionString = connectionString,
                DBProvider = "Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756",
                Name = "database"
            };


            var messageParam = new DatabaseParameterInfo { Name = "@Message", Layout = "${message}" };
            var callsiteParam = new DatabaseParameterInfo { Name = "@Callsite", Layout = "${callsite}" };
            var levelParam = new DatabaseParameterInfo { Name = "@Level", Layout = "${level}" };
            var usernameParam = new DatabaseParameterInfo { Name = "@Username", Layout = "${identity}" };
            var dateParam = new DatabaseParameterInfo { Name = "@Date", Layout = "${date}" };
            var loggerParam = new DatabaseParameterInfo { Name = "@Logger", Layout = "${logger}" };
            var exceptionParam = new DatabaseParameterInfo { Name = "@Exception", Layout = "${exception:tostring}" };

            databaseTarget.Parameters.Add(messageParam);
            databaseTarget.Parameters.Add(callsiteParam);
            databaseTarget.Parameters.Add(levelParam);
            databaseTarget.Parameters.Add(usernameParam);
            databaseTarget.Parameters.Add(dateParam);
            databaseTarget.Parameters.Add(loggerParam);
            databaseTarget.Parameters.Add(exceptionParam);

            databaseTarget.CommandText = "INSERT INTO Log (Username,Date,Level,Logger, Message, Callsite, Exception) VALUES(@Username,@Date,@Level,@Logger, @Message, @Callsite, @Exception);";
            config.AddTarget("database", databaseTarget);

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Error, databaseTarget);
            config.LoggingRules.Add(rule1);

            try
            {

                // Step 5. Activate the configuration
                LogManager.Configuration = config;
            }
            catch (Exception )
            {

                throw;
            }

            // Example usage
            Logger logger = LogManager.GetLogger("Example");

            logger.Error("error log message");
        }
    }
}
