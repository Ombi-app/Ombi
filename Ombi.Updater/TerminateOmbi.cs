#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: TerminateOmbi.cs
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
using NLog;
using Ombi.Common;
using Ombi.Common.EnvironmentInfo;
using Ombi.Common.Processes;
using IServiceProvider = Ombi.Common.IServiceProvider;

namespace Ombi.Updater
{
    public interface ITerminateOmbi
    {
        void Terminate(int processId);
    }

    public class TerminateOmbi : ITerminateOmbi
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public TerminateOmbi(IServiceProvider serviceProvider, IProcessProvider processProvider)
        {
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
        }

        public void Terminate(int processId)
        {
            if (OsInfo.IsWindows)
            {
                _logger.Info("Stopping all running services");

                if (_serviceProvider.ServiceExist(ServiceProvider.OmbiServiceName))
                {
                    try
                    {
                        _logger.Info("Ombi Service is installed and running");
                        _serviceProvider.Stop(ServiceProvider.OmbiServiceName);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "couldn't stop service");
                    }
                }

                _logger.Info("Killing all running processes");

                _processProvider.KillAll(ProcessProvider.OmbiProcessName);
            }
            else
            {
                _logger.Info("Killing all running processes");

                _processProvider.KillAll(ProcessProvider.OmbiProcessName);

                _processProvider.Kill(processId);
            }
        }
    }
}