using System.Collections.Generic;
using System.Reflection;
using System.IO;
using BveEx.PluginHost;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System;
using System.Linq;
using MetroAts;

namespace MetroPIAddon {
    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public const double LessInf = 0x7fffffff;
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string path;
        private const int buffer_size = 4096;
        public static KeyPosList StandAloneKey = KeyPosList.None;
        public static List<string> FDOpenSounds = new List<string>(), FDCloseSounds = new List<string>();
        public static bool FDenable = false, FDsinglelamp = false;
        public static double Delay_FDclosed = 1.0;
        public static int CurrentPanelIndex = 1023;
        public static double MaxCurrentSpeed = 20.0;
        public static bool Current_abs = false;
        public static Keys DriverBuzzerKey = Keys.None;
        public static Keys SnowBrakeKey = Keys.None;
        public static Keys InstrumentLightKey = Keys.None;
        public static double SnowBrakePressure = 0.0;

        public static void Load() {
            path = new FileInfo(Path.Combine(PluginDir, "MetroPIAddon.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    ReadConfig("standalonemode", "keyposition", ref StandAloneKey);

                    ReadConfig("PlatformDoor", "enable", ref FDenable);
                    ReadConfig("PlatformDoor", "singlelamp", ref FDsinglelamp);
                    ReadConfig("PlatformDoor", "ClosedDelay", ref Delay_FDclosed);
                    var opensoundStr = "";
                    ReadConfig("PlatformDoor", "Opensounds", ref opensoundStr);
                    foreach (var i in opensoundStr.ToString().Split(',')) {
                        FDOpenSounds.Add(i.Trim().ToLower());
                    }
                    var closesoundStr = "";
                    ReadConfig("PlatformDoor", "Closesounds", ref closesoundStr);
                    foreach (var i in closesoundStr.ToString().Split(',')) {
                        FDCloseSounds.Add(i.Trim().ToLower());
                    }

                    ReadConfig("Current", "panel", ref CurrentPanelIndex);
                    ReadConfig("Current", "maxcurrentspeed", ref MaxCurrentSpeed);
                    ReadConfig("Current", "abs", ref Current_abs);

                    ReadConfig("Inputs", "driverbuzzer", ref DriverBuzzerKey);
                    ReadConfig("Inputs", "snowbrake", ref SnowBrakeKey);
                    ReadConfig("Inputs", "InstrumentLightKey", ref InstrumentLightKey);

                    ReadConfig("snowbrake", "pressure", ref SnowBrakePressure);
                } catch (Exception ex) {
                    throw ex;
                }
            } else throw new BveFileLoadException("Unable to find configuration file: MetroPIAddon.ini", "MetroPIAddon");
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

        private static void ReadConfig(string Section, string Key, ref KeyPosList Value) {
            var OriginalVal = Value;
            var RetVal = new StringBuilder(buffer_size);
            var Readsize = GetPrivateProfileString(Section, Key, "", RetVal, buffer_size, path);
            if (Readsize > 0 && Readsize < buffer_size - 1) {
                Value = (KeyPosList)Enum.Parse(typeof(KeyPosList), RetVal.ToString(), true);
            } else {
                Value = OriginalVal;
            }
        }
    }
}
