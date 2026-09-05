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

namespace OdakyuSignal {
    public enum ATS_SW {
        OM_ATS = 0,
        Auto = 1,
        D_ATS_P = 2
    }

    public partial class OdakyuSignal : AssemblyPluginBase {
        private readonly INative Native;
        public static VehicleSpec vehicleSpec;
        public static SectionManager sectionManager;

        private CorePlugin corePlugin;

        private static ATS_SW ATS_Switch = ATS_SW.Auto;
        private static SoundPlayMode Sound_ResetSW;

        private static bool SignalEnable = false;
        private static bool BrakeTriggered = false;

        public OdakyuSignal(PluginBuilder builder) : base(builder) {
            Config.Load();

            Native = Extensions.GetExtension<INative>();
            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.Started += Initialize;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            Plugins.AllPluginsLoaded += OnAllPluginsLoaded;
        }

        private void OnAllPluginsLoaded(object sender, EventArgs e) {
            // MetroAts 核心为必需依赖：未加载则无法安全运行，直接报错（不再支持独立模式）
            corePlugin = Plugins.VehiclePlugins["MetroAtsCore"] as CorePlugin
                ?? throw new BveFileLoadException("未找到 MetroAts 核心插件 (MetroAtsCore)。OdakyuSignal 需要 MetroAts 核心插件。", "OdakyuSignal");
            corePlugin.RegisterDevice(this);
            corePlugin.RegisterPluginStateProvider(this);
        }

        public override void Dispose() {
            Config.Dispose();

            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
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
        }
    }
}
