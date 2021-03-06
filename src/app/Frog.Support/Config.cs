﻿using System;
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
        private static dynamic _env;

        public Config(string app_home)
        {
            var command_line = Environment.GetCommandLineArgs().Skip(1);
            command_line.Select((s, i) =>
                {
                    if (s == "-c") return command_line.ToList()[i + 1];
                    else return String.Empty;
                }).Where(s => s != String.Empty).ToList().ForEach(s =>
                    {
                        var key_value = s.Split('=');
                        cfg.Add(new JObject{
                            {
                                key_value[0], key_value[1]
                            }
                        });
                    });
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
            this.cfg = new List<JObject> {cfg};
        }

        public static dynamic Env
        {
            get { return _env; }
        }

        static Config()
        {
            dynamic config = new Config(AppDomain.CurrentDomain.BaseDirectory);
            var env = config.RUNZ_ENV;
            _env = config[env];
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

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            GetSetingValue((string) indexes[0], out result);
            return true;
        }

        private void GetSetingValue(string settingName, out object result)
        {
            JToken val;
            var setting_values = cfg.Where(o => o.TryGetValue(settingName, out val)).ToList();
            if (!setting_values.Any()) throw new SettingNotDefined(settingName, cfg);
            setting_values.First().TryGetValue(settingName, out val);
            if (val.HasValues)
                result = new Config((JObject) val);
            else
                result = val.Value<object>();
        }
    }

    public class SettingNotDefined : Exception
    {
        private readonly string settingName;
        private readonly IEnumerable<JObject> cfgs;

        public SettingNotDefined(string settingName, IEnumerable<JObject> cfgs)
        {
            this.settingName = settingName;
            this.cfgs = cfgs;
        }

        public override string ToString()
        {
            return settingName + ", Settings: " +cfgs.Select(o => o.ToString()).Aggregate((s, s1) => s + ", " + s1);
        }

        public override string Message
        {
            get { return this.ToString(); }
        }
    }

}
