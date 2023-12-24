using AtsEx.PluginHost;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;

namespace TobuAts_EX {
    internal class T_DATC {
        public static INative Native;

        //InternalValue -> ATC
        public static int[] ATCLimits = { -2, 25, 55, 75, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -1, -2, -2, -1, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };
        private static int LastSignal = 0;
        private static SpeedLimit ATCPattern = new SpeedLimit(), StationPattern = new SpeedLimit(), DistanceDisplayPattern = new SpeedLimit();
        private static int TrackPos = 0;
        private static bool StationStop = false, DistanceDisplay = false, TargetSpeedUp = false, ORPlamp = false;
        private static int ATCTargetSpeed = 0, ATCLimitSpeed = 0;
        private static double PretrainLocation = 0, LastDingTime = Config.LessInf, TrackPosDisplayEndPos = 0, InitializeStartTime = 0;

        private const double SignalPatternDec = -2.25; //*10
        private const double StationPatternDec = -4.0;

        public static int BrakeCommand = 0;


        //panel -> ATC
        private static IAtsPanelValue<bool> ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45, ATC_50, ATC_55, ATC_60, ATC_65, ATC_70,
            ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110,
            ATC_Stop, ATC_Proceed, ATC_P, ATC_TobuATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach, ATC_StationStop;
        public static IAtsPanelValue<int> ORPNeedle, ATCNeedle, ATCNeedle_Disappear;
        private static IAtsPanelValue<int> ATC_EndPointDistance, ATC_SwitcherPosition;
        private static IAtsSound ATC_Ding, ATC_PatternApproachBeep, ATC_StationStopAnnounce, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        public static void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            ATCPattern = new SpeedLimit();
            StationPattern = new SpeedLimit();
            DistanceDisplayPattern = new SpeedLimit();
            TrackPos = 0;
            StationStop = false;
            DistanceDisplay = false;
            BrakeCommand = 0;
            ORPlamp = false;
            ATCLimitSpeed = 0;
            PretrainLocation = 0;
            LastSignal = 0;
            TargetSpeedUp = false;
            LastDingTime = Config.LessInf;
            TrackPosDisplayEndPos = 0;
            InitializeStartTime = 0;
            if (e.DefaultBrakePosition == BrakePosition.Removed) {
                InitializeStartTime = TobuAts.state.Time.TotalMilliseconds;
            }
        }

