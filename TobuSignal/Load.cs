using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuSignal {
    public partial class TobuSignal : AssemblyPluginBase {
        private readonly INative Native;
        private int[] panel;
        public static VehicleSpec vehicleSpec;

        public TobuSignal(PluginBuilder builder) : base(builder) {
            Native = Extensions.GetExtension<INative>();
            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.DoorClosed += DoorClosed;
            Native.Started += Initialize;
            Native.AtsKeys.AnyKeyPressed += KeyDown;
            Native.AtsKeys.AnyKeyReleased += KeyUp;
            Native.VehicleSpecLoaded += SetVehicleSpec;
        }

        public override void Dispose() {

        }
    }
}
