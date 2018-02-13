using System.Collections.Generic;
using System.Diagnostics;
using Ombi.Helpers;

namespace Ombi.Updater
{
    public interface IProcessProvider
    {
        bool Exists(int processId);
        bool Exists(string processName);
        List<ProcessInfo> FindProcessByName(string name);
        ProcessInfo GetCurrentProcess();
        int GetCurrentProcessId();
        ProcessInfo GetProcessById(int id);
        void Kill(int processId);
        void KillAll(string processName);
        void SetPriority(int processId, ProcessPriorityClass priority);
        Process Start(string path, string args = null);
        void WaitForExit(Process process);
    }
}