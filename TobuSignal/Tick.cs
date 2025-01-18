using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public class TobuSignal : AssemblyPluginBase {
        private readonly INative Native;

        public TobuSignal(PluginBuilder builder) : base(builder) {
            Native = Extensions.GetExtension<INative>();
        }

        public override void Dispose() {
        }

        public override void Tick(TimeSpan elapsed) {
            AtsPlugin atsPlugin = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin;
            atsPlugin.AtsHandles.PowerNotch = 0;
            atsPlugin.AtsHandles.BrakeNotch = 0;
            atsPlugin.AtsHandles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            atsPlugin.AtsHandles.ReverserPosition = ReverserPosition.N;
        }
    }
}
