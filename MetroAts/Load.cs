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

    public enum AtsSoundControlInstruction {
        Stop = -10000,      // Stop
        Play = 1,           // Play Once
        PlayLooping = 0,    // Play Repeatedly
        Continue = 2        // Continue
    }

    [Plugin(PluginType.VehiclePlugin)]
    public partial class MetroAts : AssemblyPluginBase {
        private readonly INative Native;
        private static VehicleSpec vehicleSpec;
        private LeverText leverText;
        private static bool isDoorOpen = false;

        private static bool isSpacePressed = false;
        private static bool isTASCenabled = false;

        public static int NowKey;
        public static int NowSignalSW;
        private static int NoneKeyPos;
        private AtsSoundControlInstruction Sound_Keyin, Sound_Keyout, Sound_SignalSW;
        private static TimeSpan lastHandleOutputRefreshTime = TimeSpan.Zero;

        //Infomation that should be readable by sub-plugins
        public KeyPosList KeyPos {  get { return Config.KeyPosLists[NowKey]; } }
        public SignalSWList SignalSWPos {  get { return Config.SignalSWLists[NowSignalSW]; } }
        public bool SubPluginEnabled { set; get; } = false;
        public bool isATO_TASCenabled { get { return isTASCenabled; } }

        private static int Direction = 0; //0:未設定 1:上り 2:下り
        private static KeyPosList LineDef = KeyPosList.None;

        public MetroAts(PluginBuilder services) : base(services) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.Started += Initialize;
            Native.DoorClosed += DoorClosed;
            Native.DoorOpened += DoorOpened;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
            Native.BeaconPassed += SetBeaconData;
        }

        public override void Dispose() {
            Config.Dispose();

            Native.Started -= Initialize;
            Native.DoorClosed -= DoorClosed;
            Native.DoorOpened -= DoorOpened;
            Native.VehicleSpecLoaded -= SetVehicleSpec;
            Native.BeaconPassed -= SetBeaconData;

            Direction = 0; //0:未設定 1:上り 2:下り
            LineDef = KeyPosList.None;
            isDoorOpen = false;
            isSpacePressed = false;
            isTASCenabled = false;
        }
    }
}
