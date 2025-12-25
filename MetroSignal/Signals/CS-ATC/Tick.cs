using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class CS_ATC {
        public static int[] ATCLimits = { -2, -2, -2, -2, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -2, -2, -2, -1, -2, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };

        private static SpeedPattern ORPPattern = SpeedPattern.inf;
        private static int LastATCSpeed = 0, ATCSpeed = 0;
        private static bool EBUntilStop = false, ServiceBrake = false, SignalAnn = false, inDepot = false;
        private const double ORPPatternDec = -2.3; //*10
        private const double StationPatternDec = -4.0;
        private static TimeSpan InitializeStartTime = TimeSpan.Zero, BrakeStartTime = TimeSpan.Zero;

        public static int BrakeCommand = 0;
        public static bool ATCEnable = false;

        //panel -> ATC
        public static bool ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45,
            ATC_50, ATC_55, ATC_60, ATC_65, ATC_70, ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_105, ATC_110, ATC_Stop, ATC_Proceed,
            ATC_P, ATC_ATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation,
            ATC_SignalAnn, ATC_Noset, ATC_TempLimit, ATCNeedle_Disappear;
        public static int ORPNeedle, ATCNeedle;
        public static AtsSoundControlInstruction ATC_Ding, ATC_ORPBeep, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        public static void Tick(VehicleState state, Section CurrentSection, Section NextSection, HandleSet handles, bool Noset, bool InDepot) {
            if (ATCEnable) {
                ATC_Ding = AtsSoundControlInstruction.Continue;
                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == MetroSignal.vehicleSpec.BrakeNotches + 1;

                if (CurrentSection.CurrentSignalIndex <= 9 || CurrentSection.CurrentSignalIndex == 34 || CurrentSection.CurrentSignalIndex >= 49) {
                    ATC_ORPBeep = AtsSoundControlInstruction.Stop;
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
                        var lastATC_ATC = ATC_ATC;
                        ATC_ATC = true;
                        if (!lastATC_ATC && ATC_ATC) ATC_Ding = AtsSoundControlInstruction.Play;
                        BrakeCommand = 0;

                        var lastinDepot = inDepot;
                        inDepot = CurrentSection.CurrentSignalIndex >= 38 && CurrentSection.CurrentSignalIndex <= 48;
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

                        if (ATC_X) {
                            ATC_X = false;
                            ATC_Ding = AtsSoundControlInstruction.Play;
                        }

                        var lastATCSpeed = ATCSpeed;

                        var ORPSpeed = 0.0;
                        if (CurrentSection.CurrentSignalIndex == 35 || CurrentSection.CurrentSignalIndex == 38) {
                            if (ORPPattern == SpeedPattern.inf) {
                                ORPPattern = new SpeedPattern(0, NextSection.Location);
                                LastATCSpeed = ATCSpeed == 0 ? 7 : ATCSpeed;
                            }
                            ORPSpeed = Math.Min(ORPPattern.AtLocation(state.Location, ORPPatternDec), LastATCSpeed);
                            if (!Config.ORPUseNeedle) {
                                if (ORPSpeed - Math.Abs(state.Speed) < 5 || ORPSpeed == 7.5) ATC_ORPBeep = AtsSoundControlInstruction.PlayLooping;
                                else ATC_ORPBeep = AtsSoundControlInstruction.Stop;
                            }
                        } else {
                            ORPPattern = SpeedPattern.inf;
                            if (ATC_ORPBeep == AtsSoundControlInstruction.PlayLooping)
                                ATC_ORPBeep = AtsSoundControlInstruction.Stop;
                        }

                        ATCSpeed = ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? -1 : ATCLimits[CurrentSection.CurrentSignalIndex];

                        if (ATCLimits[CurrentSection.CurrentSignalIndex] == 0 && ATCSpeed != -1) {
                            BrakeCommand = (int)Math.Ceiling(MetroSignal.vehicleSpec.BrakeNotches * 0.5);
                            ATCSpeed = 0;
                        }

                        if (lastATCSpeed != ATCSpeed && !inDepot) ATC_Ding = AtsSoundControlInstruction.Play;

                        ATC_Depot = inDepot;

                        var lastAnn = SignalAnn;
                        SignalAnn = ATCSpeed > (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex]) && !inDepot;
                        ATC_SignalAnn = SignalAnn;

                        if (Math.Abs(state.Speed) > ORPPattern.AtLocation(state.Location, ORPPatternDec))
                            EBUntilStop = true;

                        if (Math.Abs(state.Speed) > ATCSpeed + 2.5 && ATCSpeed != -1) {
                            if (Math.Abs(state.Speed) >= ATCSpeed + 5) {
                                if (!ServiceBrake) ServiceBrake = true;
                                if (BrakeStartTime == TimeSpan.Zero) BrakeStartTime = state.Time;
                            } else {
                                ServiceBrake = false;
                                BrakeStartTime = TimeSpan.Zero;
                                BrakeCommand = (int)Math.Ceiling(MetroSignal.vehicleSpec.BrakeNotches * 0.5);
                            }
                        } else {
                            ServiceBrake = false;
                            BrakeStartTime = TimeSpan.Zero;
                        }

                        if (ServiceBrake) {
                            if (state.Time.TotalMilliseconds - BrakeStartTime.TotalMilliseconds < 1500)
                                BrakeCommand = (int)Math.Ceiling(MetroSignal.vehicleSpec.BrakeNotches * 0.5);
                            else BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches;
                        }

                        if (EBUntilStop) {
                            BrakeCommand = Math.Max(BrakeCommand, MetroSignal.vehicleSpec.BrakeNotches + 1);
                            if (Math.Abs(state.Speed) == 0 && handles.BrakeNotch >= MetroSignal.vehicleSpec.BrakeNotches) EBUntilStop = false;
                        }

                        if (ORPPattern != SpeedPattern.inf && (Math.Abs(state.Speed) > ORPPattern.AtLocation(state.Location, ORPPatternDec) || Math.Abs(state.Speed) < 5 || ORPPattern.AtLocation(state.Location, ORPPatternDec) < 7.5)) {
                            ORPPattern = new SpeedPattern(7.5, state.Location, 7.5);
                        }

                        if (ORPPattern != SpeedPattern.inf) {
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
