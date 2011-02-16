using System;
using System.Threading;
using Frog.Support;

namespace Frog.Domain
{
    public class ExecTaskFactory
    {
        public virtual ExecTask CreateTask(string app, string args, string name)
        {
            if (Os.IsUnix)
                return new ExecTask(app, args, name);
            if (Os.IsWindows)
                if (app.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) || app.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase) )
                return new ExecTask(app, args, name);
                else
                return new ExecTask(app+".bat", args, name);
            throw new NotSupportedException("The platform is not handled properly, please report this error");
        }
    }
}