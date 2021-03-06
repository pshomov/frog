using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Frog.Support
{
    public class Locations
    {
        static string GitScriptsLocation(string subfolder)
        {
            return Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, (Os.IsWebApp() ? "bin" : "")),subfolder);
        }

        public static string GitProductionScriptsLocation
        {
            get { return GitScriptsLocation("git_scripts"); }
        }

        public static string GitSupportScriptsLocation
        {
            get { return GitScriptsLocation("git_support_scripts"); }
        }

		public static string SupportScriptsLocation
        {
            get { return GitScriptsLocation("support_scripts"); }
        }
    }

    public class As
    {
        public static List<T> List<T>(params T[] items)
        {
            var result = new List<T>();
            result.AddRange(items);
            return result;
        }
    }

    public static class Lists
    {
        public static bool AreEqual<T>(IEnumerable<T> coll1, IEnumerable<T> coll2)
        {
            return coll1.Count() == coll2.Count() && coll1.SequenceEqual(coll2);
        }
    }

    public class Empty
    {
        public static List<T> ListOf<T>(T itemExample)
        {
            var result = new List<T>();
            return result;
        }
    }

    public static class Os
    {
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

            throw new PlatformNotSupportedException(" Not sure how to handle the path on this platform, please report this error");
        }
    }

    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static T LastItem<T>(this T[] ar )
        {
            return ar[ar.Length - 1];
        }

        public static bool Match(this string str, string pattern)
        {
            return Regex.IsMatch(str, pattern);
        }

        public static string format(this string str, params object[] objs)
        {
            return string.Format(str, objs);
        }
    }
}