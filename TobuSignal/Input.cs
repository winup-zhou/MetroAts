using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveEx.Extensions.Native.Input;
using BveEx.PluginHost.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuSignal {
    public partial class TobuSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            TSP_ATS.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            throw new NotImplementedException();
        }

        private void DoorOpened(object sender, EventArgs e) {
            TSP_ATS.DoorOpened();
        }

        private void DoorClosed(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            if (e.KeyName == AtsKeyName.B1) {
                TSP_ATS.ResetBrake(state, handles);
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }
    }
}
