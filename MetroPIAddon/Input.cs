using BveEx.Extensions.Native.Input;
using BveEx.Extensions.Native;
using BveEx.PluginHost;
using BveEx.PluginHost.Input;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroPIAddon {
    public partial class MetroPIAddon : AssemblyPluginBase {

        private void Initialize(object sender, StartedEventArgs e) {
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {

            }
        }
        private void DoorOpened(object sender, EventArgs e) {
            isDoorOpen = true;
        }

        private void DoorClosed(object sender, EventArgs e) {
            isDoorOpen = false;
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
        }

        private void SetBeaconData(object sender, BeaconPassedEventArgs e) {
            
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

    }
}
