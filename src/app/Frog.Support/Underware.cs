using System;
using System.Collections.Generic;
using System.Linq;

namespace Frog.Support
{
    public class Underware
    {
        public class As
        {
            public static List<T> ListOf<T>(params T[] items)
            {
                var result = new List<T>();
                result.AddRange(items);
                return result;
            }
        }

        public static bool IsUnix
        {
            get
            {
                var unix = As.ListOf(PlatformID.MacOSX, PlatformID.Unix);
                return unix.Contains(Environment.OSVersion.Platform); 
            }
        }
        public static bool IsWindows        {
            get
            {
                var win = As.ListOf(PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows);
                return win.Contains(Environment.OSVersion.Platform); 
            }
        }

        public static bool IsWebApp()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(
                assembly => assembly.FullName.StartsWith("System.Web.Mvc,"));
        }
    }
}