using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BveTypes.ClassWrappers;
using BveEx.Extensions.Native;
using static System.Windows.Forms.AxHost;

namespace TobuSignal {
    internal partial class T_DATC {
        public static void ResetAll() {
            ATCPattern = SpeedPattern.inf;
            StationPattern = SpeedPattern.inf;
            LimitPattern = SpeedPattern.inf;
            TrackPos = 0;
            BrakeCommand = 0;
            ORPlamp = false;
            ATCPatternSpeed = 0;
            ATCTargetSpeed = 0;
            ValidSections = 0;
            EBUntilStop = false;
            ServiceBrake = false;
            LastDingTime = TimeSpan.Zero;
            BrakeStartTime = TimeSpan.Zero;
            InitializeStartTime = TimeSpan.Zero;
            TrackPosDisplayEndLocation = 0;
            ATCEnable = false;
            ATCswitchoverSection = false;

            BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_PatternApproachBeep = AtsSoundControlInstruction.Stop;
            ATC_StationStopAnnounce = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;

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

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_P = false;
            ATC_X = false;

            ORPNeedle = 0;
            ATCNeedle = 0;
            ATCNeedle_Disappear = true;

            ATC_TobuATC = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
            ATC_StationStop = false;

            ATC_PatternApproach = false;
            ATC_EndPointDistance = 0;

            ATC_SwitcherPosition = 0;
        }

        public static void Init(TimeSpan time) {
            ATCEnable = true;
            InitializeStartTime = time;
        }

        public static void SwitchFromATS() {
            ATCEnable = true;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 31:
                    ValidSections = 4;
                    if (e.Optional < 4) {
                        TrackPos = e.Optional;
                        TrackPosDisplayEndLocation = state.Location + e.Distance;
                    }
                    break;
                case 43:
                    if (ATCEnable)
                        StationPattern = new SpeedPattern(0, state.Location + e.Optional + 25);
                    break;
                case 44:
                    var lastLimitPattern = LimitPattern;
                    if (ATCEnable) {
                        LimitPatternSignalEndLocation = state.Location + e.Distance;
                        LimitPattern = new SpeedPattern(e.Optional % 1000, state.Location + e.Optional / 1000, lastLimitPattern.TargetSpeed);
                    }
                    break;
                case 45:
                    if (ATCEnable)
                        LimitPattern = SpeedPattern.inf;
                    break;
                //case 42:
                //    ATCswitchoverSection = true;
                //    break;
            }
        }

        public static void DoorOpened() {
            StationPattern = SpeedPattern.inf;
        }



        public static void Disable() {
            ATCswitchoverSection = false;
            ATCEnable = false;

            BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_PatternApproachBeep = AtsSoundControlInstruction.Stop;
            ATC_StationStopAnnounce = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;

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

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_P = false;
            ATC_X = false;

            ORPNeedle = 0;
            ATCNeedle = 0;
            ATCNeedle_Disappear = true;

            ATC_TobuATC = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
            ATC_StationStop = false;

            ATC_PatternApproach = false;
            ATC_EndPointDistance = 0;

            ATC_SwitcherPosition = 0;
        }

        public static void SignalUpdated() {

            //if (ATCEnable && ATCswitchoverSection) {
            //    InitializeStartTime = TimeSpan.MaxValue;
            //}
        }

        //private
        private static int[] pow10 = new int[] { 1, 10, 100, 1000, 10000, 100000 };
        private static int D(int src, int digit) {
            if (pow10[digit] > src) {
                return 10;
            } else if (digit == 0 && src == 0) {
                return 0;
            } else {
                return src / pow10[digit] % 10;
            }
        }
    }
}
