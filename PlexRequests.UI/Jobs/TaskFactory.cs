using FluentScheduler;
using Nancy.TinyIoc;

namespace PlexRequests.UI.Jobs
{
    public class TaskFactory : ITaskFactory
    {
        public ITask GetTaskInstance<T>() where T : ITask
        {
            var container = TinyIoCContainer.Current;
            object outT;
            container.TryResolve(typeof(T), out outT);

            return (T)outT;
        }
    }
}