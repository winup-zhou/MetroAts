
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

namespace TobuSignal {

    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public const double LessInf = 0x7fffffff;
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string path;
        private const int buffer_size = 4096;

        //������
        public static double MaxSpeed = 100;
        public static double TrainLength = 200;
        public static bool EnableATC = true;
        public static bool ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
        public static bool SeparateATCGRlamp = false;//1:pilotlamp 0:needle
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
            path = new FileInfo(Path.Combine(PluginDir, "TobuSignal.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    //train
                    ReadConfig("train", "maxspeed", ref MaxSpeed);
                    ReadConfig("train", "length", ref TrainLength);
                    ReadConfig("train", "enableatc", ref EnableATC);

                    //panel
                    ReadConfig("panel", "atclimituseneedle",ref ATCLimitUseNeedle);
                    ReadConfig("panel", "separateatcgrlamp", ref SeparateATCGRlamp);
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
            } else throw new BveFileLoadException("Unable to find configuration file: TobuSignal.ini","TobuSignal");
        }

        /// <summary>注册本插件全部面板灯逻辑键与默认端子号（与硬编码默认一致）。</summary>
        private static void RegisterOutputDefaults() {
            // T-DATC 速度指示（针表/数显；1pilotlamp 0:needle）
            RegisterPanel("ATC_01", 287); RegisterPanel("ATC_10", 288); RegisterPanel("ATC_15", 289);
            RegisterPanel("ATC_20", 290); RegisterPanel("ATC_25", 291); RegisterPanel("ATC_30", 292);
            RegisterPanel("ATC_35", 293); RegisterPanel("ATC_40", 294); RegisterPanel("ATC_45", 295);
            RegisterPanel("ATC_50", 296); RegisterPanel("ATC_55", 297); RegisterPanel("ATC_60", 298);
            RegisterPanel("ATC_65", 299); RegisterPanel("ATC_70", 300); RegisterPanel("ATC_75", 301);
            RegisterPanel("ATC_80", 302); RegisterPanel("ATC_85", 303); RegisterPanel("ATC_90", 304);
            RegisterPanel("ATC_95", 305); RegisterPanel("ATC_100", 306); RegisterPanel("ATC_105", 307);
            RegisterPanel("ATC_110", 308);
            // 停止/进行：合并灯 285/286；separateatcgrlamp=1 时改走独立 G/R 灯 316/317
            RegisterPanel("ATC_Stop", 285); RegisterPanel("ATC_Proceed", 286);
            RegisterPanel("ATC_StopSeparate", 316); RegisterPanel("ATC_ProceedSeparate", 317);
            // T-DATC 其它状态灯
            RegisterPanel("ATC_P", 313); RegisterPanel("ATC_X", 284);
            RegisterPanel("ORPNeedle", 314); RegisterPanel("ATCNeedle", 311); RegisterPanel("ATCNeedle_Disappear", 310);
            RegisterPanel("ATC_EndPointDistance", 323); RegisterPanel("ATC_SwitcherPosition", 324);
            RegisterPanel("ATC_TobuATC", 318); RegisterPanel("ATC_Depot", 319);
            RegisterPanel("ATC_ServiceBrake", 321); RegisterPanel("ATC_EmergencyBrake", 320);
            RegisterPanel("ATC_EmergencyOperation", 326); RegisterPanel("ATC_StationStop", 325);
            RegisterPanel("ATC_PatternApproach", 322);
            // TSP-ATS 状态灯
            RegisterPanel("ATS_TobuAts", 327); RegisterPanel("ATS_ATSEmergencyBrake", 330);
            RegisterPanel("ATS_StopAnnounce", 332); RegisterPanel("ATS_EmergencyOperation", 333);
            RegisterPanel("ATS_Confirm", 331); RegisterPanel("ATS_60", 328); RegisterPanel("ATS_15", 329);

            // 声音
            RegisterSound("ResetSW", 273);
            RegisterSound("Warning", 256);
            RegisterSound("Ding", 258);
            RegisterSound("PatternApproachBeep", 265);
            RegisterSound("StationStopAnnounce", 267);
            RegisterSound("Switchover", 266);
            RegisterSound("EmergencyOperationAnnounce", 268);
        }

        private static void RegisterPanel(string key, int defaultIndex) { PanelMap.RegisterDefault(key, defaultIndex); }
        private static void RegisterSound(string key, int defaultIndex) { SoundMap.RegisterDefault(key, defaultIndex); }

        public static void Dispose() {
            MaxSpeed = 100;
            TrainLength = 200;
            EnableATC = true;
            ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
            SeparateATCGRlamp = false;//1:pilotlamp 0:needle

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
