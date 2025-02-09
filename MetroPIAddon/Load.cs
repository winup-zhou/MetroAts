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
using CorePlugin = MetroAts.MetroAts;

namespace MetroPIAddon {
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

    public enum AtsSoundControlInstruction {
        Stop = -10000,      // Stop
        Play = 1,           // Play Once
        PlayLooping = 0,    // Play Repeatedly
        Continue = 2        // Continue
    }

    [Plugin(PluginType.VehiclePlugin)]
    public partial class MetroPIAddon : AssemblyPluginBase {
        private readonly INative Native;
        private static VehicleSpec vehicleSpec;
        private static bool isDoorOpen = false;
        private static bool StandAloneMode = false;
        private static CorePlugin corePlugin;

        private static bool isStopAnnounce;
        private static AtsSoundControlInstruction StopAnnounce, StopAnnounce_Confirmed, Tobu_DoorClosed, Door_poon,
            Conductor_buzzer1, Conductor_buzzer2, Conductor_buzzer3, Driver_buzzer, Lamp_SW_on, Lamp_SW_off, SnowBrake_on, Snow_brake_off;
        private static Sound FD_on, FD_off;

        private static int CurrentSta, NextSta, Destination, TrainNumber, TrainType, TrainRunningNumber;
        private static double StopLocation;
        private static TimeSpan DoorOpenTime, Conductorbuzzertime_global, Conductorbuzzertime_station;
        private static int doorSide, FDmode;

        public MetroPIAddon(PluginBuilder services) : base(services) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.Started += Initialize;
            Native.DoorClosed += DoorClosed;
            Native.DoorOpened += DoorOpened;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
            Native.BeaconPassed += SetBeaconData;

            BveHacker.ScenarioCreated += ScenarioCreated;
            BveHacker.MainFormSource.KeyDown += OnKeyDown;
            BveHacker.MainFormSource.KeyUp += OnKeyUp;

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
            Native.Started -= Initialize;
            Native.DoorClosed -= DoorClosed;
            Native.DoorOpened -= DoorOpened;
            Native.VehicleSpecLoaded -= SetVehicleSpec;
            Native.BeaconPassed -= SetBeaconData;

            BveHacker.ScenarioCreated -= ScenarioCreated;
            BveHacker.MainFormSource.KeyDown -= OnKeyDown;
            BveHacker.MainFormSource.KeyUp -= OnKeyUp;

            Plugins.AllPluginsLoaded -= OnAllPluginsLoaded;
        }
    }
}
