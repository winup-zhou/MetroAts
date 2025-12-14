using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CorePlugin = MetroAts.MetroAts;

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

        private LeverText leverText;
        private CorePlugin corePlugin;

        private static AtsSoundControlInstruction Sound_Keyin, Sound_Keyout, Sound_ResetSW, Sound_Switchover;

        private static bool SignalEnable = false;
        private static bool Keyin = false;
        private static bool StandAloneMode = true;
        private static bool isDoorOpen = false;
        private static bool BrakeTriggered = false;
        private static TimeSpan lastHandleOutputRefreshTime = TimeSpan.Zero;
        private static int lastBrakeNotch, lastPowerNotch;

        public TobuSignal(PluginBuilder builder) : base(builder) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.DoorClosed += DoorClosed;
            Native.Started += Initialize;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
            Native.SignalUpdated += SetSignal;

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
            Config.Dispose();

            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
            Native.DoorClosed -= DoorClosed;
            Native.Started -= Initialize;
            Native.VehicleSpecLoaded -= SetVehicleSpec;

            BveHacker.ScenarioCreated -= OnScenarioCreated;

            Plugins.AllPluginsLoaded -= OnAllPluginsLoaded;

            SignalEnable = false;
            Keyin = false;
            StandAloneMode = true;
            isDoorOpen = false;
            BrakeTriggered = false;
        }
    }
}
