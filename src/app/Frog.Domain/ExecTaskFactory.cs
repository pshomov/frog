using System;
using Frog.Domain.ExecTasks;
using Frog.Support;

namespace Frog.Domain
{
    public class ExecTaskFactory
    {
        public virtual ExecTask CreateOSExecutableTask(string app, string args, string name)
        {
            if (Os.IsUnix)
                return new OSExecuatableTask(app, args, name, (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
            if (Os.IsWindows)
                if (app.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) ||
                    app.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase))
                    return new OSExecuatableTask(app, args, name, (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
                else
                    return new OSExecuatableTask(app + ".bat", args, name, (p1, p2, p3) => new ProcessWrapper(p1, p2, p3));
            throw new NotSupportedException("The platform is not handled properly, please report this error");
        }
    }
}