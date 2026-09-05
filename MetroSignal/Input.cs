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
using MetroSignal;
using System.Windows.Forms;

namespace MetroSignal {
    public partial class MetroSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            if (CS_ATC.ATCEnable) CS_ATC.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            lastHandleOutputRefreshTime = TimeSpan.Zero;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            CS_ATC.ResetAll();
            WS_ATC.ResetAll();
            Config.SoundMap.WriteSound(sound, "Warning", (int)SoundPlayMode.Stop);
            Config.PanelMap.WritePanel(panel, "ATC_Depot", 0);
            Config.PanelMap.WritePanel(panel, "ATC_Noset", 0);
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                BrakeTriggered = false;
                SignalEnable = false;
            }
            UpdatePanelAndSound(panel, sound, TimeSpan.Zero);

        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = SoundPlayMode.Play;
                WS_ATC.ResetBrake(state, handles);
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
