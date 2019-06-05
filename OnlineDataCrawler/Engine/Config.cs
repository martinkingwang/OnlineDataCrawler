using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineDataCrawler.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace OnlineDataCrawler.Engine
{
    class Config
    {
        public const string DateTimeFormat = "yyyyMMdd hh:mm:ss";
        private const string ConfName = "configure.json";

        private static readonly Lazy<JObject> Settings = new Lazy<JObject>(() =>
        {
            if (!File.Exists(ConfName))
            {
                return new JObject
                {
                    {"test", "a" }
                };
            }
            else
            {
                return JObject.Parse(File.ReadAllText(ConfName));
            }
        });

        public static DateTime GetDateTime(string key)
        {
            var date = Get(key);
            try
            {
                return DateTime.ParseExact(date, DateTimeFormat, null, System.Globalization.DateTimeStyles.None);
            }
            catch
            {
                Log.Trace("Config.GetDateTime(): Parse error - Using default value.");
                return DateTime.Now;
            }
        }

        public static void SetDateTime(string key, DateTime dateTime)
        {
            var value = dateTime.ToString(DateTimeFormat);
            Set(key, value);
        }

        public static DateTime GetDateTime(string key, DateTime defaultTime)
        {
            var date = Get(key);
            try
            {
                return DateTime.ParseExact(date, DateTimeFormat, null, System.Globalization.DateTimeStyles.None);
            }
            catch
            {
                Log.Trace("Config.GetDateTime(): Parse error - Using default value.");
                return defaultTime;
            }
        }

        public static void Set(string key, string value)
        {
            JToken environment = Settings.Value;
            environment[key] = value;
            Write();
        }

        public static string Get(string key, string defaultValue = "")
        {
            var token = GetToken(Settings.Value, key);
            if (token == null)
            {
                Log.Trace(string.Format("Config.Get(): Configuration key not found. Key: {0} - Using default value: {1}", key, defaultValue));
                return defaultValue;
            }
            return token.ToString();
        }


        public static T GetValue<T>(string key, T defaultValue = default(T))
        {
            var token = GetToken(Settings.Value, key);
            if (token == null)
            {
                Log.Trace(string.Format("Config.GetValue(): {0} - Using default value: {1}", key, defaultValue));
                return defaultValue;
            }

            var type = typeof(T);
            string value;
            try
            {
                value = token.Value<string>();
            }
            catch (Exception)
            {
                value = token.ToString();
            }

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, value);
            }

            if (typeof(IConvertible).IsAssignableFrom(type))
            {
                return (T)Convert.ChangeType(value, type);
            }

            // try and find a static parse method
            try
            {
                var parse = type.GetMethod("Parse", new[] { typeof(string) });
                if (parse != null)
                {
                    var result = parse.Invoke(null, new object[] { value });
                    return (T)result;
                }
            }
            catch (Exception err)
            {
                Log.Trace("Config.GetValue<{0}>({1},{2}): Failed to parse: {3}. Using default value.", typeof(T).Name, key, defaultValue, value);
                Log.Error(err);
                return defaultValue;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception err)
            {
                Log.Trace("Config.GetValue<{0}>({1},{2}): Failed to JSON deserialize: {3}. Using default value.", typeof(T).Name, key, defaultValue, value);
                Log.Error(err);
                return defaultValue;
            }
        }

        public static void Write()
        {
            if (!Settings.IsValueCreated) return;
            var serialized = JsonConvert.SerializeObject(Settings.Value, Formatting.Indented);
            File.WriteAllText(ConfName, serialized);
        }

        private static JToken GetToken(JToken settings, string key)
        {
            return GetToken(settings, key, settings.SelectToken(key));
        }

        private static JToken GetToken(JToken settings, string key, JToken current)
        {
            var environmentSetting = settings.SelectToken("environment");
            if (environmentSetting != null)
            {
                var environmentSettingValue = environmentSetting.Value<string>();
                if (!string.IsNullOrWhiteSpace(environmentSettingValue))
                {
                    var environment = settings.SelectToken("environments." + environmentSettingValue);
                    if (environment != null)
                    {
                        var setting = environment.SelectToken(key);
                        if (setting != null)
                        {
                            current = setting;
                        }
                        return GetToken(environment, key, current);
                    }
                }
            }
            if (current == null)
            {
                return settings.SelectToken(key);
            }
            return current;
        }
    }
}
