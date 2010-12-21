using System;
using System.Collections.Generic;

namespace Frog.Domain.Specs
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
    }
}