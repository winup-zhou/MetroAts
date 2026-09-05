
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

namespace SeibuSignal {

    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public const double LessInf = 0x7fffffff;
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string path;
        private const int buffer_size = 4096;

        //������
        public static bool ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
        public static bool isLCD = false;
        public static int LCDRefreshInterval = 0;

        public static int Panel_poweroutput = 1023;
        public static int Panel_brakeoutput = 1023;
        public static int Panel_keyoutput = 1023;
        public static int Panel_HandleOutputRefreshInterval = 0;

        // 输出端子映射（逻辑键→端子号，[output] 段可覆盖）与平行状态记录
        public static readonly MetroAts.OutputIndexMap PanelMap = new MetroAts.OutputIndexMap();
        public static readonly MetroAts.OutputIndexMap SoundMap = new MetroAts.OutputIndexMap();

        public static void Load() {
            path = new FileInfo(Path.Combine(PluginDir, "SeibuSignal.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    //panel
                    ReadConfig("panel", "atclimituseneedle", ref ATCLimitUseNeedle);
                    ReadConfig("panel", "islcd", ref isLCD);
                    ReadConfig("panel", "lcdrefreshinterval", ref LCDRefreshInterval);

                    ReadConfig("output", "power", ref Panel_poweroutput);
                    ReadConfig("output", "brake", ref Panel_brakeoutput);
                    ReadConfig("output", "key", ref Panel_keyoutput);
                    ReadConfig("output", "handlerefreshinterval", ref Panel_HandleOutputRefreshInterval);

                    RegisterOutputDefaults();
                    PanelMap.Override(MetroAts.OutputIndexMap.ReadSection(path, "output", PanelMap.Index.Keys));
                    SoundMap.Override(MetroAts.OutputIndexMap.ReadSection(path, "output", SoundMap.Index.Keys));

                    // 是否仍写入 BVE 物理 panel/sound（仅保留状态暴露）
                    bool outputWritePanel = true, outputWriteSound = true;
                    ReadConfig("output", "writepanel", ref outputWritePanel);
                    ReadConfig("output", "writesound", ref outputWriteSound);
                    PanelMap.PanelWriteEnabled = outputWritePanel;
                    SoundMap.SoundWriteEnabled = outputWriteSound;
                } catch (Exception ex) {
                    throw ex;
                }
            } else throw new BveFileLoadException("Unable to find configuration file: SeibuSignal.ini","SeibuSignal");
        }

        /// <summary>注册本插件全部面板灯逻辑键与默认端子号（与硬编码默认一致）。</summary>
        private static void RegisterOutputDefaults() {
            // ATC 速度指示（CS-ATC 现示速度灯：仅定义用到的 01/25/40/55/75/90）
            RegisterPanel("ATC_01", 287); RegisterPanel("ATC_25", 291); RegisterPanel("ATC_40", 294);
            RegisterPanel("ATC_55", 297); RegisterPanel("ATC_75", 301); RegisterPanel("ATC_90", 304);
            // 停止/进行/照查类
            RegisterPanel("ATC_Stop", 285); RegisterPanel("ATC_Proceed", 286);
            RegisterPanel("ATC_X", 284);
            RegisterPanel("ATCNeedle", 311); RegisterPanel("ATCNeedle_Disappear", 310);
            RegisterPanel("ATC_ATC", 264); RegisterPanel("ATC_Depot", 275); RegisterPanel("ATC_Noset", 278);
            RegisterPanel("ATC_ServiceBrake", 271); RegisterPanel("ATC_EmergencyBrake", 267);
            RegisterPanel("ATC_EmergencyOperation", 281);
            // SeibuATS
            RegisterPanel("ATS_Power", 334); RegisterPanel("ATS_EB", 335); RegisterPanel("ATS_Stop", 336);
            RegisterPanel("ATS_Confirm", 337); RegisterPanel("ATS_Limit", 338);

            // 声音
            RegisterSound("ResetSW", 273);
            RegisterSound("Warning", 256);
            RegisterSound("Ding", 258);
            RegisterSound("EmergencyOperationAnnounce", 261);
            RegisterSound("StopAnnounce", 262);
            RegisterSound("EBAnnounce", 263);
        }

        private static void RegisterPanel(string key, int defaultIndex) { PanelMap.RegisterDefault(key, defaultIndex); }
        private static void RegisterSound(string key, int defaultIndex) { SoundMap.RegisterDefault(key, defaultIndex); }

        public static void Dispose() {
            ATCLimitUseNeedle = true;//1:pilotlamp 0:needle

            Panel_poweroutput = 1023;
            Panel_brakeoutput = 1023;
            Panel_keyoutput = 1023;

            Panel_HandleOutputRefreshInterval = 0;
            PanelMap.Clear();
            SoundMap.Clear();
        }

        //��ȡ������غ���
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
