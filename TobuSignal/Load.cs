using BveEx.Extensions.Native;
using BveEx.PluginHost;
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

        private CorePlugin corePlugin;

        private static AtsSoundControlInstruction Sound_ResetSW, Sound_Switchover;

        private static bool SignalEnable = false;
        private static bool BrakeTriggered = false;
        private static TimeSpan lastHandleOutputRefreshTime = TimeSpan.Zero, lastPanelOutputRefreshTime = TimeSpan.Zero;
        private static readonly int[] lastPanelOutput = new int[350]; 

        private static int lastBrakeNotch, lastPowerNotch;

        public TobuSignal(PluginBuilder builder) : base(builder) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.Started += Initialize;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
            Native.SignalUpdated += SetSignal;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            Plugins.AllPluginsLoaded += OnAllPluginsLoaded;
            lastHandleOutputRefreshTime = TimeSpan.Zero;
        }

        private void OnAllPluginsLoaded(object sender, EventArgs e) {
            // MetroAts 核心为必需依赖：未加载则无法安全运行，直接报错（不再支持独立模式）
            corePlugin = Plugins.VehiclePlugins["MetroAtsCore"] as CorePlugin
                ?? throw new BveFileLoadException("未找到 MetroAts 核心插件 (MetroAtsCore)。TobuSignal 需要 MetroAts 核心插件。", "TobuSignal");
            corePlugin.RegisterDevice(this);
        }

        public override void Dispose() {
            Config.Dispose();

            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
            Native.Started -= Initialize;
            Native.VehicleSpecLoaded -= SetVehicleSpec;
            //Native.AtsKeys.AnyKeyPressed -= KeyDown;
            //Native.AtsKeys.AnyKeyReleased -= KeyUp;
            Native.SignalUpdated -= SetSignal;

            BveHacker.ScenarioCreated -= OnScenarioCreated;

            Plugins.AllPluginsLoaded -= OnAllPluginsLoaded;

            if (corePlugin != null) corePlugin.UnregisterDevice(this);

            SignalEnable = false;
            BrakeTriggered = false;
            lastBrakeNotch = lastPowerNotch = 0;
            lastHandleOutputRefreshTime = TimeSpan.Zero;
            lastPanelOutputRefreshTime = TimeSpan.Zero;
        }
    }
}
