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


namespace TobuSignal {
    public partial class TobuSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            if (TSP_ATS.ATSEnable) TSP_ATS.BeaconPassed(state, e);
            T_DATC.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            TSP_ATS.ResetAll();
            T_DATC.ResetAll();
            if (e.DefaultBrakePosition == BveTypes.ClassWrappers.BrakePosition.Emergency && !StandAloneMode) {
                BrakeTriggered = false;
                Keyin = false;
                SignalEnable = false;
            }
        }

        private void DoorOpened(object sender, EventArgs e) {
            if (TSP_ATS.ATSEnable) TSP_ATS.DoorOpened();
            if (T_DATC.ATCEnable) T_DATC.DoorOpened();
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
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = AtsSoundControlInstruction.Play;
                if (TSP_ATS.ATSEnable) TSP_ATS.ResetBrake(state, handles);
            }
            if (StandAloneMode && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1 && handles.ReverserPosition == BveTypes.ClassWrappers.ReverserPosition.N) {
                if (e.KeyName == AtsKeyName.I) {
                    Sound_Keyout = AtsSoundControlInstruction.Play;
                    BrakeTriggered = false;
                    Keyin = false;
                    SignalEnable = false;
                    T_DATC.ResetAll();
                    TSP_ATS.ResetAll();
                } else if (e.KeyName == AtsKeyName.J) {
                    Sound_Keyin = AtsSoundControlInstruction.Play;
                    Keyin = true;
                }
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

        private void SetSignal(object sender, SignalUpdatedEventArgs e) {
            T_DATC.SignalUpdated();
        }
        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }
    }
}
