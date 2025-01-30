using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JR_SotetsuSignal {
    internal partial class ATS_P {
        const double SignalDec = -2.445;
        private static double P_MaxSpeed = Config.LessInf, StationPatternEndLocation = Config.LessInf;
        private static TimeSpan InitStartTime = TimeSpan.Zero, P_OverrideStartTime = TimeSpan.Zero;
        private static bool EBBeaconPassed = false, BrakeUntilStop = false, BrakeCanRelease = false;
        private static bool lastP_PEnable = false, lastP_BrakeOverride = false;
        private static SpeedPattern P_SignalPattern = SpeedPattern.inf,
            P_StationStopPattern = SpeedPattern.inf,
            P_SpeedLimit1 = SpeedPattern.inf,//分岐器速度制限
            P_SpeedLimit2 = SpeedPattern.inf,//下り勾配速度制限
            P_SpeedLimit3 = SpeedPattern.inf,//曲線速度制限
            P_SpeedLimit4 = SpeedPattern.inf,//臨時速度制限
            P_SpeedLimit5 = SpeedPattern.inf;//誘導信号機速度制限

        public static bool ATSEnable = false;
        public static int BrakeCommand = 0;

        //panel
        public static bool P_Power, P_PatternApproach, P_BrakeActioned, P_EBActioned, P_BrakeOverride, P_PEnable, P_Fail;
        //SN_Power, SN_Action;
        public static AtsSoundControlInstruction P_Ding; //SN_WarningBell, SN_Chime;

        public static void Tick(VehicleState state) {
            P_Ding = AtsSoundControlInstruction.Continue;
            if (ATSEnable) {
                P_Power = true;
                if (state.Time.TotalMilliseconds - InitStartTime.TotalMilliseconds < 3000) {
                    P_Fail = true;
                    P_PEnable = false;
                    BrakeCommand = JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1;
                } else {
                    BrakeCommand = 0;
                    P_Fail = false;
                    if (P_PEnable) {
                        var lastPatternApproach = P_PatternApproach;
                        var lastPBrakeActioned = P_BrakeActioned;
                        var lastPEBActioned = P_EBActioned;

                        if (CalculatePattern1(state.Location + 50) - 5 < Math.Abs(state.Speed)
                            || CalculatePattern2(state.Location + 50) - 5 < Math.Abs(state.Speed)) {
                            P_PatternApproach = true;
                        } else if (P_MaxSpeed - 5 < Math.Abs(state.Speed)) {
                            P_PatternApproach = true;
                        } else if (P_EBActioned) {
                            P_PatternApproach = true;
                        } else if (state.Location > P_SpeedLimit1.Location && Math.Abs(state.Speed) > P_SpeedLimit1.TargetSpeed - 5) {
                            P_PatternApproach = true;
                        } else if (state.Location > P_SpeedLimit2.Location && Math.Abs(state.Speed) > P_SpeedLimit2.TargetSpeed - 5) {
                            P_PatternApproach = true;
                        } else if (state.Location > P_SpeedLimit3.Location && Math.Abs(state.Speed) > P_SpeedLimit3.TargetSpeed - 5) {
                            P_PatternApproach = true;
                        } else if (state.Location > P_SpeedLimit4.Location && Math.Abs(state.Speed) > P_SpeedLimit4.TargetSpeed - 5) {
                            P_PatternApproach = true;
                        } else if (state.Location > P_SpeedLimit5.Location && Math.Abs(state.Speed) > P_SpeedLimit5.TargetSpeed - 5) {
                            P_PatternApproach = true;
                        } else P_PatternApproach = false;

                        if (state.Location - 25 > P_SpeedLimit1.Location) P_SpeedLimit1 = SpeedPattern.inf;
                        if (state.Location > StationPatternEndLocation) P_StationStopPattern = SpeedPattern.inf;

                        if (Math.Abs(state.Speed) > CalculatePattern1(state.Location)) {
                            BrakeUntilStop = true;
                        } else if (Math.Abs(state.Speed) > CalculatePattern2(state.Location)) {
                            BrakeCanRelease = true;
                        }

                        if (state.Time.TotalMilliseconds - P_OverrideStartTime.TotalMilliseconds > 60000) {
                            if (BrakeUntilStop) {
                                P_BrakeActioned = true;
                                if (EBBeaconPassed) {
                                    P_EBActioned = true;
                                    BrakeCommand = JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1;
                                } else {
                                    BrakeCommand = JR_SotetsuSignal.vehicleSpec.BrakeNotches;
                                }
                            } else {
                                BrakeCommand = 0;
                                P_EBActioned = P_BrakeActioned = false;
                                if (BrakeCanRelease) {
                                    BrakeCommand = JR_SotetsuSignal.vehicleSpec.BrakeNotches;
                                    P_BrakeActioned = true;
                                    var ReleaseSpeed = Math.Min(P_SpeedLimit1.TargetSpeed, Math.Min(P_SpeedLimit2.TargetSpeed, Math.Min(P_SpeedLimit3.TargetSpeed, P_SpeedLimit4.TargetSpeed)));
                                    if (Math.Abs(state.Speed) < ReleaseSpeed - 5) BrakeCanRelease = false;
                                }
                            }
                        } else {
                            BrakeCommand = 0;
                            P_EBActioned = P_BrakeActioned = false;
                        }

                        if (lastPatternApproach != P_PatternApproach
                            || lastPBrakeActioned != P_BrakeActioned
                            || lastPEBActioned != P_EBActioned
                            || lastP_PEnable != P_PEnable
                            || lastP_BrakeOverride != P_BrakeOverride)
                            P_Ding = AtsSoundControlInstruction.Play;

                        lastP_PEnable = P_PEnable;
                        lastP_BrakeOverride = P_BrakeOverride;
                    }
                }
            } else {
                Disable();
            }

        }

    }
}
