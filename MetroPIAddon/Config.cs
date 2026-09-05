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
        public static bool FDsoundenable = false, FDsinglelamp = false;
        public static double Delay_FDclosed = 1.0;
        public static int CurrentPanelIndex = 1023;
        public static double MaxCurrentSpeed = 20.0;
        public static bool Current_abs = false;
        public static Keys DriverBuzzerKey = Keys.None;
        public static Keys OnBoardDepartMelodyKey = Keys.None;
        public static Keys SnowBrakeKey = Keys.None;
        public static Keys InstrumentLightKey = Keys.None;
        public static double SnowBrakePressure = 0.0;
        public static int MaxTrainTypeCount = 19;
        public static int Panel_LineDefOutput = 1023;
        public static int Panel_RadiochannelOutput = 1023;

        // 输出端子映射（逻辑键→端子号，[output] 段可覆盖）与平行状态记录
        public static readonly MetroAts.OutputIndexMap PanelMap = new MetroAts.OutputIndexMap();
        public static readonly MetroAts.OutputIndexMap SoundMap = new MetroAts.OutputIndexMap();

        //[odometer]
        public static int odometer_Kmsymbol = 1023;
        public static int odometer_Km100 = 1023;
        public static int odometer_Km10 = 1023;
        public static int odometer_Km1 = 1023;
        public static int odometer_Km01 = 1023;
        public static int odometer_Km001 = 1023;

        public static int depart_melody = 1023;
        public static int depart_announce = 1023;

        public static void Load() {
            path = new FileInfo(Path.Combine(PluginDir, "MetroPIAddon.ini")).FullName;
            if (File.Exists(path)) {
                try {
                    ReadConfig("standalonemode", "keyposition", ref StandAloneKey);

                    ReadConfig("PlatformDoor", "soundenable", ref FDsoundenable);
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
                    ReadConfig("Inputs", "onboarddepartmelody", ref OnBoardDepartMelodyKey);
                    ReadConfig("Inputs", "snowbrake", ref SnowBrakeKey);
                    ReadConfig("Inputs", "InstrumentLightKey", ref InstrumentLightKey);

                    ReadConfig("snowbrake", "pressure", ref SnowBrakePressure);

                    ReadConfig("traininfo", "typecounts", ref MaxTrainTypeCount);
                    ReadConfig("traininfo", "linedef", ref Panel_LineDefOutput);
                    ReadConfig("traininfo", "radiochannel", ref Panel_RadiochannelOutput);

                    ReadConfig("odometer", "kmsymbol", ref odometer_Kmsymbol);
                    ReadConfig("odometer", "km100", ref odometer_Km100);
                    ReadConfig("odometer", "km10", ref odometer_Km10);
                    ReadConfig("odometer", "km1", ref odometer_Km1);
                    ReadConfig("odometer", "km0.1", ref odometer_Km01);
                    ReadConfig("odometer", "Km0.01", ref odometer_Km001);

                    ReadConfig("departmelody", "melody", ref depart_melody);
                    ReadConfig("departmelody", "announce", ref depart_announce);

                    // 输出端子映射注册与 [output] 段覆盖（须在所有既有 ReadConfig 之后，
                    // 以便配置驱动端子的逻辑键默认值 = 已生效的旧字段值）
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
            } else throw new BveFileLoadException("Unable to find configuration file: MetroPIAddon.ini", "MetroPIAddon");
        }

        /// <summary>
        /// 注册本插件全部面板/声音输出逻辑键与默认端子号。
        /// - 固定端子号（代码内直接写 panel[n]）直接以该 n 注册；
        /// - 端子号可由既有 ini 段（[odometer]/[Current]/[traininfo]/[departmelody]）配置的输出，
        ///   以"已读入的 Config 字段值"作为默认端子注册（未配置时 = 1023 空置，与旧行为一致）；
        ///   这些键仍可再经新 [output] 段覆盖（新机制优先于旧字段段）。
        /// </summary>
        private static void RegisterOutputDefaults() {
            // ホームドア/連動表示
            RegisterPanel("platformdoor_mode", 155);      // 連動モード表示 0:OFF 1:連動 2:その他（FDmode 值）
            RegisterPanel("platformdoor_ind_right", 181); // 右側(車両右側扉)ホームドア側表示灯 0/1（点滅含む）
            RegisterPanel("platformdoor_ind_left", 182);  // 左側(車両左側扉)ホームドア側表示灯 0/1（点滅含む）
            RegisterPanel("platformdoor_count", 193);     // ホームドア開閉インターロック進捗数値表示 0..12
            // 駅番号表示
            RegisterPanel("station_stop", 167);           // 停車（戸開）中に現在駅番号を表示
            RegisterPanel("station_current", 168);        // 走行中に現在駅番号を表示
            RegisterPanel("station_next", 169);           // 走行中に次駅番号を表示
            // 列車番号表示（桁別；62..68 の各表示桁）
            RegisterPanel("trainnumber_1000000", 67);     // 10^6 以上の桁（整数，通常 0）
            RegisterPanel("trainnumber_100000", 62);      // 10^5 桁
            RegisterPanel("trainnumber_10000", 63);       // 10^4 桁
            RegisterPanel("trainnumber_1000", 64);        // 10^3 桁
            RegisterPanel("trainnumber_100", 65);         // 10^2 桁
            RegisterPanel("trainnumber_10", 68);          // 下 2 桁（0..99）
            // 種別・運用番号・行先
            RegisterPanel("traintype", 151);
            RegisterPanel("traintype_sub", 152);
            RegisterPanel("runningnumber_10", 153);       // 運用番号 10 桁
            RegisterPanel("runningnumber_1", 154);        // 運用番号 1 桁
            RegisterPanel("destination", 172);
            // 里程計（桁別；端子号旧 [odometer] 段可配）
            RegisterPanel("odometer_kmsymbol", odometer_Kmsymbol); // +/- 記号表示（0/1/2）
            RegisterPanel("odometer_km100", odometer_Km100);       // 100km 桁
            RegisterPanel("odometer_km10", odometer_Km10);         // 10km 桁
            RegisterPanel("odometer_km1", odometer_Km1);           // 1km 桁
            RegisterPanel("odometer_km01", odometer_Km01);         // 0.1km(100m) 桁
            RegisterPanel("odometer_km001", odometer_Km001);       // 0.01km(10m) 桁
            // 電流計（端子号旧 [Current] 段可配）
            RegisterPanel("current", CurrentPanelIndex);
            // 時計
            RegisterPanel("clock_hour", 58);
            RegisterPanel("clock_minute", 59);
            RegisterPanel("clock_second", 60);
            // 表示灯/状態
            RegisterPanel("keyposition", 166);            // マスコンキー現在位置 1..8（None は書込まない・非StandAlone時のみ）
            RegisterPanel("snowbrake", 176);              // 雪切ブレーキ 0/1
            RegisterPanel("instrumentlight", 161);        // 計器照明 0/1
            RegisterPanel("stopannounce_lamp", 251);      // 停車駅放送中 点滅 0/1
            RegisterPanel("speed_over5", 173);            // 5km/h 超過 0/1
            // 無線チャンネル・線区定義（端子号旧 [traininfo] 段可配）
            RegisterPanel("radiochannel", Panel_RadiochannelOutput); // 0..8（None=0）
            RegisterPanel("linedef", Panel_LineDefOutput);           // 0..8（None=0）

            // 声音
            RegisterSound("stopannounce", 5);            // 停車駅(到着)予告/接近放送
            RegisterSound("stopannounce_confirmed", 6);  // 停車確認後の到着放送
            RegisterSound("lampsw_on", 12);              // 計器照明 ON 音
            RegisterSound("lampsw_off", 13);             // 計器照明 OFF 音
            RegisterSound("snowbrake_on", 14);           // 雪切ブレーキ ON 音
            RegisterSound("snowbrake_off", 15);          // 雪切ブレーキ OFF 音
            RegisterSound("eb_alarm", 27);               // 非常ブレーキ作動ブザー
            RegisterSound("tobu_doorclosed", 30);        // 東武 戸閉ブザー
            RegisterSound("conductor_depart", 31);       // 車掌 発車合図
            RegisterSound("door_poon", 32);              // ドア開閉ブザー(プーン)
            RegisterSound("depart_melody", depart_melody);     // 車内発車メロディ（端子号旧 [departmelody] 段可配）
            RegisterSound("depart_announce", depart_announce); // 車内発車放送（端子号旧 [departmelody] 段可配）
            RegisterSound("conductor_tokyu", 90);
            RegisterSound("conductor_odakyu", 91);
            RegisterSound("conductor_tobu", 92);
            RegisterSound("conductor_test", 95);
            RegisterSound("driver_buzzer", 99);          // 運転士ブザー
        }

        private static void RegisterPanel(string key, int defaultIndex) { PanelMap.RegisterDefault(key, defaultIndex); }
        private static void RegisterSound(string key, int defaultIndex) { SoundMap.RegisterDefault(key, defaultIndex); }

        public static void Dispose() {
            StandAloneKey = KeyPosList.None;
            FDOpenSounds.Clear(); 
            FDCloseSounds.Clear();
            FDsoundenable = FDsinglelamp = false;
            Delay_FDclosed = 1.0;
            CurrentPanelIndex = 1023;
            MaxCurrentSpeed = 20.0;
            Current_abs = false;
            DriverBuzzerKey = Keys.None;
            SnowBrakeKey = Keys.None;
            InstrumentLightKey = Keys.None;
            SnowBrakePressure = 0.0;
            MaxTrainTypeCount = 19;
            Panel_LineDefOutput = 1023;
            Panel_RadiochannelOutput = 1023;

            odometer_Kmsymbol = 1023;
            odometer_Km100 = 1023;
            odometer_Km10 = 1023;
            odometer_Km1 = 1023;
            odometer_Km01 = 1023;
            odometer_Km001 = 1023;

            depart_melody = 1023;
            depart_announce = 1023;

            PanelMap.Clear();
            SoundMap.Clear();
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
