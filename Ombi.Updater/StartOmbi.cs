#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: StartOmbi.cs
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
using System.IO;
using NLog;
using Ombi.Common;
using Ombi.Common.Processes;
using IServiceProvider = Ombi.Common.IServiceProvider;

namespace Ombi.Updater
{
    public interface IStartOmbi
    {
        void Start(AppType appType, string installationFolder, string args);
        void Start(AppType app, string installationFolder);

    }
    public class StartOmbi : IStartOmbi
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public StartOmbi(IServiceProvider serviceProvider, IProcessProvider processProvider)
        {
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
        }

        public void Start(AppType app, string installationFolder)
        {
            Start(app, installationFolder, string.Empty);
        }

        public void Start(AppType appType, string installationFolder, string args)
        {
            _logger.Info("Starting Ombi");
            if (appType == AppType.Service)
            {
                try
                {
                    StartService();

                }
                catch (InvalidOperationException e)
                {
                    _logger.Warn(e, "Couldn't start Ombi Service (Most likely due to permission issues). falling back to console.");
                    StartConsole(installationFolder, args);
                }
            }
            else if (appType == AppType.Console)
            {
                StartConsole(installationFolder, args);
            }
        }

        private void StartService()
        {
            _logger.Info("Starting Ombi service");
            _serviceProvider.Start(ServiceProvider.OmbiServiceName);
        }

        private void StartConsole(string installationFolder, string args)
        {
            Start(installationFolder, "Ombi.exe", args);
        }

        private void Start(string installationFolder, string fileName, string args)
        {
            _logger.Info("Starting {0}", fileName);
            var path = Path.Combine(installationFolder, fileName);
            _processProvider.SpawnNewProcess(path);
        }
    }
}