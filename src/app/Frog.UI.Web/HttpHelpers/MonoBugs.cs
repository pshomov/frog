using System.Web.Script.Serialization;
using System.Web.Mvc;
using System.Text;
using Newtonsoft.Json;

namespace Frog.UI.Web.HttpHelpers
{
	public class MonoBugs
	{
        public static ContentResult Json(object o)
		{
			
        	return new ContentResult(){Content = JsonConvert.SerializeObject(o), ContentType = "application/json", ContentEncoding = Encoding.UTF8};
		}
	}
}

