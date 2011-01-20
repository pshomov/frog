using System;
using System.Diagnostics;
using System.Linq;

namespace Frog.Support
{
    public static class ProcessHelpers
    {
        public static bool IsWebApp()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(
                assembly => assembly.FullName.StartsWith("System.Web.Mvc,"));
        }
    }
}