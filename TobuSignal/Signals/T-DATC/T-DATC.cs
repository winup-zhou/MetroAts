using BveEx.PluginHost;
using BveEx.PluginHost.Panels.Native;
using BveEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;

namespace MetroAts {
    internal class T_DATC {
        public static INative Native;

        //InternalValue -> ATC
        public static int[] ATCLimits = { -2, -2, -2, -2, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -2, 35, -2, -1, 35, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };
        private static int LastSignal = 0;
        private static SpeedLimit ATCPattern = new SpeedLimit(), StationPattern = new SpeedLimit(), DistanceDisplayPattern = new SpeedLimit();
        private static int TrackPos = 0;
        private static bool StationStop = false, DistanceDisplay = false, TargetSpeedUp = false, ORPlamp = false;
        private static int ATCTargetSpeed = 0, ATCLimitSpeed = 0;
        private static double LastDingTime = Config.LessInf, TrackPosDisplayEndPos = 0, InitializeStartTime = 0;

        public static bool ATCEnable = false;
        private static bool Flag = false; //始発駅前の分岐器に対する特別処置
        private const double SignalPatternDec = -2.25; //*10
        private const double StationPatternDec = -4.0;

        public static int BrakeCommand = 0;


        //panel -> ATC
        public static bool ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45, ATC_50, ATC_55, ATC_60, ATC_65, ATC_70,
            ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110,
            ATC_Stop, ATC_Proceed, ATC_P, ATC_TobuATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach, ATC_StationStop;
        public static int ORPNeedle, ATCNeedle, ATCNeedle_Disappear, ATC_EndPointDistance, ATC_SwitcherPosition;

        private static IAtsSound ATC_Ding, ATC_PatternApproachBeep, ATC_StationStopAnnounce, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        public static void Initialize(BveEx.PluginHost.Native.StartedEventArgs e) {
            ATCPattern = new SpeedLimit();
            StationPattern = new SpeedLimit();
            DistanceDisplayPattern = new SpeedLimit();
            TrackPos = 0;
            StationStop = false;
            DistanceDisplay = false;
            BrakeCommand = 0;
            ORPlamp = false;
            ATCLimitSpeed = 0;
            LastSignal = 0;
            TargetSpeedUp = false;
            LastDingTime = Config.LessInf;
            TrackPosDisplayEndPos = 0;
            ATCEnable = false;

            ATC_Ding = MetroAts.ATC_Ding;
            ATC_PatternApproachBeep = MetroAts.ATC_PatternApproachBeep;
            ATC_StationStopAnnounce = MetroAts.ATC_StationStopAnnounce;
            //ATC_EmergencyOperationAnnounce = MetroAts.ATC_EmergencyOperationAnnounce;
            ATC_WarningBell = MetroAts.ATC_WarningBell;

            ATC_01 = false;
            ATC_10 = false;
            ATC_15 = false;
            ATC_20 = false;
            ATC_25 = false;
            ATC_30 = false;
            ATC_35 = false;
            ATC_40 = false;
            ATC_45 = false;
            ATC_50 = false;
            ATC_55 = false;
            ATC_60 = false;
            ATC_65 = false;
            ATC_70 = false;
            ATC_75 = false;
            ATC_80 = false;
            ATC_85 = false;
            ATC_90 = false;
            ATC_95 = false;
            ATC_100 = false;
            ATC_110 = false;

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_P = false;
            ATC_X = false;

            ORPNeedle = 0;
            ATCNeedle = 0;
            ATCNeedle_Disappear = 1;

            ATC_TobuATC = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_StationStop = false;

            ATC_PatternApproach = false;
            ATC_EndPointDistance = 0;

            ATC_SwitcherPosition = 0;
        }

        public static void Enable(double Time) {
            ATCEnable = true;
            InitializeStartTime = Time;
        }

        public static void BeaconPassed(BveEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 42:
                    if (e.Optional <= 3) {
                        TrackPos = e.Optional;
                        TrackPosDisplayEndPos = MetroAts.state.Location + e.Distance;
                        if (MetroAts.state.Location == 0) Flag = true;
                    }
                    break;
                case 43:
                    StationStop = true;
                    StationPattern = new SpeedLimit(0, MetroAts.state.Location + 510);
                    break;
                case 44:
                    StationStop = true;
                    StationPattern = new SpeedLimit(0, MetroAts.state.Location);
                    break;
            }
        }

