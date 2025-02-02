using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class CS_ATC {
        public static void ResetAll() {
            ORPPattern = SpeedPattern.inf;
            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
            ATCEnable = false;
            EBUntilStop = false;
            ServiceBrake = false;
            InitializeStartTime = TimeSpan.Zero;
            BrakeStartTime = TimeSpan.Zero;
            LastATCSpeed = 0;
            SignalAnn = false;
            inDepot = false;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_ORPBeep = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;
            ATC_WarningBell = AtsSoundControlInstruction.Stop;

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

            ATC_ATC = false;
            ATC_SignalAnn = false;
            ATC_Noset = false;
            ATC_TempLimit = false;

            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
        }

        public static void Init(TimeSpan time) {
            ATCEnable = true;
            InitializeStartTime = time;
        }

        public static void BeaconPassed(VehicleState state,BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 12:
                    if (ORPPattern != SpeedPattern.inf) ORPPattern.Location = state.Location + e.Optional;
                    break;
            }
        }

        public static void DisableAll() {
            ATCEnable = false;

            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_ORPBeep = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;
            ATC_WarningBell = AtsSoundControlInstruction.Stop;

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

            ATC_ATC = false;
            ATC_SignalAnn = false;
            ATC_Noset = false;
            ATC_TempLimit = false;

            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
        }

        private static void Disable_Noset_inDepot() {
            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_ORPBeep = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;
            ATC_WarningBell = AtsSoundControlInstruction.Stop;

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
            ATCNeedle_Disappear = true;

            ATC_ATC = false;
            ATC_SignalAnn = false;
            ATC_TempLimit = false;

            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
        }
    }
}
