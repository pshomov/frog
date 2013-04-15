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
        private readonly List<JObject> cfg  = new List<JObject>();

        public Config(string app_home)
        {
            AddConfigIfPresent(app_home);
            var parent = new DirectoryInfo(app_home);
            while((parent = Directory.GetParent(app_home)) != null)
            {
                app_home = parent.FullName;
                AddConfigIfPresent(app_home);
            }
        }

        Config(JObject cfg)
        {
            this.cfg = new List<JObject>(){cfg};
        }

        private void AddConfigIfPresent(string directory)
        {
            var path = Path.Combine(directory, "config.json");
            if (File.Exists(path))
                cfg.Add(JObject.Parse(File.ReadAllText(path)));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            GetSetingValue(binder.Name, out result);
            return true;
        }

        private void GetSetingValue(string settingName, out object result)
        {
            JToken val;
            var setting_values = cfg.Where(o => o.TryGetValue(settingName, out val)).ToList();
            if (!setting_values.Any()) throw new SettingNotDefined();
            setting_values.First().TryGetValue(settingName, out val);
            if (val.HasValues)
                result = new Config((JObject) val);
            else
                result = val.Value<object>();
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var name = (string) indexes[0];
            GetSetingValue(name, out result);
            return true;
        }
    }

    public class SettingNotDefined : Exception
    {
    }

}
