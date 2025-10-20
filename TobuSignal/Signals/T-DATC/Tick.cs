using BveEx.Extensions.Native;
using BveEx.Extensions.PreTrainPatch;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace TobuSignal {
    internal partial class T_DATC {
        //InternalValue -> ATC
        public static double[] ATCLimits = { -2, 60, 60, 60, 60, -2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,
            -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -2, 35, -2, -1, 35, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };
        private static SpeedPattern ATCPattern = SpeedPattern.inf, StationPattern = SpeedPattern.inf, LimitPattern = SpeedPattern.inf;
        private static int TrackPos = 0, ValidSections = 0;
        private static bool EBUntilStop = false, ORPlamp = false, ServiceBrake = false;
        private static int ATCTargetSpeed = 0, ATCPatternSpeed = 0;
        private static int ZeroTargetSpeedBrakeSeconds = -1; //seconds to apply brake when target speed is 0
        private static double TrackPosDisplayEndLocation = 0, LimitPatternSignalEndLocation = 0, LimitPatternEndLocation = 0, LimitPatternSignalTriggerLoc = 0;
        private static TimeSpan InitializeStartTime = TimeSpan.Zero, LastDingTime = TimeSpan.Zero, BrakeStartTime = TimeSpan.Zero,
            ZeroTargetSpeedBrakeStartTime = TimeSpan.MaxValue;
        private const double SignalPatternDec = -2.25; //*10
        private const double StationPatternDec = -4.0;
        private static Section LastCurrentSection, currentSection;

        public static bool ATCEnable = false;
        public static int BrakeCommand = 0;

        private static double SignalIndexToSpeed(int index) {
            return ATCLimits[index] < 0 ? 0 : ATCLimits[index];
        }

        //panel -> ATC
        public static bool ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45, ATC_50, ATC_55, ATC_60, ATC_65, ATC_70,
            ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_105, ATC_110,
            ATC_Stop, ATC_Proceed, ATC_P, ATC_TobuATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach, ATCNeedle_Disappear, ATC_StationStop;
        public static int ORPNeedle, ATCNeedle, ATC_EndPointDistance, ATC_SwitcherPosition;

        public static AtsSoundControlInstruction ATC_Ding, ATC_PatternApproachBeep, ATC_StationStopAnnounce, ATC_EmergencyOperationAnnounce;

        public static void Tick(VehicleState state, SectionManager sectionManager, HandleSet handles) {
            if (ATCEnable) {
                int pointer = 0, pointer_ = 0;
                while (sectionManager.Sections[pointer].Location < state.Location) {
                    pointer++;
                    if (pointer >= sectionManager.Sections.Count) {
                        pointer = sectionManager.Sections.Count - 1;
                        break;
                    }
                }
                if (sectionManager.StopSignalSectionIndexes.Count > 0) {
                    while (sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location < state.Location) {
                        pointer_++;
                        if (pointer_ >= sectionManager.StopSignalSectionIndexes.Count) {
                            pointer_ = sectionManager.StopSignalSectionIndexes.Count - 1;
                            break;
                        }
                    }
                }

                Section stopSignalSection = sectionManager.Sections[0] as Section;
                LastCurrentSection = currentSection;
                currentSection = sectionManager.Sections[pointer > 0 ? pointer - 1 : 0] as Section;
                var NextSection = sectionManager.Sections[pointer] as Section;
                var PreviousSection = sectionManager.Sections[pointer > 1 ? pointer - 2 : 0] as Section;
                if (sectionManager.StopSignalSectionIndexes.Count > 0)
                    stopSignalSection = sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]] as Section;

                ATC_TobuATC = true;

                //Sound values reset
                ATC_Ding = ATC_PatternApproachBeep = ATC_StationStopAnnounce = ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Continue;

                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == TobuSignal.vehicleSpec.BrakeNotches + 1;

                if (currentSection.CurrentSignalIndex < 109 || currentSection.CurrentSignalIndex == 134 || currentSection.CurrentSignalIndex >= 149) {
                    //no valid ATC signal
                    ATC_X = true;
                    ATC_Stop = ATC_Proceed = ATC_P = false;
                    if (!Config.ATCLimitUseNeedle) {
                        ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
                        = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
                        = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
                        = ATC_95 = ATC_100 = ATC_110 = false;
                    } else {
                        ATCNeedle = 0;
                        ATCNeedle_Disappear = true;
                    }

                    BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;

                } else {
                    if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 1000) {
                        //Initializing
                        ATC_X = true;
                        ATC_Stop = ATC_Proceed = ATC_P = false;
                        if (!Config.ATCLimitUseNeedle) {
                            ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
                            = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
                            = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
                            = ATC_95 = ATC_100 = ATC_110 = false;
                        } else {
                            ATCNeedle = 0;
                            ATCNeedle_Disappear = true;
                        }
                        BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;
                        ValidSections = 0;

                        if (TrackPos > 0 && TrackPosDisplayEndLocation < NextSection.Location) TrackPosDisplayEndLocation = NextSection.Location;
                    } else {
                        //After initializing
                        if (ATC_X) {
                            ATC_X = false;
                            ATC_Ding = AtsSoundControlInstruction.Play;
                            LastDingTime = state.Time;
                        }

                        //Refresh Valid Sections
                        if (LastCurrentSection.Location != currentSection.Location) --ValidSections;

                        //開通情報
                        if (sectionManager.StopSignalSectionIndexes.Count > 0) {
                            if (sectionManager.StopSignalSectionIndexes[pointer_] - pointer < 4 && stopSignalSection.CurrentSignalIndex >= 109
                            && stopSignalSection.CurrentSignalIndex < 134 && ValidSections > 0) {
                                var pretrainLocation = stopSignalSection.Location - state.Location;
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
                        }

                        if (ValidSections < 3 && ValidSections > 0) {
                            ATCPattern = new SpeedPattern(SignalIndexToSpeed(currentSection.CurrentSignalIndex),
                                NextSection.Location - 25, Math.Min(Config.MaxSpeed, LimitPattern.AtLocation(state.Location, SignalPatternDec)));
                        } else if (ValidSections < 1) {
                            ATCPattern = new SpeedPattern(SignalIndexToSpeed(currentSection.CurrentSignalIndex),
                                currentSection.Location,
                                SignalIndexToSpeed(currentSection.CurrentSignalIndex));
                        } else {
                            ATCPattern = new SpeedPattern(0, stopSignalSection.Location - 25,
                                Math.Min(Config.MaxSpeed, LimitPattern.AtLocation(state.Location, SignalPatternDec)));
                        }

                        var lastATCTargetSpeed = ATCTargetSpeed;
                        ATCTargetSpeed = (int)SignalIndexToSpeed(currentSection.CurrentSignalIndex);
                        ATCPatternSpeed = (int)Math.Min(ATCPattern.AtLocation(state.Location, SignalPatternDec), LimitPattern.AtLocation(state.Location, SignalPatternDec));

                        //パターン接近
                        var lastATC_PatternApproach = ATC_PatternApproach;
                        ATC_PatternApproach = ATCPatternSpeed - Math.Abs(state.Speed) < 5 && ATCPatternSpeed > 0;
                        if (!lastATC_PatternApproach && ATC_PatternApproach)
                            ATC_PatternApproachBeep = AtsSoundControlInstruction.Play;

                        var lastATC_Depot = ATC_Depot;
                        ATC_Depot = currentSection.CurrentSignalIndex >= 138 && currentSection.CurrentSignalIndex <= 148;

                        //P表示灯
                        var lastORPlamp = ORPlamp;

                        if (ValidSections < 3 && ValidSections > 0 && ATCPattern.AtLocation(NextSection.Location - 26, SignalPatternDec) > SignalIndexToSpeed(currentSection.CurrentSignalIndex)) {
                            ORPlamp = true;
                        } else if (sectionManager.StopSignalSectionIndexes[pointer_] - pointer < 4 && ValidSections >= 3
                            && Math.Min(Config.MaxSpeed, LimitPattern.AtLocation(NextSection.Location - 26, SignalPatternDec)) > SignalIndexToSpeed(currentSection.CurrentSignalIndex)) {
                            ORPlamp = true;
                        } else if (LimitPattern != SpeedPattern.inf && state.Location < LimitPatternSignalEndLocation && currentSection.Location > LimitPatternSignalTriggerLoc) {
                            ORPlamp = true;
                        } else {
                            ORPlamp = false;
                        }

                        if (state.Speed > 0 && ZeroTargetSpeedBrakeSeconds != -1)
                            ZeroTargetSpeedBrakeStartTime = state.Time + new TimeSpan(0, 0, ZeroTargetSpeedBrakeSeconds);

                        if (currentSection.CurrentSignalIndex == 109
                            || (state.Time > ZeroTargetSpeedBrakeStartTime && currentSection.CurrentSignalIndex == 110)) {
                            ATCPatternSpeed = 0;
                            ATCTargetSpeed = 0;
                            ORPlamp = false;
                        }

                        if (ATCLimits[currentSection.CurrentSignalIndex] > 0) {
                            ZeroTargetSpeedBrakeSeconds = -1;
                            ZeroTargetSpeedBrakeStartTime = TimeSpan.MaxValue;
                        }

                        ATC_P = ORPlamp;

                        //ATCベル
                        if (lastATCTargetSpeed != ATCTargetSpeed || lastORPlamp != ORPlamp || lastATC_Depot != ATC_Depot) {
                            ATC_Ding = AtsSoundControlInstruction.Play;
                            LastDingTime = state.Time;
                            if (state.Time > ZeroTargetSpeedBrakeStartTime && currentSection.CurrentSignalIndex == 110) LastDingTime = TimeSpan.Zero;
                        }
                        if (ATCTargetSpeed == 0 && state.Time.TotalMilliseconds - LastDingTime.TotalMilliseconds > 500 && LastDingTime != TimeSpan.Zero) {
                            ATC_Ding = AtsSoundControlInstruction.Play;
                            LastDingTime = TimeSpan.Zero;
                        }

                        ORPNeedle = ((ATCPatternSpeed < 0) ? 0 : ATCPatternSpeed) * 10;

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
                            ATC_105 = ATCTargetSpeed == 105;
                            ATC_110 = ATCTargetSpeed == 110;
                        } else {
                            ATCNeedle = ATCTargetSpeed;
                            ATCNeedle_Disappear = false;
                        }

                        //進行・停止
                        ATC_Stop = ATCTargetSpeed == 0;
                        ATC_Proceed = ATCTargetSpeed > 0;

                        //駅停車
                        var lastATC_StationStop = ATC_StationStop;
                        ATC_StationStop = StationPattern != SpeedPattern.inf;
                        if (StationPattern != SpeedPattern.inf && !lastATC_StationStop && ATC_StationStop)
                            ATC_StationStopAnnounce = AtsSoundControlInstruction.Play;

                        //分岐器指示
                        ATC_SwitcherPosition = (state.Time.TotalMilliseconds % 1000 < 500 && ATCTargetSpeed > 0) ? TrackPos : 0;
                        if (TrackPos > 0 && state.Location > TrackPosDisplayEndLocation) {
                            TrackPos = 0;
                            TrackPosDisplayEndLocation = 0;
                        }

                        if (Math.Abs(state.Speed) > StationPattern.AtLocation(state.Location, StationPatternDec)) {
                            StationPattern.MaxSpeed = StationPattern.TargetSpeed = 15;
                            EBUntilStop = true;
                        }


                        if (state.Location > LimitPatternEndLocation && LimitPatternEndLocation != 0) {
                            LimitPatternEndLocation = 0;
                            LimitPattern = SpeedPattern.inf;
                        }

                        if (Math.Abs(state.Speed) > ATCPatternSpeed) {
                            if (Math.Abs(state.Speed) >= ATCPatternSpeed + 1.5) {
                                if (!ServiceBrake) ServiceBrake = true;
                                if (BrakeStartTime == TimeSpan.Zero) BrakeStartTime = state.Time;
                            } else {
                                ServiceBrake = false;
                                BrakeStartTime = TimeSpan.Zero;
                                BrakeCommand = (int)Math.Ceiling(TobuSignal.vehicleSpec.BrakeNotches * 0.5);
                            }
                        } else {
                            ServiceBrake = false;
                            BrakeCommand = 0;
                            BrakeStartTime = TimeSpan.Zero;
                        }

                        if (ServiceBrake || currentSection.CurrentSignalIndex == 109
                            || (ValidSections < 1 && ATCTargetSpeed == 0) || (state.Time > ZeroTargetSpeedBrakeStartTime && currentSection.CurrentSignalIndex == 110)) {
                            if (state.Time.TotalMilliseconds - BrakeStartTime.TotalMilliseconds < 1500)
                                BrakeCommand = (int)Math.Ceiling(TobuSignal.vehicleSpec.BrakeNotches * 0.5);
                            else BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches;
                        } else if (EBUntilStop) {
                            BrakeCommand = Math.Max(BrakeCommand, TobuSignal.vehicleSpec.BrakeNotches + 1);
                            if (Math.Abs(state.Speed) == 0 && handles.BrakeNotch >= TobuSignal.vehicleSpec.BrakeNotches) EBUntilStop = false;
                        }
                    }
                }
            } else {
                Disable();
            }
        }
    }
}
