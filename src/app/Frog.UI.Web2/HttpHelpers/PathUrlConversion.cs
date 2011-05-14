using System;

namespace Frog.UI.Web
{
	public static class PathUrlConversion
	{
		public static string Path2Url(string path){
			return path.Replace('/', '|');
		}
		public static string Url2Path(string path){
			return path.Replace('|', '/');
		}
	}
}

