using System.Collections.Generic;
using System.Diagnostics;

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
        bool Kill(StartupOptions opts);
        void KillAll(string processName);
        void SetPriority(int processId, ProcessPriorityClass priority);
        void WaitForExit(Process process);
    }
}