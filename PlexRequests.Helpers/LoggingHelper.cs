#region Copyright
// /************************************************************************
//    Copyright (c) 2016 Jamie Rees
//    File: LoggingHelper.cs
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

using Newtonsoft.Json;

using NLog;
using NLog.Config;
using NLog.Targets;

namespace PlexRequests.Helpers
{
    public static class LoggingHelper
    {
        public static string DumpJson(this object value)
        {
            var dumpTarget = value;
            //if this is a string that contains a JSON object, do a round-trip serialization to format it:
            var stringValue = value as string;
            if (stringValue != null)
            {
                if (stringValue.Trim().StartsWith("{", StringComparison.Ordinal))
                {
                    var obj = JsonConvert.DeserializeObject(stringValue);
                    dumpTarget = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                else
                {
                    dumpTarget = stringValue;
                }
            }
            else
            {
                dumpTarget = JsonConvert.SerializeObject(value, Formatting.Indented);
            }
            return dumpTarget.ToString();
        }

        public static void ConfigureLogging(string connectionString)
        {
            LogManager.ThrowExceptions = true;
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var databaseTarget = new DatabaseTarget
            {
                CommandType = CommandType.Text,
                ConnectionString = connectionString,
                DBProvider = "Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756",
                Name = "database"
            };
            
            var messageParam = new DatabaseParameterInfo { Name = "@Message", Layout = "${message}" };
            var callsiteParam = new DatabaseParameterInfo { Name = "@Callsite", Layout = "${callsite}" };
            var levelParam = new DatabaseParameterInfo { Name = "@Level", Layout = "${level}" };
            var dateParam = new DatabaseParameterInfo { Name = "@Date", Layout = "${date}" };
            var loggerParam = new DatabaseParameterInfo { Name = "@Logger", Layout = "${logger}" };
            var exceptionParam = new DatabaseParameterInfo { Name = "@Exception", Layout = "${exception:tostring}" };

            databaseTarget.Parameters.Add(messageParam);
            databaseTarget.Parameters.Add(callsiteParam);
            databaseTarget.Parameters.Add(levelParam);
            databaseTarget.Parameters.Add(dateParam);
            databaseTarget.Parameters.Add(loggerParam);
            databaseTarget.Parameters.Add(exceptionParam);

            databaseTarget.CommandText = "INSERT INTO Logs (Date,Level,Logger, Message, Callsite, Exception) VALUES(@Date,@Level,@Logger, @Message, @Callsite, @Exception);";
            config.AddTarget("database", databaseTarget);

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Info, databaseTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        public static void ReconfigureLogLevel(LogLevel level)
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevel(level);
            }

            //Call to update existing Loggers created with GetLogger() or 
            //GetCurrentClassLogger()
            LogManager.ReconfigExistingLoggers();
        }
    }
}
