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
        Tokyu = -1,
        None = 0,
        Metro = 1,
        Tobu = 2,
        Seibu = 3,
        Sotetsu = 4,
        JR = 5,
        ToyoKosoku = 6
    }

    public enum SignalSWList {
        Noset = 0,
        InDepot = 1,
        ATC = 2,
        SeibuATS = 3,
        Tobu = 4,
        Sotetsu = 5,
        JR = 6,
        TokyuATS = 7,
        WS_ATC = 8,
        ATP = 9
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

        public static int NowKey;
        public static int NowSignalSW;
        private AtsSoundControlInstruction Sound_Keyin, Sound_Keyout, Sound_SignalSW;

        //Infomation that should be readable by sub-plugins
        public KeyPosList KeyPos {  get { return Config.KeyPosLists[NowKey]; } }
        public SignalSWList SignalSWPos {  get { return Config.SignalSWLists[NowSignalSW]; } } 

        public MetroAts(PluginBuilder services) : base(services) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.Started += Initialize;
            Native.DoorClosed += DoorClosed;
            Native.DoorOpened += DoorOpened;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
        }

        public override void Dispose() {
            Native.Started -= Initialize;
            Native.DoorClosed -= DoorClosed;
            Native.DoorOpened -= DoorOpened;
            Native.VehicleSpecLoaded -= SetVehicleSpec;
        }
    }
}
