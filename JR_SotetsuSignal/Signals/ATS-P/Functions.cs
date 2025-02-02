using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JR_SotetsuSignal {
    internal partial class ATS_P {
        public static void ResetAll() {
            RollbackDetect = false;
            BrakeUntilStop = false;
            BrakeCanRelease = false;
            BrakeCommand = JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1;
            EBBeaconPassed = false;
            P_OverrideStartTime = TimeSpan.Zero;
            P_MaxSpeed = Config.LessInf;
            StationPatternEndLocation = Config.LessInf;
            InitStartTime = TimeSpan.Zero;
            ATSEnable = false;
            lastP_PEnable = false;
            lastP_BrakeOverride = false;
            P_SignalPattern = SpeedPattern.inf;
            P_StationStopPattern = SpeedPattern.inf;
            P_SpeedLimit1 = SpeedPattern.inf;//分岐器速度制限
            P_SpeedLimit2 = SpeedPattern.inf;//下り勾配速度制限
            P_SpeedLimit3 = SpeedPattern.inf;//曲線速度制限
            P_SpeedLimit4 = SpeedPattern.inf;//臨時速度制限
            P_SpeedLimit5 = SpeedPattern.inf;//誘導信号機速度制限

            P_Power = false; 
            P_PatternApproach = false;
            P_BrakeActioned = false; 
            P_EBActioned = false; 
            P_BrakeOverride = false;
            P_PEnable = false;
            P_Fail = false;

            P_Ding = AtsSoundControlInstruction.Stop;
        }

        public static void SwitchToSN() {
            lastP_PEnable = P_PEnable;
            if (P_PEnable) P_PEnable = false;
        }

        public static void Init(TimeSpan time) {
            InitStartTime = time;
            ATSEnable = true;
        }

        public static void DoorOpened(VehicleState state) {
            StationPatternEndLocation = state.Location + 5;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 3:
                    if (ATSEnable) {
                        lastP_PEnable = P_PEnable;
                        if (!P_PEnable) P_PEnable = true;
                        P_SignalPattern = new SpeedPattern(-1, e.Distance + state.Location, P_MaxSpeed);
                        EBBeaconPassed = false;
                    }
                    break;
                case 4:
                    if (ATSEnable) {
                        lastP_PEnable = P_PEnable;
                        if (!P_PEnable) P_PEnable = true;
                        P_SignalPattern = new SpeedPattern(-1, e.Distance + state.Location, P_MaxSpeed);
                        if (e.Distance < 50) {
                            BrakeUntilStop = true;
                            EBBeaconPassed = true;
                        }
                    }
                    break;
                case 5:
                    if (ATSEnable) {
                        lastP_PEnable = P_PEnable;
                        if (!P_PEnable) P_PEnable = true;
                        P_SignalPattern = new SpeedPattern(-1, e.Distance + state.Location, P_MaxSpeed);
                        if (e.Distance < 50) {
                            BrakeUntilStop = true;
                            EBBeaconPassed = true;
                        }
                    }
                    break;
                case 6:
                    if (ATSEnable) P_SpeedLimit1 = new SpeedPattern(e.Optional % 1000, e.Optional / 1000 + state.Location, P_MaxSpeed);
                    break;
                case 7:
                    if (ATSEnable) P_MaxSpeed = e.Optional;
                    break;
                case 8:
                    if (ATSEnable) P_SpeedLimit2 = new SpeedPattern(e.Optional % 1000, e.Optional / 1000 + state.Location, P_MaxSpeed);
                    break;
                case 9:
                    if (ATSEnable && e.Optional > 999)
                        P_SpeedLimit3 = new SpeedPattern(e.Optional % 1000, e.Optional / 1000 + state.Location, P_MaxSpeed);
                    break;
                case 10:
                    if (ATSEnable) P_SpeedLimit4 = new SpeedPattern(e.Optional % 1000, e.Optional / 1000 + state.Location, P_MaxSpeed);
                    break;
                case 11:
                    if (ATSEnable) P_SpeedLimit5 = new SpeedPattern(e.Optional % 1000, e.Optional / 1000 + state.Location, P_MaxSpeed);
                    break;
                case 12:
                    if (ATSEnable) P_StationStopPattern = new SpeedPattern(-1, e.Optional + 25);
                    break;
                case 16:
                    if (ATSEnable) P_SpeedLimit1 = SpeedPattern.inf;
                    break;
                case 18:
                    if (ATSEnable) P_SpeedLimit2 = SpeedPattern.inf;
                    break;
                case 19:
                    if (ATSEnable) P_SpeedLimit3 = SpeedPattern.inf;
                    break;
                case 20:
                    if (ATSEnable) P_SpeedLimit4 = SpeedPattern.inf;
                    break;
                case 21:
                    if (ATSEnable) P_SpeedLimit5 = SpeedPattern.inf;
                    break;
            }
        }

        public static void ResetBrake(HandleSet handles) {
            if (EBBeaconPassed) {
                if (handles.BrakeNotch == JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1) {
                    EBBeaconPassed = false;
                    BrakeUntilStop = false;
                    P_SignalPattern.TargetSpeed = 15;
                }
            } else if (RollbackDetect) {
                if (handles.BrakeNotch == JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1) {
                    RollbackDetect = false;
                    BrakeUntilStop = false;
                }
            } else if (handles.BrakeNotch == JR_SotetsuSignal.vehicleSpec.BrakeNotches) {
                BrakeUntilStop = false;
            }
        }

        public static void BrakeOverride(VehicleState state) {
            if (ATSEnable) {
                lastP_BrakeOverride = P_BrakeOverride;
                if (!P_BrakeOverride) {
                    P_OverrideStartTime = state.Time;
                    P_BrakeOverride = true;
                } else {
                    P_BrakeOverride = false;
                    P_OverrideStartTime = TimeSpan.Zero;
                }
            }
        }

        public static void Disable() {
            BrakeCommand = JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1;
            ATSEnable = false;

            P_Power = false;
            P_PatternApproach = false;
            P_BrakeActioned = false;
            P_EBActioned = false;
            P_BrakeOverride = false;
            P_PEnable = false;
            P_Fail = false;

            P_Ding = AtsSoundControlInstruction.Stop; ;
        }

        private static double CalculatePattern1(double Location) {
            var ResultSpeed = Config.LessInf;
            ResultSpeed = Math.Min(ResultSpeed, P_SignalPattern.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_StationStopPattern.AtLocation(Location, SignalDec));
            return ResultSpeed;
        }

        private static double CalculatePattern2(double Location) { //速度制限
            var ResultSpeed = Config.LessInf;
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit1.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit2.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit3.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit4.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit5.AtLocation(Location, SignalDec));
            return ResultSpeed;
        }
    }
}

