using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace TobuAts
{
    public static class Config
    {
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static bool Load_bve_autopilot = false, Load_csc_plugin = false, Load_Notchnumber_plugin = false,Load_RealAnalogGauge_plugin = false;
        public static string bve_autopilot_path = "";
        public static string csc_plugin_path = "";
        public static string Notchnumber_plugin_path = "";
        public const double LessInf = 100000000;
        private static void Cfg(this Dictionary<string, string> configDict, string key, ref double param)
        {
            if (configDict.ContainsKey(key))
            {
                var value = configDict[key].ToLowerInvariant();
                if (value == "inf")
                {
                    param = LessInf;
                }
                else if (value == "-inf")
                {
                    param = -LessInf;
                }
                else
                {
                    double result;
                    if (!double.TryParse(configDict[key], out result)) return;
                    param = result;
                }
            }
        }

        private static void Cfg(this Dictionary<string, string> configDict, string key, ref bool param)
        {
            if (configDict.ContainsKey(key))
            {
                var str = configDict[key].ToLowerInvariant();
                param = (str == "true" || str == "1");
            }
        }

        private static void Cfg(this Dictionary<string, string> configDict, string key, ref string param)
        {
            if (configDict.ContainsKey(key))
            {
                param = configDict[key];
            }
        }

        private static void Cfg(this Dictionary<string, string> configDict, string key, ref int[] param)
        {
            if (configDict.ContainsKey(key))
            {
                var outputList = new List<int>();
                foreach (var value in configDict[key].Split(','))
                {
                    int result;
                    if (!int.TryParse(value.Trim(), out result)) return;
                    outputList.Add(result);
                }
                param = outputList.ToArray();
            }
        }

        public static void Load(string path)
        {
            if (!File.Exists(path)) return;

            var dict = new Dictionary<string, string>();
            StreamReader configFile = File.OpenText(path);
            string line;
            while ((line = configFile.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length > 0 && line[0] != '#')
                {
                    string[] commentTokens = line.Split('#');
                    string[] tokens = commentTokens[0].Trim().Split('=');
                    dict.Add(tokens[0].Trim().ToLowerInvariant(), tokens[1].Trim());
                }
            }
            configFile.Close();

            dict.Cfg("autopilot", ref Load_bve_autopilot);
            dict.Cfg("cscplugin", ref Load_csc_plugin);
            dict.Cfg("notchnumber", ref Load_Notchnumber_plugin);
            dict.Cfg("realanaloggauge", ref Load_RealAnalogGauge_plugin);

            /*
            dict.Cfg("autopilotpath", ref bve_autopilot_path);
            dict.Cfg("cscpluginpath", ref csc_plugin_path);
            dict.Cfg("notchnumberpath", ref Notchnumber_plugin_path);
            bve_autopilot_path = Path.GetFullPath(Path.Combine(PluginDir, bve_autopilot_path));
            csc_plugin_path = Path.GetFullPath(Path.Combine(PluginDir, csc_plugin_path));
            Notchnumber_plugin_path = Path.GetFullPath(Path.Combine(PluginDir, Notchnumber_plugin_path));
            */
        }
    }
}
