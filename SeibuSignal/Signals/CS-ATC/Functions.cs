using BveEx.Extensions.Native;
using MetroAts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace SeibuSignal {
    internal partial class ATC {
        public static void ResetAll() {
            BrakeCommand = 0;
            ATCSpeed = 0;
            ATCEnable = false;
            InitializeStartTime = TimeSpan.Zero;
            inDepot = false;
            BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches + 1;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;
            ATC_WarningBell = AtsSoundControlInstruction.Stop;

            ATC_01 = false;
            ATC_25 = false;
            ATC_40 = false;
            ATC_55 = false;
            ATC_75 = false;
            ATC_90 = false;

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_X = false;

            ATCNeedle = 0;
            ATCNeedle_Disappear = true;

            ATC_ATC = false;

            ATC_Noset = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
        }

        public static void InitNow() {
            ATC_Ding = AtsSoundControlInstruction.Play;
            ATCEnable = true;
            ATC_ATC = true;
        }

        public static void Init(TimeSpan time) {
            ATCEnable = true;
            InitializeStartTime = time;
        }

        public static void DisableAll() {
            ATCEnable = false;

            BrakeCommand = 0;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;
            ATC_WarningBell = AtsSoundControlInstruction.Stop;

            ATC_01 = false;
            ATC_25 = false;
            ATC_40 = false;
            ATC_55 = false;
            ATC_75 = false;
            ATC_90 = false;

            ATC_Stop = false;
            ATC_Proceed = false;
            ATC_X = false;

            ATCNeedle = 0;
            ATCNeedle_Disappear = true;

            ATC_ATC = false;

            ATC_Noset = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
        }

        private static void Disable_Noset_inDepot() {
            BrakeCommand = 0;

            ATC_Ding = AtsSoundControlInstruction.Stop;
            ATC_EmergencyOperationAnnounce = AtsSoundControlInstruction.Stop;
            ATC_WarningBell = AtsSoundControlInstruction.Stop;

            ATC_01 = false;
            ATC_25 = false;
            ATC_40 = false;
            ATC_55 = false;
            ATC_75 = false;
            ATC_90 = false;

            ATC_Stop = false;
            ATC_Proceed = false;
            ATC_X = false;

            ATCNeedle = 0;
            ATCNeedle_Disappear = true;

            ATC_ATC = false;

            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_EmergencyOperation = false;
        }
    }
}
