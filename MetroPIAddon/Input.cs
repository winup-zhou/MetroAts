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
            isStopAnnounce = false;
        }

        private void DoorOpened(object sender, EventArgs e) {
            var state = Native.VehicleState;
            isDoorOpen = true;
            isStopAnnounce = false;
            Door_poon = AtsSoundControlInstruction.PlayLooping;
            DoorOpenTime = state.Time;
        }

        private void DoorClosed(object sender, EventArgs e) {
            isDoorOpen = false;
            Door_poon = AtsSoundControlInstruction.Stop;
            if (StandAloneMode) {
                if (Config.StandAloneKey == KeyPosList.Tobu) {
                    Tobu_DoorClosed = AtsSoundControlInstruction.Play;
                }
            } else {
                if (corePlugin.KeyPos == MetroAts.KeyPosList.Tobu) {
                    Tobu_DoorClosed = AtsSoundControlInstruction.Play;
                }
            }
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
        
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {

        }

        private void SetBeaconData(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            switch (e.Type) {
                case 9:
                    if (StandAloneMode) {
                        if(Config.StandAloneKey == KeyPosList.Tokyu 
                            || Config.StandAloneKey == KeyPosList.Metro
                            || Config.StandAloneKey == KeyPosList.ToyoKosoku) {
                            isStopAnnounce = true;
                        }
                    } else {
                        if (corePlugin.KeyPos == MetroAts.KeyPosList.Tokyu 
                            || corePlugin.KeyPos == MetroAts.KeyPosList.Metro
                            || corePlugin.KeyPos == MetroAts.KeyPosList.ToyoKosoku) {
                            isStopAnnounce = true;
                        }
                    }
                    break;
                case 22://駅名表示設定
                    CurrentSta = e.Optional / 1000;
                    NextSta = e.Optional % 1000;
                    break;
                case 23://ドア開側情報
                    doorSide = e.Optional;
                    break;
                case 24://連動表示灯
                    FDmode = e.Optional;
                    break;
                case 25://停止位置目標
                    StopLocation = state.Location + 11;
                    break;
                case 26://種別/行先/運番表示
                    TrainRunningNumber = e.Optional % 100;
                    Destination = (e.Optional / 100) % 100;
                    TrainType = e.Optional / 10000;
                    break;
                case 33://車掌電鈴遅延
                case 34://列車番号表示
                    TrainNumber = e.Optional;
                    break;
                case 35://定点音鳴動
                    break;
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

        private void ScenarioCreated(ScenarioCreatedEventArgs e) {
            
        }

        static int[] pow10 = new int[] { 1, 10, 100, 1000, 10000, 100000, 1000000 };
        static int D(int src, int digit) {
            if (pow10[digit] > src) {
                return 10;
            } else if (digit == 0 && src == 0) {
                return 0;
            } else {
                return src / pow10[digit] % 10;
            }
        }

    }
}