        public static void Load() {
            ATCPattern = new SpeedLimit();
            StationPattern = new SpeedLimit();
            DistanceDisplayPattern = new SpeedLimit();
            TrackPos = 0;
            StationStop = false;
            DistanceDisplay = false;
            BrakeCommand = 0;
            ORPlamp = false;
            ATCLimitSpeed = 0;
            PretrainLocation = 0;
            LastSignal = 0;
            TargetSpeedUp = false;
            LastDingTime = Config.LessInf;
            TrackPosDisplayEndPos = 0;
            InitializeStartTime = 0;

            ATC_Ding = Native.AtsSounds.Register(2);
            ATC_PatternApproachBeep = Native.AtsSounds.Register(116);
            ATC_StationStopAnnounce = Native.AtsSounds.Register(117); 
            ATC_EmergencyOperationAnnounce = Native.AtsSounds.Register(119);
            ATC_WarningBell = Native.AtsSounds.Register(0);

            ATC_01 = Native.AtsPanelValues.RegisterBoolean(102);
            ATC_10 = Native.AtsPanelValues.RegisterBoolean(104);
            ATC_15 = Native.AtsPanelValues.RegisterBoolean(105);
            ATC_20 = Native.AtsPanelValues.RegisterBoolean(106);
            ATC_25 = Native.AtsPanelValues.RegisterBoolean(107);
            ATC_30 = Native.AtsPanelValues.RegisterBoolean(108);
            ATC_35 = Native.AtsPanelValues.RegisterBoolean(109); 
            ATC_40 = Native.AtsPanelValues.RegisterBoolean(110);
            ATC_45 = Native.AtsPanelValues.RegisterBoolean(111);
            ATC_50 = Native.AtsPanelValues.RegisterBoolean(112);
            ATC_55 = Native.AtsPanelValues.RegisterBoolean(113);
            ATC_60 = Native.AtsPanelValues.RegisterBoolean(114);
            ATC_65 = Native.AtsPanelValues.RegisterBoolean(115);
            ATC_70 = Native.AtsPanelValues.RegisterBoolean(116);
            ATC_75 = Native.AtsPanelValues.RegisterBoolean(117);
            ATC_80 = Native.AtsPanelValues.RegisterBoolean(118);
            ATC_85 = Native.AtsPanelValues.RegisterBoolean(119);
            ATC_90 = Native.AtsPanelValues.RegisterBoolean(120);
            ATC_95 = Native.AtsPanelValues.RegisterBoolean(121);
            ATC_100 = Native.AtsPanelValues.RegisterBoolean(122);
            ATC_110 = Native.AtsPanelValues.RegisterBoolean(124);

            ATC_Stop = Native.AtsPanelValues.RegisterBoolean(131);
            ATC_Proceed = Native.AtsPanelValues.RegisterBoolean(132);

            ATC_P = Native.AtsPanelValues.RegisterBoolean(134);
            ATC_X = Native.AtsPanelValues.RegisterBoolean(101);

            ORPNeedle = Native.AtsPanelValues.RegisterInt32(135);
            ATCNeedle = Native.AtsPanelValues.RegisterInt32(127);
            ATCNeedle_Disappear = Native.AtsPanelValues.RegisterInt32(103);

            ATC_TobuATC = Native.AtsPanelValues.RegisterBoolean(74);
            ATC_Depot = Native.AtsPanelValues.RegisterBoolean(75);
            ATC_ServiceBrake = Native.AtsPanelValues.RegisterBoolean(77);
            ATC_EmergencyBrake = Native.AtsPanelValues.RegisterBoolean(76);
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_StationStop = Native.AtsPanelValues.RegisterBoolean(252);

            ATC_PatternApproach = Native.AtsPanelValues.RegisterBoolean(128);
            ATC_EndPointDistance = Native.AtsPanelValues.RegisterInt32(129);

            ATC_SwitcherPosition = Native.AtsPanelValues.RegisterInt32(130);

            ATC_P.ValueChanged += ValueChanged;
            ATC_Depot.ValueChanged += ValueChanged;
        }

