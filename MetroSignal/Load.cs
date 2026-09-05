using BveEx.Extensions.Native;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorePlugin = MetroAts.MetroAts;

namespace MetroSignal {

    public partial class MetroSignal : AssemblyPluginBase {
        private readonly INative Native;
        public static VehicleSpec vehicleSpec;
        public static SectionManager sectionManager;

        private CorePlugin corePlugin;

        private static SoundPlayMode Sound_ResetSW;

        private static bool SignalEnable = false;
        private static bool BrakeTriggered = false;
        private static TimeSpan lastHandleOutputRefreshTime = TimeSpan.Zero, lastPanelOutputRefreshTime = TimeSpan.Zero;
        private static readonly int[] lastPanelOutput = new int[350];
        private static int lastBrakeNotch, lastPowerNotch;

        public MetroSignal(PluginBuilder builder) : base(builder) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.BeaconPassed += BeaconPassed;
            Native.Started += Initialize;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            Plugins.AllPluginsLoaded += OnAllPluginsLoaded;
            lastHandleOutputRefreshTime = TimeSpan.Zero;
        }

        private void OnAllPluginsLoaded(object sender, EventArgs e) {
            // MetroAts 核心为必需依赖：未加载则无法安全运行，直接报错（不再支持独立模式）
            corePlugin = Plugins.VehiclePlugins["MetroAtsCore"] as CorePlugin
                ?? throw new BveFileLoadException("未找到 MetroAts 核心插件 (MetroAtsCore)。MetroSignal 需要 MetroAts 核心插件。", "MetroSignal");
            corePlugin.RegisterDevice(this);
            corePlugin.RegisterPluginStateProvider(this);
        }

        public override void Dispose() {
            Config.Dispose();

            Native.BeaconPassed -= BeaconPassed;
            Native.Started -= Initialize;
            Native.VehicleSpecLoaded -= SetVehicleSpec;
            //Native.AtsKeys.AnyKeyPressed -= KeyDown;
            //Native.AtsKeys.AnyKeyReleased -= KeyUp;

            BveHacker.ScenarioCreated -= OnScenarioCreated;

            Plugins.AllPluginsLoaded -= OnAllPluginsLoaded;

            if (corePlugin != null) {
                corePlugin.UnregisterDevice(this);
                corePlugin.UnregisterPluginStateProvider(this);
            }

            SignalEnable = false;
            BrakeTriggered = false;
            lastBrakeNotch = lastPowerNotch = 0;
            lastHandleOutputRefreshTime = TimeSpan.Zero;
        }
    }
}
