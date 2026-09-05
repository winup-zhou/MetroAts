using BveEx.PluginHost.Plugins;
using BveEx.PluginHost;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using BveEx.Extensions.Native;
using System.Windows.Forms;

namespace MetroAts {
    public enum KeyPosList {
        Tokyu = 0,
        None = 1,
        Metro = 2,
        Tobu = 3,
        Seibu = 4,
        Sotetsu = 5,
        JR = 6,
        ToyoKosoku = 7,
        Odakyu = 8
    }

    public enum SignalSWList {
        Noset = 0,
        TokyuATS = 1,
        InDepot = 3,
        Sotetsu = 4,
        ATC = 5,
        SeibuATS = 6,
        Tobu = 7,
        JR = 8,
        WS_ATC = 9,
        ATP = 10,
        Odakyu = 2
    }

    /// <summary>
    /// ATO 运行模式（多模式 ATC 目标速度）。
    /// 模式选择开关为三位置线性：遅速 Slow → 平常 Normal → 回復 Recovery（到头不循环）。
    /// 模式经核心状态键 "ato_mode"（0/1/2 = 遅速/平常/回復）平行暴露，供 MetroAtsBridge 读取并下发给 autopilot。
    /// </summary>
    public enum AtoModeList {
        Slow = 0,       // 遅速（开关位置左端）
        Normal = 1,     // 平常
        Recovery = 2    // 回復（开关位置右端）
    }

    [Plugin(PluginType.VehiclePlugin)]
    public partial class MetroAts : AssemblyPluginBase {
        private readonly INative Native;
        private static VehicleSpec vehicleSpec;
        private LeverText leverText;
        private static bool isDoorOpen = false;

        private static bool isSpacePressed = false;
        private static bool isTASCenabled = false;
        private static AtoModeList atoRunningMode = AtoModeList.Normal;

        public static int NowKey;
        public static int NowSignalSW;
        private SoundPlayMode Sound_Keyin, Sound_Keyout, Sound_SignalSW;
        private static TimeSpan lastHandleOutputRefreshTime = TimeSpan.Zero;
        private static int lastBrakeNotch, lastPowerNotch;

        //Infomation that should be readable by sub-plugins
        public KeyPosList KeyPos {  get { return Config.KeyPosLists[NowKey]; } }
        public SignalSWList SignalSWPos {  get { return Config.SignalSWLists[NowSignalSW]; } }
        public bool SubPluginEnabled { set; get; } = false;
        public bool isATO_TASCenabled { get { return isTASCenabled; } }

        private static int Direction = 0; //0:未設定 1:上り 2:下り
        private static KeyPosList LineDef = KeyPosList.None;

        public MetroAts(PluginBuilder services) : base(services) {
            Config.Load();

            RegisterPluginStateProvider(this);

            Native = Extensions.GetExtension<INative>();
            Native.Started += Initialize;
            Native.DoorClosed += DoorClosed;
            Native.DoorOpened += DoorOpened;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
            Native.BeaconPassed += SetBeaconData;
            lastHandleOutputRefreshTime = TimeSpan.Zero;
        }

        public override void Dispose() {
            Config.Dispose();

            UnregisterPluginStateProvider(this);
            ClearPluginStates();
            ClearCoreStates();

            Native.Started -= Initialize;
            Native.DoorClosed -= DoorClosed;
            Native.DoorOpened -= DoorOpened;
            Native.VehicleSpecLoaded -= SetVehicleSpec;
            Native.BeaconPassed -= SetBeaconData;
            //Native.AtsKeys.AnyKeyPressed -= KeyDown;
            //Native.AtsKeys.AnyKeyReleased -= KeyUp;

            Direction = 0; //0:未設定 1:上り 2:下り
            LineDef = KeyPosList.None;
            isDoorOpen = false;
            isSpacePressed = false;
            isTASCenabled = false;
            lastBrakeNotch = lastPowerNotch = 0;
            lastHandleOutputRefreshTime = TimeSpan.Zero;
        }

        // ---------- ATO 运行模式（多模式 ATC 目标速度） ----------

        /// <summary>当前 ATO 运行模式（供 MetroAtsBridge 读取并下发 autopilot）。</summary>
        public AtoModeList ATORunningMode { get { return atoRunningMode; } }

        /// <summary>当前 ATO 运行模式数值（核心状态键 ato_mode = 0/1/2）。</summary>
        public int ATORunningModeValue { get { return (int)atoRunningMode; } }

        /// <summary>
        /// 按钥匙方向步进 ATO 模式选择开关（遅速→平常→回復，三位置线性，到头不循环）。
        /// delta&gt;0 向回復侧（Space+J），delta&lt;0 向遅速侧（Space+I）。
        /// 与 ATO/TASC 开关一致：模式实际变化时播放提示音（signalsw_sound）。
        /// </summary>
        private void StepAtoMode(int delta) {
            int next = (int)atoRunningMode + delta;
            if (next < 0) next = 0;
            if (next > 2) next = 2;
            if (next == (int)atoRunningMode) return;
            atoRunningMode = (AtoModeList)next;
            Sound_SignalSW = SoundPlayMode.Play;
        }

        /// <summary>模式显示文本（日文，驾驶台 leverText 用）。</summary>
        private static string AtoModeText() {
            switch (atoRunningMode) {
                case AtoModeList.Recovery: return "回復";
                case AtoModeList.Slow: return "遅速";
                default: return "平常";
            }
        }
    }
}
