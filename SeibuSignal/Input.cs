using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveEx.Extensions.Native.Input;
using BveEx.PluginHost.Input;
using BveEx.PluginHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BveTypes.ClassWrappers;
using SeibuSignal;
using System.Windows.Forms;

namespace SeibuSignal {
    public partial class SeibuSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            SeibuATS.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            SeibuATS.ResetAll();
        }

        private void DoorOpened(object sender, EventArgs e) {
            if(SeibuATS.ATSEnable)SeibuATS.DoorOpened();
        }

        private void DoorClosed(object sender, EventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = AtsSoundControlInstruction.Play;
                SeibuATS.ConfirmEB(state, handles);
            }
            if (StandAloneMode && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1 && handles.ReverserPosition == ReverserPosition.N) {
                if (e.KeyName == AtsKeyName.I) {
                    Sound_Keyout = AtsSoundControlInstruction.Play;
                    Keyin = false;
                    SignalEnable = false;
                } else if (e.KeyName == AtsKeyName.J) {
                    Sound_Keyin = AtsSoundControlInstruction.Play;
                    Keyin = true;
                }
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }
    }
}
