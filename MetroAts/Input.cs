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

namespace MetroAts {
    public partial class MetroAts : AssemblyPluginBase {

        private void Initialize(object sender, StartedEventArgs e) {
            lastHandleOutputRefreshTime = TimeSpan.Zero;
            atoRunningMode = AtoModeList.Normal; // 每次驾驶开始复位为平常
            var panel = Native.AtsPanelArray;
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                ResetPositionsToDefault();
            }
            WriteKeyPosToPanel(panel);
            WriteSignalSWToPanel(panel);
        }
        private void DoorOpened(object sender, EventArgs e) {
            isDoorOpen = true;
        }

        private void DoorClosed(object sender, EventArgs e) {
            isDoorOpen = false;
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            if (e.KeyName == AtsKeyName.S) {
                isSpacePressed = false;
            }
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (Math.Abs(state.Speed) == 0) {
                if (e.KeyName == AtsKeyName.S) {
                    isSpacePressed = true;
                } else if (isSpacePressed && Config.atotascsw_enable && e.KeyName == AtsKeyName.J) {
                    // Space + 钥匙(J=前)：ATO 模式开关向回復侧步进（遅速→平常→回復，到头不循环）
                    StepAtoMode(+1);
                } else if (isSpacePressed && Config.atotascsw_enable && e.KeyName == AtsKeyName.I) {
                    // Space + 钥匙(I=后)：ATO 模式开关向遅速侧步进（回復→平常→遅速，到头不循环）
                    StepAtoMode(-1);
                } else if (e.KeyName == AtsKeyName.I && handles.ReverserPosition == ReverserPosition.N && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) {
                    MoveKey(-1);
                } else if (e.KeyName == AtsKeyName.J && handles.ReverserPosition == ReverserPosition.N && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) {
                    MoveKey(1);
                } else {
                    if (isSpacePressed && Config.atotascsw_enable) { //ATO/TASC 开关（Space + 信号选择开关 G/H）
                        if (e.KeyName == AtsKeyName.G && handles.BrakeNotch >= vehicleSpec.BrakeNotches) {
                            ToggleTASC(false);
                        } else if (e.KeyName == AtsKeyName.H && handles.BrakeNotch >= vehicleSpec.BrakeNotches) {
                            ToggleTASC(true);
                        }
                    } else {
                        if (e.KeyName == AtsKeyName.G && handles.BrakeNotch >= vehicleSpec.BrakeNotches) {
                            MoveSignalSW(-1);
                        } else if (e.KeyName == AtsKeyName.H && handles.BrakeNotch >= vehicleSpec.BrakeNotches) {
                            MoveSignalSW(1);
                        }
                    }
                }
                
                
            }
        }

        private void SetBeaconData(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            switch (e.Type) {
                case 42:
                    switch (e.Optional / 10) {
                        default: LineDef = KeyPosList.None; break;
                        case 1: LineDef = KeyPosList.Metro; break;
                        case 2: LineDef = KeyPosList.Tobu; break;
                        case 3: LineDef = KeyPosList.Tokyu; break;
                        case 4: LineDef = KeyPosList.Seibu; break;
                        case 5: LineDef = KeyPosList.Sotetsu; break;
                        case 6: LineDef = KeyPosList.JR; break;
                        case 7: LineDef = KeyPosList.Odakyu; break;
                        case 8: LineDef = KeyPosList.ToyoKosoku; break;
                    }
                    Direction = e.Optional % 10;
                    break;
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

    }
}
