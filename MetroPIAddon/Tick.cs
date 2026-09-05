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
using System.Collections.Generic;

namespace MetroPIAddon {
    public partial class MetroPIAddon : AssemblyPluginBase {
        public override void Tick(TimeSpan elapsed) {
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            Conductorbuzzer_Depart = SoundPlayMode.Continue;

            if (isStopAnnounce) {
                if (StopAnnounce == SoundPlayMode.Stop && StopAnnounce_Confirmed != SoundPlayMode.PlayLooping) {
                    StopAnnounce = SoundPlayMode.PlayLooping;
                }
                if (handles.BrakeNotch > 0 && StopAnnounce != SoundPlayMode.Stop) {
                    StopAnnounce = SoundPlayMode.Stop;
                    StopAnnounce_Confirmed = SoundPlayMode.PlayLooping;
                }
            } else {
                StopAnnounce = StopAnnounce_Confirmed = SoundPlayMode.Stop;
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
                        BaseOdometer = Convert.ToInt32(targetBeacon.SendData / 100) - (isOdometerPlus ? 1 : -1) * targetBeacon.Location;
                        lastOdometerBeacon = targetBeacon;
                    }
                    break;
                }
            }

            if (FDmode == 0) {
                Config.PanelMap.WritePanel(panel, "platformdoor_mode", 0);
                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 0);
                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 0);
                Config.PanelMap.WritePanel(panel, "platformdoor_count", 0);
            } else if (FDmode == 1) {
                Config.PanelMap.WritePanel(panel, "platformdoor_mode", 1);
                var leftDoorState = vehicle.Doors.GetSide(DoorSide.Left).CarDoors[0].State;
                var rightDoorState = vehicle.Doors.GetSide(DoorSide.Right).CarDoors[0].State;
                var doorCloseTimes = TimeSpan.FromMilliseconds(vehicle.Doors.StandardCloseTime) + TimeSpan.FromSeconds(Config.Delay_FDclosed);
                if (state.Location > currentStation.MinStopPosition && state.Location < currentStation.MaxStopPosition) {
                    if (!isDoorOpen && state.Time > TimeSpan.FromSeconds(Config.Delay_FDclosed) + DoorClosedTime) {
                        int blinkVal = state.Time.TotalMilliseconds % 1000 < 500 ? 1 : 0;
                        if (Config.FDsinglelamp) {
                            Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", blinkVal);
                            Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", blinkVal);
                        } else {
                            if (currentStation.DoorSide == DoorSide.Left) {
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", blinkVal);
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 1);
                            } else if (currentStation.DoorSide == DoorSide.Right) {
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 1);
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", blinkVal);
                            } else {
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", blinkVal);
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", blinkVal);
                            }
                        }
                    } else if (isDoorOpen) {
                        if (Config.FDsinglelamp) {
                            Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 0);
                            Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 0);
                        } else {
                            if (currentStation.DoorSide == DoorSide.Left) {
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 0);
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 1);
                            } else if (currentStation.DoorSide == DoorSide.Right) {
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 1);
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 0);
                            } else {
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 0);
                                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 0);
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
                            WritePlatformDoorCountdown(panel, state.Time, doorCloseTimes);
                        } else Config.PanelMap.WritePanel(panel, "platformdoor_count", 0);
                    } else {
                        if (corePlugin.KeyPos != MetroAts.KeyPosList.None && state.Speed < 15 && CCTVenable) {
                            WritePlatformDoorCountdown(panel, state.Time, doorCloseTimes);
                        } else Config.PanelMap.WritePanel(panel, "platformdoor_count", 0);
                    }

                } else {
                    Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 1);
                    Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 1);
                    if (StandAloneMode && Keyin) {
                        if (Math.Abs(state.Location - currentStation.Location) < 10 && state.Speed < 15 && CCTVenable) {
                            Config.PanelMap.WritePanel(panel, "platformdoor_count", 1);
                        } else {
                            Config.PanelMap.WritePanel(panel, "platformdoor_count", 0);
                        }
                    } else if (corePlugin.KeyPos != MetroAts.KeyPosList.None) {
                        if (Math.Abs(state.Location - currentStation.Location) < 10 && state.Speed < 15 && CCTVenable) {
                            Config.PanelMap.WritePanel(panel, "platformdoor_count", 1);
                        } else {
                            Config.PanelMap.WritePanel(panel, "platformdoor_count", 0);
                        }
                    }

                }
                lastLeftDoorState = leftDoorState;
                lastRightDoorState = rightDoorState;
            } else if (FDmode == 2) {
                Config.PanelMap.WritePanel(panel, "platformdoor_mode", 2);
                Config.PanelMap.WritePanel(panel, "platformdoor_ind_right", 0);
                Config.PanelMap.WritePanel(panel, "platformdoor_ind_left", 0);
                Config.PanelMap.WritePanel(panel, "platformdoor_count", 0);
            }



            if (isDoorOpen) {
                if (state.Time > DoorOpenTime + new TimeSpan(0, 0, 2)) {
                    Config.PanelMap.WritePanel(panel, "station_stop", CurrentSta);
                    Config.PanelMap.WritePanel(panel, "station_current", 0);
                    Config.PanelMap.WritePanel(panel, "station_next", 0);
                    lastisOdometerPlus = isOdometerPlus;
                    lastisOdometerHasMinus = isOdometerHasMinus;
                    lastBaseOdometer = BaseOdometer;
                }
                if (state.Time > DoorOpenTime + new TimeSpan(0, 0, 10)) {
                    WriteTrainNumberDisplay(panel);
                    Config.PanelMap.WritePanel(panel, "traintype", TrainType);
                    Config.PanelMap.WritePanel(panel, "traintype_sub", TrainType);
                    if (UpdateRequested) {
                        UpdateRequested = false;
                        lastTrainType = TrainType;
                    }
                } else {
                    Config.PanelMap.WritePanel(panel, "traintype", lastTrainType);
                    Config.PanelMap.WritePanel(panel, "traintype_sub", lastTrainType);
                }
                var nowLocation = (int)(lastBaseOdometer + (lastisOdometerPlus ? 1 : -1) * state.Location);
                WriteOdometer(panel, lastisOdometerHasMinus, nowLocation);
            } else {
                if (state.Time > DoorClosedTime + new TimeSpan(0, 0, 10) && DoorClosedTime != TimeSpan.Zero) {
                    Config.PanelMap.WritePanel(panel, "station_stop", 0);
                    Config.PanelMap.WritePanel(panel, "station_current", CurrentSta);
                    Config.PanelMap.WritePanel(panel, "station_next", NextSta);
                    DoorClosedTime = TimeSpan.Zero;
                }
                if (UpdateRequested) {
                    Config.PanelMap.WritePanel(panel, "traintype", lastTrainType);
                    Config.PanelMap.WritePanel(panel, "traintype_sub", lastTrainType);
                } else {
                    Config.PanelMap.WritePanel(panel, "traintype", TrainType);
                    Config.PanelMap.WritePanel(panel, "traintype_sub", TrainType);
                }
                var nowLocation = (int)(lastBaseOdometer + (lastisOdometerPlus ? 1 : -1) * state.Location);
                WriteOdometer(panel, lastisOdometerHasMinus, nowLocation);
                
            }

            if (Snowbrake && state.BcPressure < Config.SnowBrakePressure) {
                vehicle.Instruments.BrakeSystem.TrailerCarBrake.BcValve.Pressure.Value = Config.SnowBrakePressure * 1000;
                vehicle.Instruments.BrakeSystem.MotorCarBrake.BcValve.Pressure.Value = Config.SnowBrakePressure * 1000;
            }

            Config.PanelMap.WritePanel(panel, "snowbrake", Convert.ToInt32(Snowbrake));
            Config.PanelMap.WritePanel(panel, "instrumentlight", Convert.ToInt32(InstrumentLight));
            Config.PanelMap.WritePanel(panel, "stopannounce_lamp", isStopAnnounce ? (state.Time.TotalMilliseconds % 1400 < 700 ? 1 : 0) : 0);
            Config.PanelMap.WritePanel(panel, "clock_hour", state.Time.Hours);
            Config.PanelMap.WritePanel(panel, "clock_minute", state.Time.Minutes);
            Config.PanelMap.WritePanel(panel, "clock_second", state.Time.Seconds);
            Config.PanelMap.WritePanel(panel, "speed_over5", state.Speed > 5 ? 1 : 0);

            if (Config.Current_abs) {
                if (state.Speed < Config.MaxCurrentSpeed) {
                    Config.PanelMap.WritePanel(panel, "current", (int)Math.Abs((0.25 + 0.75 * (state.Speed / Config.MaxCurrentSpeed)) * state.Current));
                } else {
                    Config.PanelMap.WritePanel(panel, "current", (int)Math.Abs(state.Current));
                }
            } else {
                if (state.Speed < Config.MaxCurrentSpeed) {
                    Config.PanelMap.WritePanel(panel, "current", (int)((state.Speed / Config.MaxCurrentSpeed) * state.Current));
                } else {
                    Config.PanelMap.WritePanel(panel, "current", (int)state.Current);
                }
            }

            if (NeedConductorBuzzer) {
                if (state.Speed > 5) NeedConductorBuzzer = false;
                if (Conductorbuzzertime_station != TimeSpan.Zero) {
                    if (!isDoorOpen && state.Time > DoorClosedTime + Conductorbuzzertime_station) {
                        Conductorbuzzer_Depart = SoundPlayMode.Play;
                        Conductorbuzzertime_station = TimeSpan.Zero;
                        NeedConductorBuzzer = false;
                    }
                } else if (Conductorbuzzertime_global != TimeSpan.Zero && Conductorbuzzertime_station == TimeSpan.Zero) {
                    if (!isDoorOpen && state.Time > DoorClosedTime + Conductorbuzzertime_global) {
                        Conductorbuzzer_Depart = SoundPlayMode.Play;
                        NeedConductorBuzzer = false;
                    }
                }
            }

            if (state.Time > RadioChannelUpdateTime && RadioChannelUpdateTime != TimeSpan.Zero) {
                Config.PanelMap.WritePanel(panel, "radiochannel", KeyPosToOutputNumber(RadioChannel));
                RadioChannelUpdateTime = TimeSpan.Zero;
                lastRadioChannel = RadioChannel;
            } else {
                Config.PanelMap.WritePanel(panel, "radiochannel", KeyPosToOutputNumber(lastRadioChannel));
            }

            Config.PanelMap.WritePanel(panel, "linedef", KeyPosToOutputNumber(LineDef));

            if (!StandAloneMode) {
                var keyPos = (KeyPosList)corePlugin.KeyPos;
                if (keyPos != KeyPosList.None) Config.PanelMap.WritePanel(panel, "keyposition", KeyPosToOutputNumber(keyPos));
            }
            

            Config.SoundMap.WriteSound(sound, "stopannounce", (int)StopAnnounce);
            Config.SoundMap.WriteSound(sound, "stopannounce_confirmed", (int)StopAnnounce_Confirmed);
            Config.SoundMap.WriteSound(sound, "lampsw_on", (int)Lamp_SW_on);
            Config.SoundMap.WriteSound(sound, "lampsw_off", (int)Lamp_SW_off);
            Config.SoundMap.WriteSound(sound, "snowbrake_on", (int)SnowBrake_on);
            Config.SoundMap.WriteSound(sound, "snowbrake_off", (int)SnowBrake_off);
            if (lastBrakeNotch != vehicleSpec.BrakeNotches + 1 && AtsHandles.BrakeNotch == vehicleSpec.BrakeNotches + 1 && state.Speed > 7) {
                Config.SoundMap.WriteSound(sound, "eb_alarm", (int)SoundPlayMode.Play);
            } else if (AtsHandles.BrakeNotch != vehicleSpec.BrakeNotches + 1) Config.SoundMap.WriteSound(sound, "eb_alarm", (int)SoundPlayMode.Continue);
            lastBrakeNotch = AtsHandles.BrakeNotch;
            Config.SoundMap.WriteSound(sound, "tobu_doorclosed", (int)Tobu_DoorClosed);
            Config.SoundMap.WriteSound(sound, "conductor_depart", (int)Conductorbuzzer_Depart);
            Config.SoundMap.WriteSound(sound, "door_poon", (int)Door_poon);
            Config.SoundMap.WriteSound(sound, "depart_melody", (int)OnBoardDepartMelody1);
            Config.SoundMap.WriteSound(sound, "depart_announce", (int)OnBoardDepartMelody2);

            Config.SoundMap.WriteSound(sound, "conductor_tokyu", (int)Conductorbuzzer_Tokyu);
            Config.SoundMap.WriteSound(sound, "conductor_odakyu", (int)Conductorbuzzer_Odakyu);
            Config.SoundMap.WriteSound(sound, "conductor_tobu", (int)Conductorbuzzer_Tobu);
            Config.SoundMap.WriteSound(sound, "conductor_test", (int)Conductorbuzzer_Test);
            Config.SoundMap.WriteSound(sound, "driver_buzzer", (int)Driver_buzzer);

            OnBoardDepartMelody2 = Tobu_DoorClosed = Conductorbuzzer_Tokyu = Conductorbuzzer_Tobu = Conductorbuzzer_Odakyu = Conductorbuzzer_Test = Lamp_SW_on = Lamp_SW_off = SnowBrake_on = SnowBrake_off = SoundPlayMode.Continue;
        }

        /// <summary>
        /// ホームドア開閉インターロック進捗数値表示（旧 panel[193]）を PanelMap 経由で書込む。
        /// FDCloseTime/FDOpenTime の経過に応じ 0..12 の値を表示し、完了したタイマは自らクリアする。
        /// 条件に該当しない場合は何も書込まない（旧コードのフォールスルーと同一）。
        /// </summary>
        private void WritePlatformDoorCountdown(IList<int> panel, TimeSpan stateTime, TimeSpan doorCloseTimes) {
            if (FDCloseTime != TimeSpan.Zero) {
                FDOpenTime = TimeSpan.Zero;
                if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < doorCloseTimes.TotalSeconds && FDCloseTime.TotalSeconds - stateTime.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 5) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 7);
                } else if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 5 && FDCloseTime.TotalSeconds - stateTime.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 4) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 8);
                } else if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 4 && FDCloseTime.TotalSeconds - stateTime.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 3) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 9);
                } else if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 3 && FDCloseTime.TotalSeconds - stateTime.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6) * 2) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 10);
                } else if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) * 2 && FDCloseTime.TotalSeconds - stateTime.TotalSeconds >= (doorCloseTimes.TotalSeconds / 6)) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 11);
                } else if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < (doorCloseTimes.TotalSeconds / 6) && FDCloseTime.TotalSeconds - stateTime.TotalSeconds >= 0) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 12);
                } else if (FDCloseTime.TotalSeconds - stateTime.TotalSeconds < 0) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 2);
                    FDCloseTime = TimeSpan.Zero;
                }
            } else if (FDOpenTime != TimeSpan.Zero) {
                FDCloseTime = TimeSpan.Zero;
                if (FDOpenTime.TotalSeconds - stateTime.TotalSeconds < 2.5 && FDOpenTime.TotalSeconds - stateTime.TotalSeconds >= 2) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 2);
                } else if (FDOpenTime.TotalSeconds - stateTime.TotalSeconds < 2 && FDOpenTime.TotalSeconds - stateTime.TotalSeconds >= 1.5) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 3);
                } else if (FDOpenTime.TotalSeconds - stateTime.TotalSeconds < 1.5 && FDOpenTime.TotalSeconds - stateTime.TotalSeconds >= 1) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 4);
                } else if (FDOpenTime.TotalSeconds - stateTime.TotalSeconds < 1 && FDOpenTime.TotalSeconds - stateTime.TotalSeconds >= 0.5) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 5);
                } else if (FDOpenTime.TotalSeconds - stateTime.TotalSeconds < 0.5 && FDOpenTime.TotalSeconds - stateTime.TotalSeconds >= 0) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 6);
                } else if (FDOpenTime.TotalSeconds - stateTime.TotalSeconds < 0) {
                    Config.PanelMap.WritePanel(panel, "platformdoor_count", 7);
                    FDOpenTime = TimeSpan.Zero;
                }
            } else if (FDOpenTime == TimeSpan.Zero && FDCloseTime == TimeSpan.Zero) {
                if (!isDoorOpen && stateTime > TimeSpan.FromSeconds(Config.Delay_FDclosed) + DoorClosedTime) Config.PanelMap.WritePanel(panel, "platformdoor_count", 2);
                else if (isDoorOpen) Config.PanelMap.WritePanel(panel, "platformdoor_count", 7);
            }
        }

        /// <summary>
        /// 列車番号表示（旧 panel[62..68]）と運用番号（153/154）・行先（172）を PanelMap 経由で書込む。
        /// 62..65 = TrainNumber の 10^2..10^5 桁、67 = 10^6 以上、68 = 下 2 桁（0..99）。
        /// </summary>
        private void WriteTrainNumberDisplay(IList<int> panel) {
            Config.PanelMap.WritePanel(panel, "trainnumber_100000", D(TrainNumber / 100 % 10000, 3));
            Config.PanelMap.WritePanel(panel, "trainnumber_10000", D(TrainNumber / 100 % 10000, 2));
            Config.PanelMap.WritePanel(panel, "trainnumber_1000", D(TrainNumber / 100 % 10000, 1));
            Config.PanelMap.WritePanel(panel, "trainnumber_100", D(TrainNumber / 100 % 10000, 0));
            Config.PanelMap.WritePanel(panel, "trainnumber_1000000", TrainNumber / 1000000);
            Config.PanelMap.WritePanel(panel, "trainnumber_10", TrainNumber % 100);
            Config.PanelMap.WritePanel(panel, "runningnumber_10", D(TrainRunningNumber, 1));
            Config.PanelMap.WritePanel(panel, "runningnumber_1", D(TrainRunningNumber, 0));
            Config.PanelMap.WritePanel(panel, "destination", Destination);
        }

        /// <summary>
        /// 里程計表示（旧 panel[Config.odometer_*]）を PanelMap 経由で書込む。
        /// hasMinus = マイナス区間（キロ程が減少する方向）であるか。
        /// </summary>
        private void WriteOdometer(IList<int> panel, bool hasMinus, int nowLocation) {
            if (hasMinus) {
                Config.PanelMap.WritePanel(panel, "odometer_kmsymbol", nowLocation > 0 ? 1 : 2);
                //100km = 100000m
                Config.PanelMap.WritePanel(panel, "odometer_km100", D(Math.Abs(nowLocation), 5));
                Config.PanelMap.WritePanel(panel, "odometer_km10", D(Math.Abs(nowLocation), 4));
                Config.PanelMap.WritePanel(panel, "odometer_km1", D(Math.Abs(nowLocation), 3));
                Config.PanelMap.WritePanel(panel, "odometer_km01", D(Math.Abs(nowLocation), 2));
                Config.PanelMap.WritePanel(panel, "odometer_km001", D(Math.Abs(nowLocation), 1));
            } else {
                Config.PanelMap.WritePanel(panel, "odometer_kmsymbol", 0);
                //100km = 100000m
                Config.PanelMap.WritePanel(panel, "odometer_km100", D(nowLocation < 0 ? 0 : nowLocation, 5));
                Config.PanelMap.WritePanel(panel, "odometer_km10", D(nowLocation < 0 ? 0 : nowLocation, 4));
                Config.PanelMap.WritePanel(panel, "odometer_km1", D(nowLocation < 0 ? 0 : nowLocation, 3));
                Config.PanelMap.WritePanel(panel, "odometer_km01", D(nowLocation < 0 ? 0 : nowLocation, 2));
                Config.PanelMap.WritePanel(panel, "odometer_km001", D(nowLocation < 0 ? 0 : nowLocation, 1));
            }
        }

        /// <summary>KeyPosList → 表示用番号（None=0, Metro=1, Tobu=2, Tokyu=3, Seibu=4, Sotetsu=5, JR=6, Odakyu=7, ToyoKosoku=8）。</summary>
        private static int KeyPosToOutputNumber(KeyPosList k) {
            switch (k) {
                case KeyPosList.None: return 0;
                case KeyPosList.Metro: return 1;
                case KeyPosList.Tobu: return 2;
                case KeyPosList.Tokyu: return 3;
                case KeyPosList.Seibu: return 4;
                case KeyPosList.Sotetsu: return 5;
                case KeyPosList.JR: return 6;
                case KeyPosList.Odakyu: return 7;
                case KeyPosList.ToyoKosoku: return 8;
                default: return 0;
            }
        }
    }
}