        public static void DoorOpened(BveEx.PluginHost.Native.DoorEventArgs e) {
            StationStop = false;
        }

        public static void Tick(double Location, double Speed, double Time, Section CurrentSection, Section NextSection, Section Next2Section, double PretrainLocation) {
            if (ATCEnable) {
                ATC_TobuATC = true;
                var pretrainLocation = PretrainLocation - Location;

                DistanceDisplayPattern = new SpeedLimit(0, PretrainLocation);

                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == MetroAts.vehicleSpec.BrakeNotches + 1;
                if (CurrentSection.CurrentSignalIndex < 9 || CurrentSection.CurrentSignalIndex == 34 || CurrentSection.CurrentSignalIndex >= 49) {
                    ATC_X = true;
                    ATC_Stop = ATC_Proceed = ATC_P = false;
                    if (!Config.ATCLimitUseNeedle) {
                        ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
                        = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
                        = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
                        = ATC_95 = ATC_100 = ATC_110 = false;
                    } else {
                        ATCNeedle = 0;
                        ATCNeedle_Disappear = 1;
                    }
                    ATC_WarningBell.PlayLoop();

                    BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;

                } else {
                    if (Time - InitializeStartTime < 3000) {
                        ATC_X = true;
                        ATC_Stop = ATC_Proceed = ATC_P = false;
                        if (!Config.ATCLimitUseNeedle) {
                            ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
                            = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
                            = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
                            = ATC_95 = ATC_100 = ATC_110 = false;
                        } else {
                            ATCNeedle = 0;
                            ATCNeedle_Disappear = 1;
                        }

                        ATC_WarningBell.PlayLoop();

                        BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;
                    } else {
                        LastSignal = ATCTargetSpeed;

                        if (ATC_WarningBell.PlayState == BveEx.PluginHost.Sound.PlayState.PlayingLoop) {
                            ATC_WarningBell.Stop();
                            ATC_Ding.Play();
                            LastDingTime = MetroAts.state.Time.TotalMilliseconds;
                        }
                        if (ATC_X) ATC_X = false;

                        ATCLimitSpeed = (TargetSpeedUp && NextSection.Location - Location <= 25) ? (int)Math.Min(Config.TobuMaxSpeed, ATCPattern.AtLocation(Location, SignalPatternDec)) :
                                        (int)Math.Min(Config.TobuMaxSpeed, Math.Min(ATCPattern.AtLocation(Location, SignalPatternDec), ATCLimits[CurrentSection.CurrentSignalIndex]));

                        if (NextSection.CurrentSignalIndex < 9 || NextSection.CurrentSignalIndex == 34 || NextSection.CurrentSignalIndex >= 49) {
                            ATCPattern = new SpeedLimit(ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex],
                                        NextSection.CurrentSignalIndex == 0 ? NextSection.Location - 25 : NextSection.Location);
                            ATCTargetSpeed = NextSection.CurrentSignalIndex == 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex];
                            TargetSpeedUp = false;
                            if (LastSignal != ATCTargetSpeed && (ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex])
                                > (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex])) ORPlamp = true;
                        } else {
                            if (CurrentSection.CurrentSignalIndex >= 9 && CurrentSection.CurrentSignalIndex < 49) {
                                if ((ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex])
                                    < (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex])) {
                                    ATCPattern = new SpeedLimit(ATCLimits[NextSection.CurrentSignalIndex], NextSection.Location - 25);
                                    ATCTargetSpeed = ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex];
                                    TargetSpeedUp = true;
                                    ORPlamp = false;
                                } else {
                                    ATCPattern = new SpeedLimit(ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex],
                                        ATCLimits[NextSection.CurrentSignalIndex] <= 0 ? NextSection.Location - 25 : NextSection.Location);
                                    ATCTargetSpeed = ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex];
                                    TargetSpeedUp = false;
                                    if (LastSignal != ATCTargetSpeed && (ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex])
                                        > (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex])) ORPlamp = true;
                                }
                            } else {
                                ATCTargetSpeed = ATCLimitSpeed = 0;
                                ATC_X = true;
                            }
                        }

                        BrakeCommand = Math.Max(Speed > ATCLimitSpeed + 1.5 ? MetroAts.vehicleSpec.BrakeNotches
                            : (Speed > ATCLimitSpeed ? (int)Math.Ceiling(MetroAts.vehicleSpec.BrakeNotches * 0.5) : 0),
                            StationStop ? (Speed > StationPattern.AtLocation(Location, Config.EBDec) ?
                            MetroAts.vehicleSpec.BrakeNotches + 1 : 0) : 0);

                        //開通情報
                        var lastDistanceDisplay = DistanceDisplay;

                        DistanceDisplay = ATCLimits[NextSection.SignalIndexes[NextSection.SignalIndexes.Length - 1]]
                            > DistanceDisplayPattern.AtLocation(Next2Section.Location, SignalPatternDec)
                            && !(NextSection.CurrentSignalIndex >= 38 && NextSection.CurrentSignalIndex <= 48)
                            && !(Next2Section.CurrentSignalIndex >= 38 && Next2Section.CurrentSignalIndex <= 48);
                        if (DistanceDisplay) {
                            if (pretrainLocation < 200) ATC_EndPointDistance = 0;
                            else if (pretrainLocation >= 200 && pretrainLocation < 400) ATC_EndPointDistance = 1;
                            else if (pretrainLocation >= 400 && pretrainLocation < 600) ATC_EndPointDistance = 2;
                            else if (pretrainLocation >= 600 && pretrainLocation < 800) ATC_EndPointDistance = 3;
                            else if (pretrainLocation >= 800 && pretrainLocation < 1000) ATC_EndPointDistance = 4;
                            else if (pretrainLocation >= 1000 && pretrainLocation < 1200) ATC_EndPointDistance = 5;
                            else if (pretrainLocation >= 1200 && pretrainLocation < 1400) ATC_EndPointDistance = 6;
                            else if (pretrainLocation > 1400 && pretrainLocation < 1600) ATC_EndPointDistance = 7;
                            else if (pretrainLocation > 1600) ATC_EndPointDistance = 0;
                        } else ATC_EndPointDistance = 0;

                        //パターン接近
                        var lastATC_PatternApproach = ATC_PatternApproach;
                        ATC_PatternApproach = ATCLimitSpeed - Speed < 5 && ATCLimitSpeed > 0;
                        if (!lastATC_PatternApproach && ATC_PatternApproach)
                            ATC_PatternApproachBeep.StopAndPlay();

                        var lastATC_Depot = ATC_Depot;
                        ATC_Depot = CurrentSection.CurrentSignalIndex >= 38 && CurrentSection.CurrentSignalIndex <= 48;

                        //P表示灯
                        var lastORPlamp = ORPlamp;
                        if (!DistanceDisplay && (Speed < ATCTargetSpeed || Speed > ATCLimitSpeed) && ATCTargetSpeed > 0)
                            ORPlamp = false;
                        if (ATCTargetSpeed == (ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ?
                            0 : ATCLimits[CurrentSection.CurrentSignalIndex]))
                            ORPlamp = false;
                        ATC_P = ORPlamp;

                        if (ATCLimits[CurrentSection.CurrentSignalIndex] <= 0) {
                            BrakeCommand = MetroAts.vehicleSpec.BrakeNotches;
                            ORPNeedle = 0;
                            ATCTargetSpeed = 0;
                        }

                        //ATCベル
                        if (LastSignal != ATCTargetSpeed || lastORPlamp != ORPlamp || lastATC_Depot != ATC_Depot || lastDistanceDisplay != DistanceDisplay) {
                            ATC_Ding.Play();
                            LastDingTime = MetroAts.state.Time.TotalMilliseconds;
                        }
                        if (ATCTargetSpeed == 0 && MetroAts.state.Time.TotalMilliseconds - LastDingTime > 500) {
                            ATC_Ding.Play();
                            LastDingTime = Config.LessInf;
                        }

                        ORPNeedle = (ATCLimitSpeed < 0 ? 0 : ATCLimitSpeed) * 10;

                        //ATC速度指示
                        if (!Config.ATCLimitUseNeedle) {
                            ATC_01 = ATCTargetSpeed == 0;
                            ATC_10 = ATCTargetSpeed == 10;
                            ATC_15 = ATCTargetSpeed == 15;
                            ATC_20 = ATCTargetSpeed == 20;
                            ATC_25 = ATCTargetSpeed == 25;
                            ATC_30 = ATCTargetSpeed == 30;
                            ATC_35 = ATCTargetSpeed == 35;
                            ATC_40 = ATCTargetSpeed == 40;
                            ATC_45 = ATCTargetSpeed == 45;
                            ATC_50 = ATCTargetSpeed == 50;
                            ATC_55 = ATCTargetSpeed == 55;
                            ATC_60 = ATCTargetSpeed == 60;
                            ATC_65 = ATCTargetSpeed == 65;
                            ATC_70 = ATCTargetSpeed == 70;
                            ATC_75 = ATCTargetSpeed == 75;
                            ATC_80 = ATCTargetSpeed == 80;
                            ATC_85 = ATCTargetSpeed == 85;
                            ATC_90 = ATCTargetSpeed == 90;
                            ATC_95 = ATCTargetSpeed == 95;
                            ATC_100 = ATCTargetSpeed == 100;
                            ATC_110 = ATCTargetSpeed == 110;
                        } else {
                            ATCNeedle = ATCTargetSpeed;
                            ATCNeedle_Disappear = 0;
                        }

                        //進行・停止
                        ATC_Stop = ATCTargetSpeed == 0;
                        ATC_Proceed = ATCTargetSpeed > 0;

                        //駅停車
                        ATC_StationStop = StationStop;

                        //分岐器指示
                        ATC_SwitcherPosition = (MetroAts.state.Time.TotalMilliseconds % 1000 < 500 && ATCTargetSpeed > 0) ? TrackPos : 0;
                        if (Flag) {
                            TrackPosDisplayEndPos = NextSection.Location;
                            Flag = false;
                        }
                        if (TrackPos > 0 && Location > TrackPosDisplayEndPos) TrackPos = 0;
                    }
                }
            } else {
                BrakeCommand = 0;

                ATC_Ding.Stop();
                ATC_PatternApproachBeep.Stop();
                ATC_StationStopAnnounce.Stop();
                //ATC_EmergencyOperationAnnounce.Stop();
                ATC_WarningBell.Stop();

                ATC_01 = false;
                ATC_10 = false;
                ATC_15 = false;
                ATC_20 = false;
                ATC_25 = false;
                ATC_30 = false;
                ATC_35 = false;
                ATC_40 = false;
                ATC_45 = false;
                ATC_50 = false;
                ATC_55 = false;
                ATC_60 = false;
                ATC_65 = false;
                ATC_70 = false;
                ATC_75 = false;
                ATC_80 = false;
                ATC_85 = false;
                ATC_90 = false;
                ATC_95 = false;
                ATC_100 = false;
                ATC_110 = false;

                ATC_Stop = false;
                ATC_Proceed = false;

                ATC_P = false;
                ATC_X = false;

                ORPNeedle = 0;
                ATCNeedle = 0;
                ATCNeedle_Disappear = 1;

                ATC_TobuATC = false;
                ATC_Depot = false;
                ATC_ServiceBrake = false;
                ATC_EmergencyBrake = false;
                //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
                ATC_StationStop = false;

                ATC_PatternApproach = false;
                ATC_EndPointDistance = 0;

                ATC_SwitcherPosition = 0;
            }
        }

        public static void Disable() {
            ATCEnable = false;

            BrakeCommand = 0;

            ATC_Ding.Stop();
            ATC_PatternApproachBeep.Stop();
            ATC_StationStopAnnounce.Stop();
            //ATC_EmergencyOperationAnnounce.Stop();
            ATC_WarningBell.Stop();

            ATC_01 = false;
            ATC_10 = false;
            ATC_15 = false;
            ATC_20 = false;
            ATC_25 = false;
            ATC_30 = false;
            ATC_35 = false;
            ATC_40 = false;
            ATC_45 = false;
            ATC_50 = false;
            ATC_55 = false;
            ATC_60 = false;
            ATC_65 = false;
            ATC_70 = false;
            ATC_75 = false;
            ATC_80 = false;
            ATC_85 = false;
            ATC_90 = false;
            ATC_95 = false;
            ATC_100 = false;
            ATC_110 = false;

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_P = false;
            ATC_X = false;

            ORPNeedle = 0;
            ATCNeedle = 0;
            ATCNeedle_Disappear = 1;

            ATC_TobuATC = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_StationStop = false;

            ATC_PatternApproach = false;
            ATC_EndPointDistance = 0;

            ATC_SwitcherPosition = 0;
        }
    }
}
