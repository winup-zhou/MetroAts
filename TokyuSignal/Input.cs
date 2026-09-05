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
            lastHandleOutputRefreshTime = TimeSpan.Zero;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            ATC.ResetAll();
            TokyuATS.ResetAll();
            if (sound[256] != (int)SoundPlayMode.Stop) sound[256] = (int)SoundPlayMode.Stop;
            panel[275] = 0;
            panel[278] = 0;
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                BrakeTriggered = false;
                SignalEnable = false;
            }
            UpdatePanelAndSound(panel, sound, TimeSpan.Zero);

        }

        private void DoorOpened(object sender, EventArgs e) {
            if (ATC.ATCEnable) ATC.DoorOpened();
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = SoundPlayMode.Play;
                TokyuATS.ResetBrake(state, handles);
            } else if (e.KeyName == AtsKeyName.S) {
                TokyuATS.ResetWarn();
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
