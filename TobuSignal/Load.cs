using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TobuSignal {
    public enum AtsSoundControlInstruction {
        Stop = -10000,      // Stop
        Play = 1,           // Play Once
        PlayLooping = 0,    // Play Repeatedly
        Continue = 2        // Continue
    }

    public partial class TobuSignal : AssemblyPluginBase {
        private readonly INative Native;
        public static VehicleSpec vehicleSpec;
        public static SectionManager sectionManager;

        private AtsSoundControlInstruction Sound_Keyin, Sound_Keyout, Sound_ResetSW, Sound_Switchover;

        private static bool SignalEnable = false;
        private static bool Keyin = false;
        private static bool StandAloneMode = true;

        public TobuSignal(PluginBuilder builder) : base(builder) {
            Native = Extensions.GetExtension<INative>();
            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.DoorClosed += DoorClosed;
            Native.Started += Initialize;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            Plugins.AllPluginsLoaded += OnAllPluginsLoaded;
        }

        private void OnAllPluginsLoaded(object sender, EventArgs e) {
            //try {
            //    //mapPlugin = Plugins[PluginType.MapPlugin]["TGMT_WCU_Plugin"] as MapPlugin;
            //    StandAloneMode = false;
            //} catch (Exception ex) {
            //    StandAloneMode = true;
            //}
        }

        public override void Dispose() {
            
        }
    }
}
