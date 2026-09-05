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
using OdakyuSignal;
using System.Windows.Forms;

namespace OdakyuSignal {
    public partial class OdakyuSignal : AssemblyPluginBase {

        private void BeaconPassed(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            OM_ATS.BeaconPassed(state, e);
            D_ATS_P.BeaconPassed(state, e);
        }

        private void Initialize(object sender, StartedEventArgs e) {
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            OM_ATS.ResetAll();
            D_ATS_P.ResetAll();
            Config.SoundMap.WriteSound(sound, "Warning", (int)SoundPlayMode.Stop);
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                BrakeTriggered = false;
                SignalEnable = false;
            }
            UpdatePanelAndSound(panel, sound);
        }

        private void DoorOpened(object sender, EventArgs e) {
            OM_ATS.DoorOpened();
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (e.KeyName == AtsKeyName.B1) {
                Sound_ResetSW = SoundPlayMode.Play;
                OM_ATS.ConfirmEB(state, handles);
                D_ATS_P.ConfirmEB(state, handles);
            } else if (e.KeyName == AtsKeyName.B2) {
                // 手动/自动切换开关（现实中驾驶台有 OM-ATS / D-ATS-P 与自动的切换）：
                // Auto → OM-ATS → D-ATS-P → Auto 循环
                ATS_Switch = (ATS_SW)(((int)ATS_Switch + 1) % 3);
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
