using BveEx.Extensions.PreTrainPatch;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MetroPIAddon {
    public partial class MetroPIAddon : AssemblyPluginBase {
        public override void Tick(TimeSpan elapsed) {
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            Conductorbuzzer_Depart = AtsSoundControlInstruction.Continue;

            if (isStopAnnounce) {
                if (StopAnnounce == AtsSoundControlInstruction.Stop && StopAnnounce_Confirmed != AtsSoundControlInstruction.PlayLooping) {
                    StopAnnounce = AtsSoundControlInstruction.PlayLooping;
                }
                if (handles.BrakeNotch > 0 && StopAnnounce != AtsSoundControlInstruction.Stop) {
                    StopAnnounce = AtsSoundControlInstruction.Stop;
                    StopAnnounce_Confirmed = AtsSoundControlInstruction.PlayLooping;
                }
            } else {
                StopAnnounce = StopAnnounce_Confirmed = AtsSoundControlInstruction.Stop;
            }

            int pointer = 0;
            while (MapStationList[pointer].Location - 25 < state.Location) {
                pointer++;
                if (pointer >= MapStationList.Count) {
                    pointer = MapStationList.Count - 1;
                    break;
                }
            }
            var currentStation = MapStationList[MapStationList.Count - 1].Location - 25 < state.Location ? MapStationList[MapStationList.Count - 1] as Station :
                MapStationList[pointer > 0 ? pointer - 1 : 0] as Station;

            for (int i = OdometerBeacons.Count - 1; i >= 0; i--) {
                if (OdometerBeacons[i].Location <= state.Location) {
                    var targetBeacon = OdometerBeacons[i];
                    if (lastOdometerBeacon != targetBeacon) {
                        lastisOdometerPlus = isOdometerPlus;
                        lastisOdometerHasMinus = isOdometerHasMinus;
                        lastBaseOdometer = BaseOdometer;
                        isOdometerPlus = Math.Abs(targetBeacon.SendData) % 10 > 0;
                        isOdometerHasMinus = Math.Abs(targetBeacon.SendData) / 10 % 10 > 0;
                        BaseOdometer = Convert.ToInt32(targetBeacon.SendData / 100) - targetBeacon.Location;
                        lastOdometerBeacon = targetBeacon;
                    }
                    break;
                }
            }

            if (FDmode == 0) {
                panel[155] = 0;
                panel[181] = panel[182] = 0;
                panel[193] = 0;
            } else if (FDmode == 1) {
                panel[155] = 1;
                var leftDoorState = vehicle.Doors.GetSide(DoorSide.Left).CarDoors[0].State;
                var rightDoorState = vehicle.Doors.GetSide(DoorSide.Right).CarDoors[0].State;
                var doorCloseTimes = TimeSpan.FromMilliseconds(vehicle.Doors.StandardCloseTime) + TimeSpan.FromSeconds(Config.Delay_FDclosed);
                if (state.Location > currentStation.MinStopPosition && state.Location < currentStation.MaxStopPosition) {
                    if (!isDoorOpen && state.Time > TimeSpan.FromSeconds(Config.Delay_FDclosed) + DoorClosedTime) {
                        if (Config.FDsinglelamp) {
                            panel[181] = panel[182] = state.Time.TotalMilliseconds % 1000 < 500 ? 1 : 0;
                        } else {
                            if (currentStation.DoorSide == DoorSide.Left) {
                                panel[181] = state.Time.TotalMilliseconds % 1000 < 500 ? 1 : 0;
                                panel[182] = 1;
                            } else if (currentStation.DoorSide == DoorSide.Right) {
                                panel[181] = 1;
                                panel[182] = state.Time.TotalMilliseconds % 1000 < 500 ? 1 : 0;
                            } else {
                                panel[181] = panel[182] = state.Time.TotalMilliseconds % 1000 < 500 ? 1 : 0;
                            }
                        }
                    } else if (isDoorOpen) {
                        if (Config.FDsinglelamp) {
                            panel[181] = panel[182] = 0;
                        } else {
                            if (currentStation.DoorSide == DoorSide.Left) {
                                panel[181] = 0;
                                panel[182] = 1;
                            } else if (currentStation.DoorSide == DoorSide.Right) {
                                panel[181] = 1;
                                panel[182] = 0;
                            } else {
                                panel[181] = panel[182] = 0;
                            }
                        }
                    }

                    try {
                        if (lastLeftDoorState == DoorState.Close && leftDoorState == DoorState.Open) {
                            if (Config.FDsoundenable) FDOpenSound.Play(1, 1, 100);
                            FDOpenTime = state.Time + TimeSpan.FromSeconds(3);
                        } else if (lastLeftDoorState == DoorState.Open && leftDoorState == DoorState.Close) {
                            if (Config.FDsoundenable) FDCloseSound.Play(1, 1, 100);
                            FDCloseTime = state.Time + doorCloseTimes;
                        }
                        if (lastRightDoorState == DoorState.Close && rightDoorState == DoorState.Open) {
                            if (Config.FDsoundenable) FDOpenSound.Play(1, 1, 100);
                            FDOpenTime = state.Time + TimeSpan.FromSeconds(3);
                        } else if (lastRightDoorState == DoorState.Open && rightDoorState == DoorState.Close) {
                            if (Config.FDsoundenable) FDCloseSound.Play(1, 1, 100);
                            FDCloseTime = state.Time + doorCloseTimes;
                        }
                    } catch {
                        Config.FDsoundenable = false;
                    }
                    
                    if (StandAloneMode) {
                        if (Keyin && state.Speed < 15 && CCTVenable) {
                            if (FDCloseTime != TimeSpan.Zero) {
                                FDOpenTime = TimeSpan.Zero;
                                if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < doorCloseTimes.TotalSeconds && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 5) {
                                    panel[193] = 7;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 5 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 4) {
                                    panel[193] = 8;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 4 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 3) {
                                    panel[193] = 9;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 3 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 2) {
                                    panel[193] = 10;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 2 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6)) {
                                    panel[193] = 11;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= 0) {
                                    panel[193] = 12;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < 0) {
                                    panel[193] = 2;
                                    FDCloseTime = TimeSpan.Zero;
                                }
                            } else if (FDOpenTime != TimeSpan.Zero) {
                                FDCloseTime = TimeSpan.Zero;
                                if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 2.5 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 2) {
                                    panel[193] = 2;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 2 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 1.5) {
                                    panel[193] = 3;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 1.5 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 1) {
                                    panel[193] = 4;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 1 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 0.5) {
                                    panel[193] = 5;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 0.5 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 0) {
                                    panel[193] = 6;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 0) {
                                    panel[193] = 7;
                                    FDOpenTime = TimeSpan.Zero;
                                }
                            } else if (FDOpenTime == TimeSpan.Zero && FDCloseTime == TimeSpan.Zero) {
                                if (!isDoorOpen && state.Time > TimeSpan.FromSeconds(Config.Delay_FDclosed) + DoorClosedTime) panel[193] = 2;
                                else if (isDoorOpen) panel[193] = 7;
                            }
                        } else panel[193] = 0;
                    } else {
                        if (corePlugin.KeyPos != MetroAts.KeyPosList.None && state.Speed < 15 && CCTVenable) {
                            if (FDCloseTime != TimeSpan.Zero) {
                                FDOpenTime = TimeSpan.Zero;
                                if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < doorCloseTimes.TotalSeconds && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 5) {
                                    panel[193] = 7;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 5 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 4) {
                                    panel[193] = 8;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 4 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 3) {
                                    panel[193] = 9;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 3 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 2) {
                                    panel[193] = 10;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 2 && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6)) {
                                    panel[193] = 11;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) && FDCloseTime.TotalSeconds - state.Time.TotalSeconds >= 0) {
                                    panel[193] = 12;
                                } else if (FDCloseTime.TotalSeconds - state.Time.TotalSeconds < 0) {
                                    panel[193] = 2;
                                    FDCloseTime = TimeSpan.Zero;
                                }
                            } else if (FDOpenTime != TimeSpan.Zero) {
                                FDCloseTime = TimeSpan.Zero;
                                if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 2.5 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 2) {
                                    panel[193] = 2;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 2 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 1.5) {
                                    panel[193] = 3;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 1.5 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 1) {
                                    panel[193] = 4;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 1 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 0.5) {
                                    panel[193] = 5;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 0.5 && FDOpenTime.TotalSeconds - state.Time.TotalSeconds >= 0) {
                                    panel[193] = 6;
                                } else if (FDOpenTime.TotalSeconds - state.Time.TotalSeconds < 0) {
                                    panel[193] = 7;
                                    FDOpenTime = TimeSpan.Zero;
                                }
                            } else if (FDOpenTime == TimeSpan.Zero && FDCloseTime == TimeSpan.Zero) {
                                if (!isDoorOpen && state.Time > TimeSpan.FromSeconds(Config.Delay_FDclosed) + DoorClosedTime) panel[193] = 2;
                                else if (isDoorOpen) panel[193] = 7;
                            }
                        } else panel[193] = 0;
                    }

                } else {
                    panel[181] = panel[182] = 1;
                    if (StandAloneMode && Keyin) {
                        if (Math.Abs(state.Location - currentStation.Location) < 10 && state.Speed < 15 && CCTVenable) {
                            panel[193] = 1;
                        } else {
                            panel[193] = 0;
                        }
                    } else if (corePlugin.KeyPos != MetroAts.KeyPosList.None) {
                        if (Math.Abs(state.Location - currentStation.Location) < 10 && state.Speed < 15 && CCTVenable) {
                            panel[193] = 1;
                        } else {
                            panel[193] = 0;
                        }
                    }

                }
                lastLeftDoorState = leftDoorState;
                lastRightDoorState = rightDoorState;
            } else if (FDmode == 2) {
                panel[155] = 2;
                panel[181] = panel[182] = 0;
                panel[193] = 0;
            }



            if (isDoorOpen) {
                if (state.Time > DoorOpenTime + new TimeSpan(0, 0, 2)) {
                    panel[167] = CurrentSta;
                    panel[168] = panel[169] = 0;
                    lastisOdometerPlus = isOdometerPlus;
                    lastisOdometerHasMinus = isOdometerHasMinus;
                    lastBaseOdometer = BaseOdometer;
                }
                if (state.Time > DoorOpenTime + new TimeSpan(0, 0, 10)) {
                    panel[62] = D(TrainNumber / 100, 3);
                    panel[63] = D(TrainNumber / 100, 2);
                    panel[64] = D(TrainNumber / 100, 1);
                    panel[65] = D(TrainNumber / 100, 0);
                    panel[68] = TrainNumber % 100;
                    panel[151] = panel[152] = TrainType;
                    panel[153] = D(TrainRunningNumber, 1);
                    panel[154] = D(TrainRunningNumber, 0);
                    panel[172] = Destination;
                    if (UpdateRequested) {
                        UpdateRequested = false;
                        lastTrainType = TrainType;
                    }
                } else {
                    panel[151] = panel[152] = lastTrainType;
                }
                var nowLocation = (int)(lastBaseOdometer + (lastisOdometerPlus ? 1 : -1) * state.Location);
                if (lastisOdometerHasMinus) {
                    panel[Config.odometer_Kmsymbol] = nowLocation > 0 ? 1 : 2;
                    //100km = 100000m
                    panel[Config.odometer_Km100] = D(Math.Abs(nowLocation), 5);
                    panel[Config.odometer_Km10] = D(Math.Abs(nowLocation), 4);
                    panel[Config.odometer_Km1] = D(Math.Abs(nowLocation), 3);
                    panel[Config.odometer_Km01] = D(Math.Abs(nowLocation), 2);
                    panel[Config.odometer_Km001] = D(Math.Abs(nowLocation), 1);
                } else {
                    panel[Config.odometer_Kmsymbol] = 0;
                    //100km = 100000m
                    panel[Config.odometer_Km100] = D(nowLocation < 0 ? 0 : nowLocation, 5);
                    panel[Config.odometer_Km10] = D(nowLocation < 0 ? 0 : nowLocation, 4);
                    panel[Config.odometer_Km1] = D(nowLocation < 0 ? 0 : nowLocation, 3);
                    panel[Config.odometer_Km01] = D(nowLocation < 0 ? 0 : nowLocation, 2);
                    panel[Config.odometer_Km001] = D(nowLocation < 0 ? 0 : nowLocation, 1);
                }
            } else {
                if (state.Time > DoorClosedTime + new TimeSpan(0, 0, 10) && DoorClosedTime != TimeSpan.Zero) {
                    panel[167] = 0;
                    panel[168] = CurrentSta;
                    panel[169] = NextSta;
                    DoorClosedTime = TimeSpan.Zero;
                }
                if (UpdateRequested) {
                    panel[151] = panel[152] = lastTrainType;
                } else {
                    panel[151] = panel[152] = TrainType;
                }
                var nowLocation = (int)(lastBaseOdometer + (lastisOdometerPlus ? 1 : -1) * state.Location);
                if (lastisOdometerHasMinus) {
                    panel[Config.odometer_Kmsymbol] = nowLocation > 0 ? 1 : 2;
                    //100km = 100000m
                    panel[Config.odometer_Km100] = D(Math.Abs(nowLocation), 5);
                    panel[Config.odometer_Km10] = D(Math.Abs(nowLocation), 4);
                    panel[Config.odometer_Km1] = D(Math.Abs(nowLocation), 3);
                    panel[Config.odometer_Km01] = D(Math.Abs(nowLocation), 2);
                    panel[Config.odometer_Km001] = D(Math.Abs(nowLocation), 1);
                } else {
                    panel[Config.odometer_Kmsymbol] = 0;
                    //100km = 100000m
                    panel[Config.odometer_Km100] = D(nowLocation < 0 ? 0 : nowLocation, 5);
                    panel[Config.odometer_Km10] = D(nowLocation < 0 ? 0 : nowLocation, 4);
                    panel[Config.odometer_Km1] = D(nowLocation < 0 ? 0 : nowLocation, 3);
                    panel[Config.odometer_Km01] = D(nowLocation < 0 ? 0 : nowLocation, 2);
                    panel[Config.odometer_Km001] = D(nowLocation < 0 ? 0 : nowLocation, 1);
                }
                
            }

            if (Snowbrake && state.BcPressure < Config.SnowBrakePressure) {
                vehicle.Instruments.BrakeSystem.TrailerCarBrake.BcValve.Pressure.Value = Config.SnowBrakePressure * 1000;
                vehicle.Instruments.BrakeSystem.MotorCarBrake.BcValve.Pressure.Value = Config.SnowBrakePressure * 1000;
            }

            panel[176] = Convert.ToInt32(Snowbrake);
            panel[161] = Convert.ToInt32(InstrumentLight);
            panel[251] = isStopAnnounce ? (state.Time.TotalMilliseconds % 1400 < 700 ? 1 : 0) : 0;
            panel[58] = state.Time.Hours;
            panel[59] = state.Time.Minutes;
            panel[60] = state.Time.Seconds;
            panel[173] = state.Speed > 5 ? 1 : 0;

            if (Config.Current_abs) {
                if (state.Speed < Config.MaxCurrentSpeed) {
                    panel[Config.CurrentPanelIndex] = (int)Math.Abs((0.25 + 0.75 * (state.Speed / Config.MaxCurrentSpeed)) * state.Current);
                } else {
                    panel[Config.CurrentPanelIndex] = (int)Math.Abs(state.Current);
                }
            } else {
                if (state.Speed < Config.MaxCurrentSpeed) {
                    panel[Config.CurrentPanelIndex] = (int)((state.Speed / Config.MaxCurrentSpeed) * state.Current);
                } else {
                    panel[Config.CurrentPanelIndex] = (int)state.Current;
                }
            }

            if (NeedConductorBuzzer) {
                if (state.Speed > 5) NeedConductorBuzzer = false;
                if (Conductorbuzzertime_station != TimeSpan.Zero) {
                    if (!isDoorOpen && state.Time > DoorClosedTime + Conductorbuzzertime_station) {
                        Conductorbuzzer_Depart = AtsSoundControlInstruction.Play;
                        Conductorbuzzertime_station = TimeSpan.Zero;
                        NeedConductorBuzzer = false;
                    }
                } else if (Conductorbuzzertime_global != TimeSpan.Zero && Conductorbuzzertime_station == TimeSpan.Zero) {
                    if (!isDoorOpen && state.Time > DoorClosedTime + Conductorbuzzertime_global) {
                        Conductorbuzzer_Depart = AtsSoundControlInstruction.Play;
                        NeedConductorBuzzer = false;
                    }
                }
            }

            if (state.Time > RadioChannelUpdateTime && RadioChannelUpdateTime != TimeSpan.Zero) {
                switch (RadioChannel) {
                    case KeyPosList.None: panel[Config.Panel_RadiochannelOutput] = 0; break;
                    case KeyPosList.Metro: panel[Config.Panel_RadiochannelOutput] = 1; break;
                    case KeyPosList.Tobu: panel[Config.Panel_RadiochannelOutput] = 2; break;
                    case KeyPosList.Tokyu: panel[Config.Panel_RadiochannelOutput] = 3; break;
                    case KeyPosList.Seibu: panel[Config.Panel_RadiochannelOutput] = 4; break;
                    case KeyPosList.Sotetsu: panel[Config.Panel_RadiochannelOutput] = 5; break;
                    case KeyPosList.JR: panel[Config.Panel_RadiochannelOutput] = 6; break;
                    case KeyPosList.Odakyu: panel[Config.Panel_RadiochannelOutput] = 7; break;
                    case KeyPosList.ToyoKosoku: panel[Config.Panel_RadiochannelOutput] = 8; break;
                }
                RadioChannelUpdateTime = TimeSpan.Zero;
                lastRadioChannel = RadioChannel;
            } else {
                switch (lastRadioChannel) {
                    case KeyPosList.None: panel[Config.Panel_RadiochannelOutput] = 0; break;
                    case KeyPosList.Metro: panel[Config.Panel_RadiochannelOutput] = 1; break;
                    case KeyPosList.Tobu: panel[Config.Panel_RadiochannelOutput] = 2; break;
                    case KeyPosList.Tokyu: panel[Config.Panel_RadiochannelOutput] = 3; break;
                    case KeyPosList.Seibu: panel[Config.Panel_RadiochannelOutput] = 4; break;
                    case KeyPosList.Sotetsu: panel[Config.Panel_RadiochannelOutput] = 5; break;
                    case KeyPosList.JR: panel[Config.Panel_RadiochannelOutput] = 6; break;
                    case KeyPosList.Odakyu: panel[Config.Panel_RadiochannelOutput] = 7; break;
                    case KeyPosList.ToyoKosoku: panel[Config.Panel_RadiochannelOutput] = 8; break;
                }
            }

            switch (LineDef) {
                case KeyPosList.None: panel[Config.Panel_LineDefOutput] = 0; break;
                case KeyPosList.Metro: panel[Config.Panel_LineDefOutput] = 1; break;
                case KeyPosList.Tobu: panel[Config.Panel_LineDefOutput] = 2; break;
                case KeyPosList.Tokyu: panel[Config.Panel_LineDefOutput] = 3; break;
                case KeyPosList.Seibu: panel[Config.Panel_LineDefOutput] = 4; break;
                case KeyPosList.Sotetsu: panel[Config.Panel_LineDefOutput] = 5; break;
                case KeyPosList.JR: panel[Config.Panel_LineDefOutput] = 6; break;
                case KeyPosList.Odakyu: panel[Config.Panel_LineDefOutput] = 7; break;
                case KeyPosList.ToyoKosoku: panel[Config.Panel_LineDefOutput] = 8; break;
            }

            sound[5] = (int)StopAnnounce;
            sound[6] = (int)StopAnnounce_Confirmed;
            sound[12] = (int)Lamp_SW_on;
            sound[13] = (int)Lamp_SW_off;
            sound[14] = (int)SnowBrake_on;
            sound[15] = (int)SnowBrake_off;
            if (lastBrakeNotch != vehicleSpec.BrakeNotches + 1 && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1 && state.Speed > 7) {
                sound[27] = (int)AtsSoundControlInstruction.Play;
            } else if (handles.BrakeNotch != vehicleSpec.BrakeNotches + 1) sound[27] = (int)AtsSoundControlInstruction.Continue;
            lastBrakeNotch = handles.BrakeNotch;
            sound[30] = (int)Tobu_DoorClosed;
            sound[31] = (int)Conductorbuzzer_Depart;
            sound[32] = (int)Door_poon;
            sound[33] = (int)OnBoardDepartMelody1;
            sound[34] = (int)OnBoardDepartMelody2;

            sound[90] = (int)Conductorbuzzer_Tokyu;
            sound[91] = (int)Conductorbuzzer_Odakyu;
            sound[92] = (int)Conductorbuzzer_Tobu;
            sound[95] = (int)Conductorbuzzer_Test;
            sound[99] = (int)Driver_buzzer;

            OnBoardDepartMelody2 = Tobu_DoorClosed = Conductorbuzzer_Tokyu = Conductorbuzzer_Tobu = Conductorbuzzer_Odakyu = Conductorbuzzer_Test = Lamp_SW_on = Lamp_SW_off = SnowBrake_on = SnowBrake_off = AtsSoundControlInstruction.Continue;
        }
    }
}
