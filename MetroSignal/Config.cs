
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using BveEx.PluginHost;

namespace MetroSignal {

    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public const double LessInf = 0x7fffffff;
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string path;
        private const int buffer_size = 4096;

        //配置项
        public static bool ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
        public static bool ORPUseNeedle = true;//1:pilotlamp 0:needle
        public static List<SignalSWListStandAlone> SignalSWLists = new List<SignalSWListStandAlone>();

        public static int Panel_poweroutput = 1023;
        public static int Panel_brakeoutput = 1023;
        public static int Panel_keyoutput = 1023;
        public static int Panel_SignalSWoutput = 1023;
        public static bool SignalSW_legacyoutput = false;
        public static int Panel_HandleOutputRefreshInterval = 0;

        public static void Load() {
            path = new FileInfo(Path.Combine(PluginDir, "MetroSignal.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    //panel
                    ReadConfig("panel", "atclimituseneedle", ref ATCLimitUseNeedle);
                    ReadConfig("panel", "orpuseneedle", ref ORPUseNeedle);

                    var SignalSWString = "";
                    ReadConfig("signalsw", "positions", ref SignalSWString);
                    foreach (var i in SignalSWString.Split(',')) {
                        SignalSWLists.Add((SignalSWListStandAlone)Enum.Parse(typeof(SignalSWListStandAlone), i, true));
                    }
                    if (!SignalSWLists.Contains(SignalSWListStandAlone.Noset))
                        SignalSWLists.Add(SignalSWListStandAlone.Noset);
                    ReadConfig("signalsw", "legacyoutput", ref SignalSW_legacyoutput);

                    ReadConfig("output", "power", ref Panel_poweroutput);
                    ReadConfig("output", "brake", ref Panel_brakeoutput);
                    ReadConfig("output", "key", ref Panel_keyoutput);
                    ReadConfig("output", "signalsw", ref Panel_SignalSWoutput);
                    ReadConfig("output", "handlerefreshinterval", ref Panel_HandleOutputRefreshInterval);
                } catch (Exception ex) {
                    throw ex;
                }
            } else throw new BveFileLoadException("Unable to find configuration file: MetroSignal.ini","MetroSignal");
        }

        public static void Dispose() {
            ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
            ORPUseNeedle = true;//1:pilotlamp 0:needle
            SignalSWLists.Clear();

            Panel_poweroutput = 1023;
            Panel_brakeoutput = 1023;
            Panel_keyoutput = 1023;
            Panel_SignalSWoutput = 1023;

            Panel_HandleOutputRefreshInterval = 0; 
            SignalSW_legacyoutput = false;
        }

        //读取配置相关函数
        private static void ReadConfig(string Section, string Key, ref int Value) {
            var OriginalVal = Value;
            var RetVal = new StringBuilder(buffer_size);
            var Readsize = GetPrivateProfileString(Section, Key, "", RetVal, buffer_size, path);
            if (Readsize > 0 && Readsize < buffer_size - 1) {
                Value = Convert.ToInt32(RetVal.ToString());
            } else {
                Value = OriginalVal;
            }
        }

        private static void ReadConfig(string Section, string Key, ref double Value) {
            var OriginalVal = Value;
            var RetVal = new StringBuilder(buffer_size);
            var Readsize = GetPrivateProfileString(Section, Key, "", RetVal, buffer_size, path);
            if (Readsize > 0 && Readsize < buffer_size - 1) {
                Value = Convert.ToDouble(RetVal.ToString());
            } else {
                Value = OriginalVal;
            }
        }

        private static void ReadConfig(string Section, string Key, ref bool Value) {
            var OriginalVal = Value;
            var RetVal = new StringBuilder(buffer_size);
            var Readsize = GetPrivateProfileString(Section, Key, "", RetVal, buffer_size, path);
            if (Readsize > 0 && Readsize < buffer_size - 1) {
                Value = Convert.ToBoolean(RetVal.ToString());
            } else {
                Value = OriginalVal;
            }
        }

        private static void ReadConfig(string Section, string Key, ref string Value) {
            var OriginalVal = Value;
            var RetVal = new StringBuilder(buffer_size);
            var Readsize = GetPrivateProfileString(Section, Key, "", RetVal, buffer_size, path);
            if (Readsize > 0 && Readsize < buffer_size - 1) {
                Value = RetVal.ToString();
            } else {
                Value = OriginalVal;
            }
        }

        private static void ReadConfig(string Section, string Key, ref Keys Value) {
            var OriginalVal = Value;
            var RetVal = new StringBuilder(buffer_size);
            var Readsize = GetPrivateProfileString(Section, Key, "", RetVal, buffer_size, path);
            if (Readsize > 0 && Readsize < buffer_size - 1) {
                Value = (Keys)Enum.Parse(typeof(Keys), RetVal.ToString(), false);
            } else {
                Value = OriginalVal;
            }
        }
    }
}
