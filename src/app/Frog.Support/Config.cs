using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Frog.Support
{
    public class Config : DynamicObject
    {
        private readonly string appHome;
        private readonly List<JObject> cfg  = new List<JObject>();

        public Config(string app_home)
        {
            appHome = app_home;
            AddConfigIfPresent(appHome);
            DirectoryInfo parent = new DirectoryInfo(app_home);
            while((parent = Directory.GetParent(app_home)) != null)
            {
                app_home = parent.FullName;
                AddConfigIfPresent(app_home);
            }
        }

        private void AddConfigIfPresent(string directory)
        {
            var path = Path.Combine(directory, "config.json");
            if (File.Exists(path))
                cfg.Add(JObject.Parse(File.ReadAllText(path)));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            JToken val;
            var setting_values = cfg.Where(o => o.TryGetValue(binder.Name, out val));
            if (setting_values.Count() == 0) throw new SettingNotDefined();
            setting_values.First().TryGetValue(binder.Name, out val);
            result = val.Value<object>();
            return true;
        }
    }

    public class SettingNotDefined : Exception
    {
    }

}
