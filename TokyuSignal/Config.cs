
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

namespace TokyuSignal {

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
        public static int Panel_SignalSWoutput = 1023;
        public static bool SignalSW_legacyoutput = false;
        public static int Panel_HandleOutputRefreshInterval = 0;

        // 输出端子映射（逻辑键→端子号，[output] 段可覆盖）与平行状态记录
        public static readonly MetroAts.OutputIndexMap PanelMap = new MetroAts.OutputIndexMap();
        public static readonly MetroAts.OutputIndexMap SoundMap = new MetroAts.OutputIndexMap();

        public static void Load() {
            path = new FileInfo(Path.Combine(PluginDir, "TokyuSignal.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    //panel
                    ReadConfig("panel", "atclimituseneedle", ref ATCLimitUseNeedle);
                    ReadConfig("panel", "islcd", ref isLCD);
                    ReadConfig("panel", "lcdrefreshinterval", ref LCDRefreshInterval);

                    ReadConfig("signalsw", "legacyoutput", ref SignalSW_legacyoutput);

                    ReadConfig("output", "power", ref Panel_poweroutput);
                    ReadConfig("output", "brake", ref Panel_brakeoutput);
                    ReadConfig("output", "key", ref Panel_keyoutput);
                    ReadConfig("output", "signalsw", ref Panel_SignalSWoutput);
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
            } else throw new BveFileLoadException("Unable to find configuration file: TokyuSignal.ini","TokyuSignal");
        }

        /// <summary>注册本插件全部面板灯逻辑键与默认端子号（与硬编码默认一致）。</summary>
        private static void RegisterOutputDefaults() {
            // ATC 速度指示
            RegisterPanel("ATC_01", 287); RegisterPanel("ATC_10", 288); RegisterPanel("ATC_15", 289);
            RegisterPanel("ATC_20", 290); RegisterPanel("ATC_25", 291); RegisterPanel("ATC_30", 292);
            RegisterPanel("ATC_35", 293); RegisterPanel("ATC_40", 294); RegisterPanel("ATC_45", 295);
            RegisterPanel("ATC_50", 296); RegisterPanel("ATC_55", 297); RegisterPanel("ATC_60", 298);
            RegisterPanel("ATC_65", 299); RegisterPanel("ATC_70", 300); RegisterPanel("ATC_75", 301);
            RegisterPanel("ATC_80", 302); RegisterPanel("ATC_85", 303); RegisterPanel("ATC_90", 304);
            RegisterPanel("ATC_95", 305); RegisterPanel("ATC_100", 306); RegisterPanel("ATC_105", 307);
            RegisterPanel("ATC_110", 308);
            // 停止/进行/照查类
            RegisterPanel("ATC_Stop", 285); RegisterPanel("ATC_Proceed", 286);
            RegisterPanel("ATC_P", 313); RegisterPanel("ATC_SignalAnn", 312); RegisterPanel("ATC_X", 284);
            RegisterPanel("ATCNeedle", 311); RegisterPanel("ATCNeedle_Disappear", 310);
            RegisterPanel("ATC_ATC", 265); RegisterPanel("ATC_Depot", 276); RegisterPanel("ATC_Noset", 279);
            RegisterPanel("ATC_ServiceBrake", 272); RegisterPanel("ATC_EmergencyBrake", 268);
            RegisterPanel("ATC_EmergencyOperation", 282); RegisterPanel("ATC_StationStop", 342);
            RegisterPanel("ATS_TokyuATS", 343); RegisterPanel("ATS_EB", 344);
            RegisterPanel("ATS_WarnNormal", 345); RegisterPanel("ATS_WarnTriggered", 346);

            // 声音
            RegisterSound("ResetSW", 273);
            RegisterSound("Warning", 256);
            RegisterSound("Ding", 258);
            RegisterSound("ORPBeep", 259);
            RegisterSound("SignalAnnBeep", 260);
            RegisterSound("WarnBell", 269);
            RegisterSound("EmergencyOperationAnnounce", 261);
        }

        private static void RegisterPanel(string key, int defaultIndex) { PanelMap.RegisterDefault(key, defaultIndex); }
        private static void RegisterSound(string key, int defaultIndex) { SoundMap.RegisterDefault(key, defaultIndex); }

        public static void Dispose() {
            ATCLimitUseNeedle = true;//1:pilotlamp 0:needle

            Panel_poweroutput = 1023;
            Panel_brakeoutput = 1023;
            Panel_keyoutput = 1023;
            Panel_SignalSWoutput = 1023;

            SignalSW_legacyoutput = false;
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
