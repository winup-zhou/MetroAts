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
using System.Windows.Forms;

namespace JR_SotetsuSignal {
    public partial class JR_SotetsuSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            if (e.Type == 25) {
                if (e.Optional == 0) {
                    if (ATS_P.ATSEnable) ATS_P.SwitchToSN();
                    if (!ATS_SN.ATSEnable) ATS_SN.Init(state.Time);
                }
            }
            if (ATS_P.ATSEnable) ATS_P.BeaconPassed(state, e);
            if (ATS_SN.ATSEnable) ATS_SN.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            ATS_P.ResetAll();
            ATS_SN.ResetAll();
            if (e.DefaultBrakePosition == BrakePosition.Emergency && !StandAloneMode) {
                BrakeTriggered = false;
                Keyin = false;
                SignalEnable = false;
                sound[256] = (int)AtsSoundControlInstruction.Stop;
            }
            UpdatePanelAndSound(panel, sound);
        }

        private void DoorOpened(object sender, EventArgs e) {
            isDoorOpen = true;
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            if (ATS_P.ATSEnable) ATS_P.DoorOpened(state);
        }

        private void DoorClosed(object sender, EventArgs e) {
            isDoorOpen = false;
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            if (e.KeyName == AtsKeyName.S) {
                if (ATS_SN.ATSEnable) ATS_SN.ConfirmButtonUp();
            }
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var sound = Native.AtsSoundArray;
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = AtsSoundControlInstruction.Play;
                if (ATS_P.ATSEnable && state.Speed == 0) ATS_P.ResetBrake(handles);
                if (ATS_SN.ATSEnable) ATS_SN.ResetBrake(handles);
            } else if (e.KeyName == AtsKeyName.A1) {
                if (ATS_SN.ATSEnable) ATS_SN.ResetChime();
            } else if (e.KeyName == AtsKeyName.S) {
                if (ATS_SN.ATSEnable) ATS_SN.ResetWarn(handles);
            } else if (e.KeyName == AtsKeyName.B2) {
                if (ATS_P.ATSEnable && state.Speed == 0) ATS_P.BrakeOverride(state);
            }
            if (StandAloneMode && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1 && handles.ReverserPosition == ReverserPosition.N) {
                if (e.KeyName == AtsKeyName.I) {
                    Sound_Keyout = AtsSoundControlInstruction.Play;
                    Keyin = false;
                    BrakeTriggered = false;
                    SignalEnable = false;
                    sound[256] = (int)AtsSoundControlInstruction.Stop;

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
