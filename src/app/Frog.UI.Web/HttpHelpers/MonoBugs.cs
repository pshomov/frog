using System.Web.Script.Serialization;
using System.Web.Mvc;
using System.Text;

namespace Frog.UI.Web.HttpHelpers
{
	public class MonoBugs
	{
        public static ContentResult Json(object o)
		{
			var ser = new JavaScriptSerializer();
        	return new ContentResult(){Content = ser.Serialize(o), ContentType = "application/json", ContentEncoding = Encoding.UTF8};
		}
	}
}

