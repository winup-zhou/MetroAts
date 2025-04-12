using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorePlugin = MetroAts.MetroAts;

namespace OdakyuSignal {
    public enum ATS_SW {
        OM_ATS = 0,
        Auto = 1,
        D_ATS_P = 2
    }

    public enum AtsSoundControlInstruction {
        Stop = -10000,      // Stop
        Play = 1,           // Play Once
        PlayLooping = 0,    // Play Repeatedly
        Continue = 2        // Continue
    }

    public partial class OdakyuSignal : AssemblyPluginBase {
        private readonly INative Native;
        public static VehicleSpec vehicleSpec;
        public static SectionManager sectionManager;

        private LeverText leverText;
        private CorePlugin corePlugin;

        private static ATS_SW ATS_Switch = ATS_SW.Auto;
        private static AtsSoundControlInstruction Sound_Keyin, Sound_Keyout, Sound_ResetSW;

        private static bool SignalEnable = false;
        private static bool Keyin = false;
        private static bool StandAloneMode = true;
        private static bool isDoorOpen = false;
        private static bool BrakeTriggered = false;

        public OdakyuSignal(PluginBuilder builder) : base(builder) {
            Config.Load();

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
            try {
                corePlugin = Plugins.VehiclePlugins["MetroAtsCore"] as CorePlugin;
                StandAloneMode = false;
            } catch (Exception ex) {
                StandAloneMode = true;
            }
        }

        public override void Dispose() {
            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
            Native.DoorClosed -= DoorClosed;
            Native.Started -= Initialize;
            Native.VehicleSpecLoaded -= SetVehicleSpec;

            BveHacker.ScenarioCreated -= OnScenarioCreated;

            Plugins.AllPluginsLoaded -= OnAllPluginsLoaded;
        }
    }
}
