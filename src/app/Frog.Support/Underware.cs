using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Frog.Support
{
    public class Underware
    {
        public class As
        {
            public static List<T> List<T>(params T[] items)
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
                var unix = As.List(PlatformID.MacOSX, PlatformID.Unix);
                return unix.Contains(Environment.OSVersion.Platform); 
            }
        }
        public static bool IsWindows        {
            get
            {
                var win = As.List(PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows);
                return win.Contains(Environment.OSVersion.Platform); 
            }
        }

        public static bool IsWebApp()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(
                assembly => assembly.FullName.StartsWith("System.Web.Mvc,"));
        }
        static string GitScriptsLocation(string subfolder)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (Underware.IsWebApp() ? "bin" + Path.DirectorySeparatorChar : "") + subfolder);
        }

        public static string GitProductionScriptsLocation
        {
            get { return GitScriptsLocation("git_scripts"); }
        }

        public static string GitSupportScriptsLocation
        {
            get { return GitScriptsLocation("git_support_scripts"); }
        }

        public static string DirChars(string path)
        {
            if (IsUnix)
            {
                return path.Replace('\\', Path.DirectorySeparatorChar);
            }

            if (IsWindows)
            {
                return path.Replace('/', Path.DirectorySeparatorChar);
            }

            throw new PlatformNotSupportedException(" Not sure how t handle the path on this platform, please report this error");
        }
    }
}