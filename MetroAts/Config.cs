using System.Collections.Generic;
using System.Reflection;
using System.IO;
using BveEx.PluginHost;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System;
using System.Linq;

namespace MetroAts {
    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public const double LessInf = 0x7fffffff;
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string path;
        private const int buffer_size = 4096;

        public static List<KeyPosList> KeyPosLists = new List<KeyPosList>();
        public static List<SignalSWList> SignalSWLists = new List<SignalSWList>();
        public static bool SignalSW_loop = false;

        public static int Panel_brakeoutput = 1023;
        public static int Panel_poweroutput = 1023;
        public static int Panel_keyoutput = 1023;
        public static int Panel_SignalSWoutput = 1023;

        public static bool EnforceKeyPos = false;

        public static void Load() {
            path = new FileInfo(Path.Combine(PluginDir, "MetroAtsConfig.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    var KeysString = "";
                    ReadConfig("keys", "positions", ref KeysString);
                    foreach (var i in KeysString.Split(',')) {
                        KeyPosLists.Add((KeyPosList)Enum.Parse(typeof(KeyPosList), i, true));
                    }
                    if (!KeyPosLists.Contains(KeyPosList.None)) KeyPosLists.Add(KeyPosList.None);
                    KeyPosLists.Sort();

                    for (int i = 0; i < KeyPosLists.Count; ++i) {
                        if (KeyPosLists[i] == KeyPosList.None) {
                            MetroAts.NowKey = i;
                            break;
                        }
                    }
                    ReadConfig("keys", "enforce", ref EnforceKeyPos);

                    var SignalSWString = "";
                    ReadConfig("signalsw", "positions", ref SignalSWString);
                    foreach (var i in SignalSWString.Split(',')) {
                        SignalSWLists.Add((SignalSWList)Enum.Parse(typeof(SignalSWList), i, true));
                    }
                    if (!SignalSWLists.Contains(SignalSWList.Noset)&&!SignalSWLists.Contains(SignalSWList.JR)) SignalSWLists.Add(SignalSWList.Noset);

                    ReadConfig("signalsw", "isloop", ref SignalSW_loop);

                    ReadConfig("output", "signalsw", ref Panel_SignalSWoutput);
                    ReadConfig("output", "power", ref Panel_poweroutput);
                    ReadConfig("output", "brake", ref Panel_brakeoutput);
                    ReadConfig("output", "key", ref Panel_keyoutput);
                } catch (Exception ex) {
                    throw ex;
                }
            } else throw new BveFileLoadException("Unable to find configuration file: MetroAtsConfig.ini", "MetroAts");
        }

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
