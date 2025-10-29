using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeibuSignal {
    internal partial class ATC {
        public static int[] ATCLimits = { -2, -2, -2, -2, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -2, -2, -2, -1, -2, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };

        private static TimeSpan InitializeStartTime = TimeSpan.Zero;
        private static bool inDepot = false;
        public static bool ATCEnable = false;
        public static int BrakeCommand = 0, ATCSpeed = 0;

        //panel -> ATC
        public static bool ATC_X, ATC_01, ATC_25, ATC_40, ATC_55, ATC_75, ATC_90, ATC_Stop, ATC_Proceed,
            ATC_ATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_Noset, ATCNeedle_Disappear;
        public static int ATCNeedle;
        public static AtsSoundControlInstruction ATC_Ding, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        private static bool ValidATCCode(int index) {
            var speed = ATCLimits[index] < 0 ? 0 : ATCLimits[index];
            return speed == 0 || speed == 25 || speed == 40 || speed == 55 || speed == 75 || speed == 90;
        }

        public static void Tick(VehicleState state, HandleSet handles, Section CurrentSection, bool Noset, bool InDepot) {
            if (ATCEnable) {
                ATC_Ding = AtsSoundControlInstruction.Continue;
                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == SeibuSignal.vehicleSpec.BrakeNotches + 1;

                if (CurrentSection.CurrentSignalIndex <= 9 || !ValidATCCode(CurrentSection.CurrentSignalIndex) || CurrentSection.CurrentSignalIndex == 34 || CurrentSection.CurrentSignalIndex >= 49) {
                    if (InDepot) {
                        ATC_Depot = true;
                        Disable_Noset_inDepot();
                    } else if (Noset) {
                        ATC_Noset = true;
                        Disable_Noset_inDepot();
                    } else {
                        ATC_Noset = false;
                        ATC_Depot = false;
                        if(!ATC_X)ATC_Ding = AtsSoundControlInstruction.Play;
                        ATC_X = true;
                        ATC_Stop = ATC_Proceed = false;
                        if (!Config.ATCLimitUseNeedle) {
                            ATC_01 = ATC_25 = ATC_40 = ATC_55 = ATC_75 = ATC_90 = false;
                        } else {
                            ATCNeedle = 0;
                            ATCNeedle_Disappear = true;
                        }
                        BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches + 1;
                    }
                } else {
                    if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 3000) {
                        ATC_X = true;
                        ATC_Stop = ATC_Proceed = false;
                        if (!Config.ATCLimitUseNeedle) {
                            ATC_01 = ATC_25 = ATC_40 = ATC_55 = ATC_75 = ATC_90 = false;
                        } else {
                            ATCNeedle = 0;
                            ATCNeedle_Disappear = true;
                        }
                        BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches + 1;
                    } else {
                        ATC_ATC = true;
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
                        ATCSpeed = ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? -1 : ATCLimits[CurrentSection.CurrentSignalIndex];

                        if (ATCLimits[CurrentSection.CurrentSignalIndex] == 0 && ATCSpeed != -1) {
                            BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches;
                            ATCSpeed = 0;
                        }

                        if (lastATCSpeed != ATCSpeed && !inDepot) ATC_Ding = AtsSoundControlInstruction.Play;

                        if (inDepot) {
                            ATC_Depot = true;
                        } else {
                            ATC_Depot = false;
                        }

                        if (Math.Abs(state.Speed) > ATCSpeed + 2.5 && ATCSpeed != -1) {
                            BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches;
                        }

                        //ATC速度指示
                        if (!inDepot) {
                            if (!Config.ATCLimitUseNeedle) {
                                ATC_01 = ATCSpeed == 0;
                                ATC_25 = ATCSpeed == 25;
                                ATC_40 = ATCSpeed == 40;
                                ATC_55 = ATCSpeed == 55;
                                ATC_75 = ATCSpeed == 75;
                                ATC_90 = ATCSpeed == 90;
                            } else {
                                ATCNeedle = ATCSpeed;
                                ATCNeedle_Disappear = ATCSpeed != -1;
                            }

                            //進行・停止
                            ATC_Stop = ATCSpeed == 0 || ATCSpeed == -1;
                            ATC_Proceed = ATCSpeed > 0;
                        } else {
                            ATC_01 = ATC_25 = ATC_40 = ATC_55 = ATC_75 = ATC_90 = false;

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
