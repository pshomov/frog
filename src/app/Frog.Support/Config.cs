using System;
using System.Dynamic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Frog.Support
{
    public class Config : DynamicObject
    {
        private readonly string appHome;
        private readonly JObject cfg;

        public Config(string app_home)
        {
            appHome = app_home;
            cfg = JObject.Parse(File.ReadAllText(Path.Combine(appHome, "config.json")));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            JToken val;
            var has_value = cfg.TryGetValue(binder.Name, out val);
            if (has_value) 
                result = val.Value<object>();
            else 
                throw new SettingNotDefined();
            return has_value;
        }
    }

    public class SettingNotDefined : Exception
    {
    }

}
