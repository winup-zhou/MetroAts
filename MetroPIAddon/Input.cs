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
using static System.Windows.Forms.AxHost;

namespace MetroPIAddon {
    public partial class MetroPIAddon : AssemblyPluginBase {

        private void Initialize(object sender, StartedEventArgs e) {
            var panel = Native.AtsPanelArray;
            isStopAnnounce = false;
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                Keyin = false;
                panel[167] = CurrentSta;
                panel[168] = panel[169] = 0;
                panel[62] = D(TrainNumber / 100, 3);
                panel[63] = D(TrainNumber / 100, 2);
                panel[64] = D(TrainNumber / 100, 1);
                panel[65] = D(TrainNumber / 100, 0);
                panel[68] = TrainNumber % 100;
                panel[151] = panel[152] = TrainType;
                panel[153] = D(TrainRunningNumber, 1);
                panel[154] = D(TrainRunningNumber, 0);
                panel[172] = Destination;
                UpdateRequested = false;
            }
        }

        private void DoorOpened(object sender, EventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            isDoorOpen = true;
            isStopAnnounce = false;
            Door_poon = AtsSoundControlInstruction.PlayLooping;
            DoorOpenTime = state.Time;
            DoorClosedTime = TimeSpan.Zero;
        }

        private void DoorClosed(object sender, EventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            isDoorOpen = false;
            Door_poon = AtsSoundControlInstruction.Stop;
            DoorClosedTime = state.Time;
            DoorOpenTime = TimeSpan.Zero;
            NeedConductorBuzzer = true;
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
            if (handles.BrakeNotch == vehicleSpec.BrakeNotches + 1 && handles.ReverserPosition == ReverserPosition.N) {
                if (StandAloneMode && e.KeyName == AtsKeyName.I) {
                    Keyin = false;
                } else if (StandAloneMode && e.KeyName == AtsKeyName.J) {
                    Keyin = true;
                } else if (e.KeyName == AtsKeyName.C1 && TrainType > 0) {
                    --TrainType;
                    lastTrainType = TrainType;
                } else if (e.KeyName == AtsKeyName.C2 && TrainType < Config.MaxTrainTypeCount) {
                    ++TrainType;
                    lastTrainType = TrainType;
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Config.DriverBuzzerKey) {
                Driver_buzzer = AtsSoundControlInstruction.Stop;
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Config.DriverBuzzerKey) {
                Driver_buzzer = AtsSoundControlInstruction.PlayLooping;
            } else if (e.KeyCode == Config.SnowBrakeKey) {
                if (Snowbrake) SnowBrake_off = AtsSoundControlInstruction.Play;
                else SnowBrake_on = AtsSoundControlInstruction.Play;
                Snowbrake = !Snowbrake;
            } else if (e.KeyCode == Config.InstrumentLightKey) {
                if (InstrumentLight) Lamp_SW_off = AtsSoundControlInstruction.Play;
                else Lamp_SW_on = AtsSoundControlInstruction.Play;
                InstrumentLight = !InstrumentLight;
            }
        }

        private void SetBeaconData(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            switch (e.Type) {
                case 9:
                    if (StandAloneMode) {
                        if (Config.StandAloneKey == KeyPosList.Tokyu
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
                case 48://駅名表示設定
                    CurrentSta = e.Optional / 1000;
                    NextSta = e.Optional % 1000;
                    break;
                //case 49://ドア開側
                //    doorSide = e.Optional;
                //    break;
                case 14://連動表示灯
                    FDmode = e.Optional;
                    break;
                //case 17://停止位置目標
                //    StopLocation = state.Location + 11;
                //    break;
                case 50://種別/行先/運番表示
                    TrainRunningNumber = e.Optional % 100;
                    Destination = (e.Optional / 100) % 1000;
                    lastTrainType = TrainType;
                    TrainType = e.Optional / 100000;
                    UpdateRequested = true;
                    break;
                case 33://車掌電鈴遅延
                    if (e.Optional < 100) {
                        if (e.Optional != 99) Conductorbuzzertime_station = new TimeSpan(0, 0, e.Optional);
                        else Conductorbuzzertime_station = TimeSpan.MinValue;
                    } else {
                        Conductorbuzzertime_station = TimeSpan.MinValue;
                        if (e.Optional != 199) Conductorbuzzertime_global = new TimeSpan(0, 0, e.Optional);
                        else Conductorbuzzertime_global = TimeSpan.Zero;
                    }
                    break;
                case 34://列車番号表示
                    TrainNumber = e.Optional;
                    break;
                case 41://定点音鳴動
                    switch (e.Optional) {
                        case 0://Tokyu
                            Conductorbuzzer_Tokyu = AtsSoundControlInstruction.Play;
                            break;
                        case 1://Odakyu
                            Conductorbuzzer_Odakyu = AtsSoundControlInstruction.Play;
                            break;
                        case 2://Tobu
                            Conductorbuzzer_Tobu = AtsSoundControlInstruction.Play;
                            break;
                        case 5://Test
                            Conductorbuzzer_Test = AtsSoundControlInstruction.Play;
                            break;
                    }
                    break;
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
                case 46://FD OPEN SOUND CHANGE
                    if (e.Optional < Config.FDOpenSounds.Count && e.Optional >= 0) {
                        if (MapSoundList is null) FDOpenSoundIndex = e.Optional;
                        else FDOpenSound = MapSoundList[Config.FDOpenSounds[e.Optional]];
                    }
                    break;
                case 47://FD CLOSE SOUND CHANGE
                    if (e.Optional < Config.FDCloseSounds.Count && e.Optional >= 0) {
                        if (MapSoundList is null) FDCloseSoundIndex = e.Optional;
                        else FDCloseSound = MapSoundList[Config.FDCloseSounds[e.Optional]];
                    }
                    break;
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

        private void ScenarioCreated(ScenarioCreatedEventArgs e) {
            MapSoundList = e.Scenario.Map.Sounds;
            MapStationList = e.Scenario.Map.Stations;
            FDOpenSound = MapSoundList[Config.FDOpenSounds[FDOpenSoundIndex]];
            FDCloseSound = MapSoundList[Config.FDCloseSounds[FDCloseSoundIndex]];
            vehicle = e.Scenario.Vehicle;
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