        public static void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 42:
                    if (e.Optional <= 3) {
                        TrackPos = e.Optional;
                        TrackPosDisplayEndPos = TobuAts.state.Location + e.Distance;
                    }
                    break;
                case 43:
                    StationStop = true;
                    StationPattern = new SpeedLimit(0, TobuAts.state.Location + 510);
                    break;
                case 44:
                    StationStop = true;
                    StationPattern = new SpeedLimit(0, TobuAts.state.Location);
                    break;
            }
        }

        public static void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            StationStop = false;
        }

        public static void Tick(double Location, double Speed, SectionManager sectionManager) {
            ATC_TobuATC.Value = TobuAts.SignalMode == 1;

            int pointer = 0, pointer_ = 0;
            while (sectionManager.Sections[pointer].Location < Location) pointer++;
            if (pointer >= sectionManager.Sections.Count) pointer = sectionManager.Sections.Count - 1;

            while (sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location < Location) pointer_++;
            if (pointer_ >= sectionManager.StopSignalSectionIndexes.Count) pointer_ = sectionManager.StopSignalSectionIndexes.Count - 1;

            var CurrentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;
            var NextSection = sectionManager.Sections[pointer] as Section;

            PretrainLocation = sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location - Location;

            DistanceDisplayPattern = new SpeedLimit(0, sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location);

            ATC_ServiceBrake.Value = BrakeCommand > 0;
            ATC_EmergencyBrake.Value = BrakeCommand == TobuAts.vehicleSpec.BrakeNotches + 1;
            if(TobuAts.state.Time.TotalMilliseconds - InitializeStartTime < 5000) {
                ATC_X.Value = true;
                ATC_Stop.Value = ATC_Proceed.Value = ATC_P.Value = false;
                if (Config.ATCLimitPerLamp) {
                    ATC_01.Value = ATC_10.Value = ATC_15.Value = ATC_20.Value = ATC_25.Value = ATC_30.Value
                    = ATC_35.Value = ATC_40.Value = ATC_45.Value = ATC_50.Value = ATC_55.Value = ATC_60.Value
                    = ATC_65.Value = ATC_70.Value = ATC_75.Value = ATC_80.Value = ATC_85.Value = ATC_90.Value
                    = ATC_95.Value = ATC_100.Value = ATC_110.Value = false;
                } else {
                    ATCNeedle.Value = 0;
                    ATCNeedle_Disappear.Value = 1;
                }

                ATC_WarningBell.PlayLoop();

                BrakeCommand = TobuAts.vehicleSpec.BrakeNotches + 1;
            } else {
                if (CurrentSection.CurrentSignalIndex <= 9 && CurrentSection.CurrentSignalIndex >= 49) {
                    ATC_X.Value = true;
                    ATC_Stop.Value = ATC_Proceed.Value = ATC_P.Value = false;
                    if (Config.ATCLimitPerLamp) {
                        ATC_01.Value = ATC_10.Value = ATC_15.Value = ATC_20.Value = ATC_25.Value = ATC_30.Value
                        = ATC_35.Value = ATC_40.Value = ATC_45.Value = ATC_50.Value = ATC_55.Value = ATC_60.Value
                        = ATC_65.Value = ATC_70.Value = ATC_75.Value = ATC_80.Value = ATC_85.Value = ATC_90.Value
                        = ATC_95.Value = ATC_100.Value = ATC_110.Value = false;
                    } else {
                        ATCNeedle.Value = 0;
                        ATCNeedle_Disappear.Value = 1;
                    }
                    ATC_WarningBell.PlayLoop();

                    BrakeCommand = TobuAts.vehicleSpec.BrakeNotches + 1;

                } else {
                    if (ATC_WarningBell.PlayState == AtsEx.PluginHost.Sound.PlayState.PlayingLoop) ATC_WarningBell.Stop();

                    ATC_Depot.Value = CurrentSection.CurrentSignalIndex >= 38 && CurrentSection.CurrentSignalIndex <= 48;

                    if (ATCLimits[CurrentSection.SignalIndexes[CurrentSection.SignalIndexes.Length - 1]] > DistanceDisplayPattern.AtLocation(NextSection.Location, SignalPatternDec)) {
                        if (PretrainLocation < 200) ATC_EndPointDistance.Value = 0;
                        else if (PretrainLocation >= 200 && PretrainLocation < 400) ATC_EndPointDistance.Value = 1;
                        else if (PretrainLocation >= 400 && PretrainLocation < 600) ATC_EndPointDistance.Value = 2;
                        else if (PretrainLocation >= 600 && PretrainLocation < 800) ATC_EndPointDistance.Value = 3;
                        else if (PretrainLocation >= 800 && PretrainLocation < 1000) ATC_EndPointDistance.Value = 4;
                        else if (PretrainLocation >= 1000 && PretrainLocation < 1200) ATC_EndPointDistance.Value = 5;
                        else if (PretrainLocation >= 1200 && PretrainLocation < 1400) ATC_EndPointDistance.Value = 6;
                        else if (PretrainLocation > 1400 && PretrainLocation < 1600) ATC_EndPointDistance.Value = 7;
                        else if (PretrainLocation > 1600) ATC_EndPointDistance.Value = 0;
                    } else ATC_EndPointDistance.Value = 0;

                    if (ATCLimits[CurrentSection.SignalIndexes[CurrentSection.SignalIndexes.Length - 1]] > DistanceDisplayPattern.AtLocation(NextSection.Location, SignalPatternDec)
                        != DistanceDisplay) ATC_Ding.Play();
                    DistanceDisplay = ATCLimits[CurrentSection.SignalIndexes[CurrentSection.SignalIndexes.Length - 1]] > DistanceDisplayPattern.AtLocation(NextSection.Location, SignalPatternDec);

                    if(NextSection.CurrentSignalIndex <= 9 && NextSection.CurrentSignalIndex >= 49) {
                        ATCPattern = new SpeedLimit(NextSection.CurrentSignalIndex == 0 ? 7 : ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex],
                                    NextSection.CurrentSignalIndex == 0 ? NextSection.Location - 25 : NextSection.Location);
                        ATCTargetSpeed = NextSection.CurrentSignalIndex == 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex];
                        TargetSpeedUp = false;
                        if (LastSignal != ATCTargetSpeed && (ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex])
                            > (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex])) ORPlamp = true;
                    } else {
                        if (CurrentSection.CurrentSignalIndex > 9 && CurrentSection.CurrentSignalIndex < 49) {
                            if (CurrentSection.CurrentSignalIndex < NextSection.CurrentSignalIndex) {
                                ATCPattern = new SpeedLimit(ATCLimits[NextSection.CurrentSignalIndex], NextSection.Location - 25);
                                ATCTargetSpeed = ATCLimits[CurrentSection.CurrentSignalIndex];
                                TargetSpeedUp = true;
                                ORPlamp = false;
                            } else {
                                ATCPattern = new SpeedLimit(ATCLimits[NextSection.CurrentSignalIndex] == 0 ? 7 : ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex],
                                    ATCLimits[NextSection.CurrentSignalIndex] <= 0 ? NextSection.Location - 25 : NextSection.Location);
                                ATCTargetSpeed = ATCLimits[NextSection.CurrentSignalIndex];
                                TargetSpeedUp = false;
                                if (LastSignal != ATCTargetSpeed && (ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex])
                                    > (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex])) ORPlamp = true;
                            }   
                        }
                    }

                    if (LastSignal != ATCTargetSpeed) { ATC_Ding.StopAndPlay(); LastDingTime = TobuAts.state.Time.TotalMilliseconds; }
                    if (ATCTargetSpeed == 0 && TobuAts.state.Time.TotalMilliseconds - LastDingTime > 500) { ATC_Ding.Play(); LastDingTime = Config.LessInf; }
                    if (!DistanceDisplay && (Speed < ATCTargetSpeed || Speed > ATCLimitSpeed) && ATCTargetSpeed > 0) ORPlamp = false;
                    if (ATCTargetSpeed == (ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[CurrentSection.CurrentSignalIndex])) ORPlamp = false;

                    LastSignal = ATCTargetSpeed;

                    ATCLimitSpeed = (TargetSpeedUp && NextSection.Location - Location <= 25) ? (int)Math.Min(Config.MaxSpeed, ATCPattern.AtLocation(Location, SignalPatternDec)) :
                        (int)Math.Min(Config.MaxSpeed,Math.Min(ATCPattern.AtLocation(Location, SignalPatternDec), ATCLimits[CurrentSection.CurrentSignalIndex]));
                    ORPNeedle.Value = ATCLimitSpeed * 10;

                    ATC_P.Value = ORPlamp;

                    if (ATCLimitSpeed - Speed < 5 && ATCLimitSpeed > 0) ATC_PatternApproachBeep.Play();
                    else ATC_PatternApproachBeep.Stop();
                    ATC_PatternApproach.Value = ATCLimitSpeed - Speed < 5 && ATCLimitSpeed > 0;

                    if (StationStop) { 
                        ATC_StationStopAnnounce.Play();
                        if (Speed == 0) StationPattern = new SpeedLimit();
                    }

                    BrakeCommand = Math.Max(Speed > ATCLimitSpeed + 1.5 ? TobuAts.vehicleSpec.BrakeNotches : (Speed > ATCLimitSpeed ? 4 : 0),
                        StationStop ? (Speed > StationPattern.AtLocation(Location, Config.EBDec) ? TobuAts.vehicleSpec.BrakeNotches + 1 : 0) : 0);

                    if (ATCLimits[CurrentSection.CurrentSignalIndex] <= 0) {
                        BrakeCommand = TobuAts.vehicleSpec.BrakeNotches;
                        ORPNeedle.Value = 0;
                        ATCTargetSpeed = 0;
                    };

                    if (Config.ATCLimitPerLamp) {
                        ATC_01.Value = ATCTargetSpeed == 0;
                        ATC_10.Value = ATCTargetSpeed == 10;
                        ATC_15.Value = ATCTargetSpeed == 15;
                        ATC_20.Value = ATCTargetSpeed == 20;
                        ATC_25.Value = ATCTargetSpeed == 25;
                        ATC_30.Value = ATCTargetSpeed == 30;
                        ATC_35.Value = ATCTargetSpeed == 35;
                        ATC_40.Value = ATCTargetSpeed == 40;
                        ATC_45.Value = ATCTargetSpeed == 45;
                        ATC_50.Value = ATCTargetSpeed == 50;
                        ATC_55.Value = ATCTargetSpeed == 55;
                        ATC_60.Value = ATCTargetSpeed == 60;
                        ATC_65.Value = ATCTargetSpeed == 65;
                        ATC_70.Value = ATCTargetSpeed == 70;
                        ATC_75.Value = ATCTargetSpeed == 75;
                        ATC_80.Value = ATCTargetSpeed == 80;
                        ATC_85.Value = ATCTargetSpeed == 85;
                        ATC_90.Value = ATCTargetSpeed == 90;
                        ATC_95.Value = ATCTargetSpeed == 95;
                        ATC_100.Value = ATCTargetSpeed == 100;
                        ATC_110.Value = ATCTargetSpeed == 110;
                    } else {
                        ATCNeedle.Value = ATCTargetSpeed;
                        ATCNeedle_Disappear.Value = 0;
                    }

                    ATC_Stop.Value = ATCTargetSpeed == 0;
                    ATC_Proceed.Value = ATCTargetSpeed > 0;

                    ATC_StationStop.Value = StationStop;

                    ATC_SwitcherPosition.Value = (TobuAts.state.Time.TotalMilliseconds % 1000 < 500 && ATCTargetSpeed > 0) ? TrackPos : 0;
                    if (TrackPos > 0 && Location > TrackPosDisplayEndPos) TrackPos = 0;
                }
            }
        }

        public static void Dispose() {
            ATC_P.ValueChanged -= ValueChanged;
            ATC_Depot.ValueChanged -= ValueChanged;

            ATC_01.Dispose();
            ATC_10.Dispose();
            ATC_15.Dispose();
            ATC_20.Dispose();
            ATC_25.Dispose();
            ATC_30.Dispose();
            ATC_35.Dispose();
            ATC_40.Dispose();
            ATC_45.Dispose();
            ATC_50.Dispose();
            ATC_55.Dispose();
            ATC_60.Dispose();
            ATC_65.Dispose();
            ATC_70.Dispose();
            ATC_75.Dispose();
            ATC_80.Dispose();
            ATC_85.Dispose();
            ATC_90.Dispose();
            ATC_95.Dispose();
            ATC_100.Dispose();
            ATC_110.Dispose();

            ATC_Stop.Dispose();
            ATC_Proceed.Dispose();

            ATC_P.Dispose();
            ATC_X.Dispose();

            ORPNeedle.Dispose();
            ATCNeedle.Dispose();
            ATCNeedle_Disappear.Dispose();

            ATC_TobuATC.Dispose();
            ATC_Depot.Dispose();
            ATC_ServiceBrake.Dispose();
            ATC_EmergencyBrake.Dispose();
            //ATC_EmergencyOperation.Dispose();
            ATC_StationStop.Dispose();

            ATC_PatternApproach.Dispose();
            ATC_EndPointDistance.Dispose();

            ATC_SwitcherPosition.Dispose();
        }

        private static void ValueChanged(object sender, EventArgs e) {
            ATC_Ding.Play();
        }
    }
}
