using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace MetroAts {
    public static class Config {
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static double EBDec = 5.0;
        public static double MaxSpeed = 100;
        public static bool ATCLimitPerLamp = false;//1:pilotlamp 0:needle
        public const double LessInf = 0xffffffff;
        private static void Cfg(this Dictionary<string, string> configDict, string key, ref double param) {
            if (configDict.ContainsKey(key)) {
                var value = configDict[key].ToLowerInvariant();
                if (value == "inf") {
                    param = LessInf;
                } else if (value == "-inf") {
                    param = -LessInf;
                } else {
                    double result;
                    if (!double.TryParse(configDict[key], out result)) return;
                    param = result;
                }
            }
        }

        private static void Cfg(this Dictionary<string, string> configDict, string key, ref bool param) {
            if (configDict.ContainsKey(key)) {
                var str = configDict[key].ToLowerInvariant();
                param = (str == "true" || str == "1");
            }
        }

        private static void Cfg(this Dictionary<string, string> configDict, string key, ref string param) {
            if (configDict.ContainsKey(key)) {
                param = configDict[key];
            }
        }

        private static void Cfg(this Dictionary<string, string> configDict, string key, ref int[] param) {
            if (configDict.ContainsKey(key)) {
                var outputList = new List<int>();
                foreach (var value in configDict[key].Split(',')) {
                    int result;
                    if (!int.TryParse(value.Trim(), out result)) return;
                    outputList.Add(result);
                }
                param = outputList.ToArray();
            }
        }

        public static void Load(string path) {
            if (!File.Exists(path)) return;

            var dict = new Dictionary<string, string>();
            StreamReader configFile = File.OpenText(path);
            string line;
            while ((line = configFile.ReadLine()) != null) {
                line = line.Trim();
                if (line.Length > 0 && line[0] != '#') {
                    string[] commentTokens = line.Split('#');
                    string[] tokens = commentTokens[0].Trim().Split('=');
                    dict.Add(tokens[0].Trim().ToLowerInvariant(), tokens[1].Trim());
                }
            }
            configFile.Close();

            dict.Cfg("EmergencyBrakeDeceleration", ref EBDec);
            dict.Cfg("MaxSpeed", ref MaxSpeed);
            dict.Cfg("ATCLimitPerLamp", ref ATCLimitPerLamp);
        }
    }
}
