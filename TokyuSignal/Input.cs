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
using TokyuSignal;
using System.Windows.Forms;

namespace TokyuSignal {
    public partial class TokyuSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            if (ATC.ATCEnable) ATC.BeaconPassed(state, e);
            TokyuATS.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            ATC.ResetAll();
            TokyuATS.ResetAll();
            if (sound[256] != (int)AtsSoundControlInstruction.Stop) sound[256] = (int)AtsSoundControlInstruction.Stop;
            panel[275] = 0;
            panel[278] = 0;
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                BrakeTriggered = false;
                Keyin = false;
                SignalEnable = false;
                if (StandAloneMode) {
                    for (int i = 0; i < Config.SignalSWLists.Count; ++i) {
                        if (Config.SignalSWLists[i] == SignalSWListStandAlone.Noset) {
                            NowSignalSW = i;
                            break;
                        }
                    }
                }
            }
            UpdatePanelAndSound(panel, sound);

        }

        private void DoorOpened(object sender, EventArgs e) {
            isDoorOpen = true;
            if (ATC.ATCEnable) ATC.DoorOpened();
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
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = AtsSoundControlInstruction.Play;
                TokyuATS.ResetBrake(state, handles);
            } else if (e.KeyName == AtsKeyName.S) {
                TokyuATS.ResetWarn();
            }
            if (StandAloneMode && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) {
                if (e.KeyName == AtsKeyName.I && handles.ReverserPosition == ReverserPosition.N) {
                    Sound_Keyout = AtsSoundControlInstruction.Play;
                    Keyin = false;
                    BrakeTriggered = false;
                    SignalEnable = false;
                    ATC.ResetAll();
                    TokyuATS.ResetAll();
                    if (sound[256] != (int)AtsSoundControlInstruction.Stop) sound[256] = (int)AtsSoundControlInstruction.Stop;
                    panel[275] = 0;
                    panel[278] = 0;
                    UpdatePanelAndSound(panel, sound);     
                } else if (e.KeyName == AtsKeyName.J && handles.ReverserPosition == ReverserPosition.N) {
                    Sound_Keyin = AtsSoundControlInstruction.Play;
                    Keyin = true;
                } else if (e.KeyName == AtsKeyName.G && NowSignalSW > 0) {
                    NowSignalSW--;
                    Sound_SignalSW = AtsSoundControlInstruction.Play;
                } else if (e.KeyName == AtsKeyName.H && NowSignalSW < Config.SignalSWLists.Count - 1) {
                    NowSignalSW++;
                    Sound_SignalSW = AtsSoundControlInstruction.Play;
                }
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

        private void SetSignal(object sender, SignalUpdatedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            if (TokyuATS.ATSEnable) TokyuATS.SignalUpdated(state, e);
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }
    }
}
