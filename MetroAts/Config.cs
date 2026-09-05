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
        public static bool SignalSW_legacyoutput = false;

        /// <summary>默认档位（复位目标档）。由 [signalsw]default 显式指定；未指定时按 ResolveDefaultSignalSW 自动解析。</summary>
        public static SignalSWList DefaultSignalSW = SignalSWList.Noset;

        public static bool atotascsw_enable = false;

        /// <summary>
        /// 允许使用 ATO 的钥匙位（线路）白名单。由 [atotascsw]atokeys 指定（逗号分隔），
        /// 缺省仅 Metro（地铁）。用于决定核心 IsATOAvailable 是否认可设备上报的 ATO 可用状态。
        /// </summary>
        public static List<KeyPosList> ATOKeyPosLists = new List<KeyPosList>();

        public static int Panel_brakeoutput = 1023;
        public static int Panel_poweroutput = 1023;
        public static int Panel_keyoutput = 1023;
        public static int Panel_SignalSWoutput = 1023;
        public static int Panel_ATOTASCSWoutput = 1023;
        public static int Panel_HandleOutputRefreshInterval = 0;

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
                    ReadConfig("signalsw", "legacyoutput", ref SignalSW_legacyoutput);

                    DefaultSignalSW = ResolveDefaultSignalSW();

                    ReadConfig("atotascsw", "enable", ref atotascsw_enable);

                    var ATOKeysString = "";
                    ReadConfig("atotascsw", "atokeys", ref ATOKeysString);
                    ATOKeyPosLists.Clear();
                    if (string.IsNullOrWhiteSpace(ATOKeysString)) {
                        ATOKeyPosLists.Add(KeyPosList.Metro); // 缺省：仅地铁可用
                    } else {
                        foreach (var i in ATOKeysString.Split(',')) {
                            ATOKeyPosLists.Add((KeyPosList)Enum.Parse(typeof(KeyPosList), i, true));
                        }
                    }

                    ReadConfig("output", "signalsw", ref Panel_SignalSWoutput);
                    ReadConfig("output", "atotascsw", ref Panel_ATOTASCSWoutput);
                    ReadConfig("output", "power", ref Panel_poweroutput);
                    ReadConfig("output", "brake", ref Panel_brakeoutput);
                    ReadConfig("output", "key", ref Panel_keyoutput);
                    ReadConfig("output", "handlerefreshinterval", ref Panel_HandleOutputRefreshInterval);
                } catch (Exception ex) {
                    throw ex;
                }
            } else throw new BveFileLoadException("Unable to find configuration file: MetroAtsConfig.ini", "MetroAts");
        }

        /// <summary>
        /// 解析本车的"默认档位"（复位目标，恒为单一档位，而非并列候选）。
        /// 优先级：1) [signalsw]default 显式指定且存在于档位列表；
        ///         2) 列表含 Noset → Noset（常规：非设档独立存在）；
        ///         3) 列表仅含 JR（JR 兼任非设的车）→ JR；
        ///         4) 均不含 → Noset（调用侧在列表中找不到时保持原位不动）。
        /// </summary>
        private static SignalSWList ResolveDefaultSignalSW() {
            string configured = "";
            ReadConfig("signalsw", "default", ref configured);
            if (!string.IsNullOrEmpty(configured)) {
                try {
                    SignalSWList parsed = (SignalSWList)Enum.Parse(typeof(SignalSWList), configured, true);
                    if (SignalSWLists.Contains(parsed)) return parsed;
                } catch (Exception) {
                    // 无效配置值 → 回退到自动解析
                }
            }
            if (SignalSWLists.Contains(SignalSWList.Noset)) return SignalSWList.Noset;
            if (SignalSWLists.Contains(SignalSWList.JR)) return SignalSWList.JR; // JR-非设同位情形的车
            return SignalSWList.Noset;
        }

        public static void Dispose() {
            KeyPosLists.Clear();
            SignalSWLists.Clear();
            SignalSW_loop = false;
            DefaultSignalSW = SignalSWList.Noset;
            atotascsw_enable = false;
            ATOKeyPosLists.Clear();
            ATOKeyPosLists.Add(KeyPosList.Metro);

            Panel_brakeoutput = 1023;
            Panel_poweroutput = 1023;
            Panel_keyoutput = 1023;
            Panel_SignalSWoutput = 1023;
            EnforceKeyPos = false;

            SignalSW_legacyoutput = false;
            Panel_HandleOutputRefreshInterval = 0;
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
