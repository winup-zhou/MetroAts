using System.Collections.Generic;
using System.Reflection;
using System.IO;
using AtsEx.PluginHost;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX.DirectInput;
using System;
using System.Linq;

namespace MetroAts {
    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public static string path;

        public static double EBDec = 5.0;
        public static double TobuMaxSpeed = 100;
        public static bool ATCLimitUseNeedle = false;//1:pilotlamp 0:needle
        public static double LessInf = 0x7fffffff;

        public static void Load() {
            path = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MetroAtsConfig.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    TobuMaxSpeed = ReadConfigDouble("Main", "TobuMaxSpeed");
                    ATCLimitUseNeedle = ReadConfigBoolean("Main", "LimitUseNeedle");
                    EBDec = ReadConfigDouble("Main", "MaxEBDeceleration");

                } catch (Exception ex) {
                    throw ex;
                }
            } else throw new BveFileLoadException("設定ファイルは見つかりませんでした。", "MetroAts");
        }

        private static int ReadConfigInt32(string Section, string Key) {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, path);
            return Convert.ToInt32(RetVal.ToString());
        }

        private static double ReadConfigDouble(string Section, string Key) {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, path);
            return Convert.ToDouble(RetVal.ToString());
        }

        private static bool ReadConfigBoolean(string Section, string Key) {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, path);
            return Convert.ToBoolean(RetVal.ToString());
        }

        private static List<int> ReadConfigListInt32(string Section, string Key) {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, path);
            string[] strArray = RetVal.ToString().Split(',');
            return Array.ConvertAll(strArray, s => int.Parse(s)).ToList();
        }

        private static List<double> ReadConfigListDouble(string Section, string Key) {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, path);
            string[] strArray = RetVal.ToString().Split(',');
            return Array.ConvertAll(strArray, s => double.Parse(s)).ToList();
        }
    }
}
