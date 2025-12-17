using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class CS_DATC {
        public static double[] ATCLimits = { -2, 60, 60, 60, 60, -2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,-2,
            -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -2, 35, -2, -1, 35, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };

        private static SpeedPattern ATCPattern = SpeedPattern.inf;
        private static int LastATCSpeed = 0, ATCSpeed = 0;
        private static bool EBUntilStop = false, ServiceBrake = false, SignalAnn = false, inDepot = false;
        private static double M01SectionEntrySpeed = -25;
        private const double PatternDec = -2.3; //*10
        private const double StationPatternDec = -4.0;
        private static TimeSpan InitializeStartTime = TimeSpan.Zero, BrakeStartTime = TimeSpan.Zero;

        public static int BrakeCommand = 0;
        public static bool ATCEnable = false;

        //panel -> ATC
        public static bool ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45,
            ATC_50, ATC_55, ATC_60, ATC_65, ATC_70, ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_105, ATC_110, ATC_Stop, ATC_Proceed,
            ATC_P, ATC_ATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_Noset, ATC_TempLimit, ATCNeedle_Disappear;
        public static int ORPNeedle, ATCNeedle;
        public static AtsSoundControlInstruction ATC_Ding, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        public static void Tick(VehicleState state, Section CurrentSection, Section NextSection, HandleSet handles, bool Noset, bool InDepot) {
            if (ATCEnable) {
                ATC_Ding = AtsSoundControlInstruction.Continue;
                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == MetroSignal.vehicleSpec.BrakeNotches + 1;

                if (CurrentSection.CurrentSignalIndex < 109 || CurrentSection.CurrentSignalIndex == 134 || CurrentSection.CurrentSignalIndex >= 149) {
                    if (InDepot) {
                        ATC_Depot = true;
                        Disable_Noset_inDepot();
                    } else if (Noset) {
                        ATC_Noset = true;
                        Disable_Noset_inDepot();
                    } else {
                        ATC_Noset = false;
                        ATC_Depot = false;
                        if (!ATC_X) ATC_Ding = AtsSoundControlInstruction.Play;
                        ATC_X = true;
                        ATC_Stop = ATC_Proceed = false;
                        if (!Config.ATCLimitUseNeedle) {
                            ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
                            = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
                            = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
                            = ATC_95 = ATC_100 = ATC_110 = false;
                        } else {
                            ATCNeedle = 0;
                            ATCNeedle_Disappear = true;
                        }
                        BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                    }
                } else {
                    if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 3000) {
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
                        BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                    } else {
                        ATC_ATC = true;
                        BrakeCommand = 0;

                        var lastinDepot = inDepot;
                        inDepot = CurrentSection.CurrentSignalIndex >= 138 && CurrentSection.CurrentSignalIndex <= 148;
                        if (lastinDepot != inDepot) ATC_Ding = AtsSoundControlInstruction.Play;

                        if (Noset) {
                            ATC_Noset = true;
                            ATC_WarningBell = AtsSoundControlInstruction.PlayLooping;
                        } else {
                            ATC_Noset = false;
                            ATC_WarningBell = AtsSoundControlInstruction.PlayLooping;
                        }

                        if (ATC_WarningBell == AtsSoundControlInstruction.PlayLooping && !Noset)
                            ATC_WarningBell = AtsSoundControlInstruction.Stop;

                        var lastATC_X = ATC_X;
                        if (ATC_X) {
                            ATC_X = false;
                        }

                        var lastATCSpeed = ATCSpeed;

                        if (inDepot) {
                            ATCSpeed = ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? -1 : (int)ATCLimits[CurrentSection.CurrentSignalIndex];
                        } else {
                            if (CurrentSection.CurrentSignalIndex == 109) {
                                ATC_X = true;
                            } else if (CurrentSection.CurrentSignalIndex == 110) {

                            } else if (ATCLimits[CurrentSection.CurrentSignalIndex] > 0) {

                            }
                        }

                        var ORPSpeed = 0.0;
                        if (CurrentSection.CurrentSignalIndex == 135 || CurrentSection.CurrentSignalIndex == 138) {
                            if (ATCPattern == SpeedPattern.inf) {
                                ATCPattern = new SpeedPattern(0, NextSection.Location);
                                LastATCSpeed = ATCSpeed;
                            }
                            ORPSpeed = Math.Min(ATCPattern.AtLocation(state.Location, PatternDec), LastATCSpeed);
                        } else {
                            ATCPattern = SpeedPattern.inf;
                        }

                        

                        if (ATCLimits[CurrentSection.CurrentSignalIndex] == 0 && ATCSpeed != -1) {
                            BrakeCommand = (int)MetroSignal.vehicleSpec.BrakeNotches;
                            ATCSpeed = 0;
                        }

                        if ((lastATC_X != ATC_X || lastATCSpeed != ATCSpeed) && !inDepot) ATC_Ding = AtsSoundControlInstruction.Play;

                        ATC_Depot = inDepot;

                        if (Math.Abs(state.Speed) > ATCPattern.AtLocation(state.Location, PatternDec)
                            && (CurrentSection.CurrentSignalIndex == 135 || CurrentSection.CurrentSignalIndex == 138))
                            EBUntilStop = true;

                        if (Math.Abs(state.Speed) > ATCSpeed + 2.5 && ATCSpeed != -1) {
                            if (!ServiceBrake) ServiceBrake = true;
                        } else {
                            ServiceBrake = false;
                        }

                        if (ServiceBrake) BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches;


                        if (EBUntilStop) {
                            BrakeCommand = Math.Max(BrakeCommand, MetroSignal.vehicleSpec.BrakeNotches + 1);
                            if (Math.Abs(state.Speed) == 0 && handles.BrakeNotch >= MetroSignal.vehicleSpec.BrakeNotches) EBUntilStop = false;
                        }

                        if (ATCPattern != SpeedPattern.inf && (Math.Abs(state.Speed) > ATCPattern.AtLocation(state.Location, PatternDec)
                            || Math.Abs(state.Speed) < 5 || ATCPattern.AtLocation(state.Location, PatternDec) < 7.5)) {
                            ATCPattern = new SpeedPattern(7.5, state.Location, 7.5);
                        }

                        if (ATCPattern != SpeedPattern.inf) {
                            if (!Config.ORPUseNeedle) {
                                ATC_P = state.Time.TotalMilliseconds % 1000 < 500;
                            } else {
                                ATC_P = true;
                                ORPNeedle = (int)ORPSpeed * 10;
                            }
                        } else {
                            ORPNeedle = (int)ORPSpeed * 10;
                            ATC_P = false;
                        }

                        //ATC速度指示
                        if (!inDepot) {
                            if (!Config.ATCLimitUseNeedle) {
                                ATC_01 = ATCSpeed == 0;
                                ATC_10 = ATCSpeed == 10;
                                ATC_15 = ATCSpeed == 15;
                                ATC_20 = ATCSpeed == 20;
                                ATC_25 = ATCSpeed == 25;
                                ATC_30 = ATCSpeed == 30;
                                ATC_35 = ATCSpeed == 35;
                                ATC_40 = ATCSpeed == 40;
                                ATC_45 = ATCSpeed == 45;
                                ATC_50 = ATCSpeed == 50;
                                ATC_55 = ATCSpeed == 55;
                                ATC_60 = ATCSpeed == 60;
                                ATC_65 = ATCSpeed == 65;
                                ATC_70 = ATCSpeed == 70;
                                ATC_75 = ATCSpeed == 75;
                                ATC_80 = ATCSpeed == 80;
                                ATC_85 = ATCSpeed == 85;
                                ATC_90 = ATCSpeed == 90;
                                ATC_95 = ATCSpeed == 95;
                                ATC_100 = ATCSpeed == 100;
                                ATC_105 = ATCSpeed == 105;
                                ATC_110 = ATCSpeed == 110;
                            } else {
                                ATCNeedle = ATCSpeed;
                                ATCNeedle_Disappear = (ATCSpeed != -1 || ORPNeedle > 0);
                            }

                            //進行・停止
                            ATC_Stop = ATCSpeed == 0 || ATCSpeed == -1;
                            ATC_Proceed = ATCSpeed > 0;
                        } else {
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
                            ATC_105 = false;
                            ATC_110 = false;

                            ATCNeedle = 0;
                            ATCNeedle_Disappear = true;

                            //進行・停止
                            ATC_Stop = ATC_Proceed = false;
                        }
                    }
                }
            } else {
                DisableAll();
            }
        }
    }
}
