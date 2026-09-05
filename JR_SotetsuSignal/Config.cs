
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

namespace JR_SotetsuSignal {

    public static class Config {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public const double LessInf = 0x7fffffff;
        public static string PluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string path;
        private const int buffer_size = 4096;

        //������
        public static bool SNEnable = true;
        public static bool PPowerAlwaysLight = false;

        public static bool ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
        public static bool ORPUseNeedle = true;//1:pilotlamp 0:needle
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
            path = new FileInfo(Path.Combine(PluginDir, "JR_SotetsuSignal.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    ReadConfig("panel", "atclimituseneedle", ref ATCLimitUseNeedle);
                    ReadConfig("panel", "orpuseneedle", ref ORPUseNeedle);
                    ReadConfig("panel", "islcd", ref isLCD);
                    ReadConfig("panel", "lcdrefreshinterval", ref LCDRefreshInterval);

                    ReadConfig("ats", "snenable", ref SNEnable);
                    ReadConfig("ats", "ppowerlampalwayslight", ref PPowerAlwaysLight);

                    ReadConfig("output","power",ref Panel_poweroutput);
                    ReadConfig("output","brake",ref Panel_brakeoutput);
                    ReadConfig("output","key",ref Panel_keyoutput);
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
            } else throw new BveFileLoadException("Unable to find configuration file: JR_SotetsuSignal.ini","JR_SotetsuSignal");
        }

        /// <summary>注册本插件全部面板灯逻辑键与默认端子号（与硬编码默认一致）。</summary>
        private static void RegisterOutputDefaults() {
            // ATS-P 状态灯（P 優先；256-262 为 ATS-P 块）
            RegisterPanel("P_Power", 256); RegisterPanel("P_PatternApproach", 257);
            RegisterPanel("P_BrakeActioned", 258); RegisterPanel("P_EBActioned", 259);
            RegisterPanel("P_BrakeOverride", 260); RegisterPanel("P_PEnable", 261);
            RegisterPanel("P_Fail", 262);
            // ATS-SN 状态灯（341-342 为 ATS-SN 块）
            RegisterPanel("SN_Power", 341); RegisterPanel("SN_Action", 342);

            // 声音（256 警報=信号注意区間/SN 警報共用端子；257 SN チャイム；258 P 状態変化 Ding；273 リセットSW）
            RegisterSound("ResetSW", 273);
            RegisterSound("Warning", 256);
            RegisterSound("Ding", 258);
            RegisterSound("Chime", 257);
        }

        private static void RegisterPanel(string key, int defaultIndex) { PanelMap.RegisterDefault(key, defaultIndex); }
        private static void RegisterSound(string key, int defaultIndex) { SoundMap.RegisterDefault(key, defaultIndex); }

        public static void Dispose() {
            SNEnable = true;
            PPowerAlwaysLight = false;
            ATCLimitUseNeedle = true;//1:pilotlamp 0:needle
            ORPUseNeedle = true;//1:pilotlamp 0:needle

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
