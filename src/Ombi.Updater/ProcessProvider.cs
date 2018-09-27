using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ombi.Updater
{
    public class ProcessProvider : IProcessProvider
    {
        public ProcessProvider()
        {
            
        }

        public int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        public ProcessInfo GetCurrentProcess()
        {
            return ConvertToProcessInfo(Process.GetCurrentProcess());
        }
        public bool Exists(int processId)
        {
            return GetProcessById(processId) != null;
        }

        public bool Exists(string processName)
        {
            return GetProcessesByName(processName).Any();
        }
        public ProcessInfo GetProcessById(int id)
        {
            Console.WriteLine("Finding process with Id:{0}", id);

            var processInfo = ConvertToProcessInfo(Process.GetProcesses().FirstOrDefault(p => p.Id == id));

            if (processInfo == null)
            {
                Console.WriteLine("Unable to find process with ID {0}", id);
            }
            else
            {
                Console.WriteLine("Found process {0}", processInfo.ToString());
            }

            return processInfo;
        }

        public List<ProcessInfo> FindProcessByName(string name)
        {
            return GetProcessesByName(name).Select(ConvertToProcessInfo).Where(c => c != null).ToList();
        }
        

        public void WaitForExit(Process process)
        {
            Console.WriteLine("Waiting for process {0} to exit.", process.ProcessName);

            process.WaitForExit();
        }

        public void SetPriority(int processId, ProcessPriorityClass priority)
        {
            var process = Process.GetProcessById(processId);

            Console.WriteLine("Updating [{0}] process priority from {1} to {2}",
                        process.ProcessName,
                        process.PriorityClass,
                        priority);

            process.PriorityClass = priority;
        }

        public bool Kill(StartupOptions opts)
        {
            //if (opts.IsWindowsService)
            //{
            //    Console.WriteLine("Stopping Service {0}", opts.WindowsServiceName);
            //    var process = new Process();
            //    var startInfo =
            //        new ProcessStartInfo
            //        {
            //            WindowStyle = ProcessWindowStyle.Hidden,
            //            FileName = "cmd.exe",
            //            Arguments = $"/C net stop \"{opts.WindowsServiceName}\""
            //        };
            //    process.StartInfo = startInfo;
            //    process.Start();
            //}
            //else
            //{
                var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == opts.ProcessName);

                if (process == null)
                {
                    Console.WriteLine("Cannot find process with name: {0}", opts.ProcessName);
                    return false;
                }
                

                if (process.Id > 0)
                {
                    Console.WriteLine("[{0}]: Killing process", process.Id);
                    process.Kill();
                    Console.WriteLine("[{0}]: Waiting for exit", process.Id);
                    process.WaitForExit();
                    Console.WriteLine("[{0}]: Process terminated successfully", process.Id);

                return true;
                }

            return false;
            //}
        }

        public void KillAll(string processName)
        {
            var processes = GetProcessesByName(processName);

            Console.WriteLine("Found {0} processes to kill", processes.Count);

            foreach (var processInfo in processes)
            {
                if (processInfo.Id == Process.GetCurrentProcess().Id)
                {
                    Console.WriteLine("Tried killing own process, skipping: {0} [{1}]", processInfo.Id, processInfo.ProcessName);
                    continue;
                }

                Console.WriteLine("Killing process: {0} [{1}]", processInfo.Id, processInfo.ProcessName);
                Kill(new StartupOptions{OmbiProcessId = processInfo.Id});
            }
        }


        private ProcessInfo ConvertToProcessInfo(Process process)
        {
            if (process == null) return null;

            process.Refresh();

            ProcessInfo processInfo = null;

            try
            {
                if (process.Id <= 0) return null;

                processInfo = new ProcessInfo
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    StartPath = GetExeFileName(process)
                };

                if (process.Id != Process.GetCurrentProcess().Id && process.HasExited)
                {
                    processInfo = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return processInfo;

        }

        private static string GetExeFileName(Process process)
        {
            return process.MainModule.FileName;
        }

        private List<Process> GetProcessesByName(string name)
        {
            var processes = Process.GetProcessesByName(name).ToList();

            Console.WriteLine("Found {0} processes with the name: {1}", processes.Count, name);

            try
            {
                foreach (var process in processes)
                {
                    Console.WriteLine(" - [{0}] {1}", process.Id, process.ProcessName);
                }
            }
            catch
            {
                // Don't crash on gettings some log data.
            }

            return processes;
        }
    }

    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartPath { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1} [{2}]", Id, Name ?? "Unknown", StartPath ?? "Unknown");
        }
    }
}
