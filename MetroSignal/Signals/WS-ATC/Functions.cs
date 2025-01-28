using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class WS_ATC {
        public static void ResetAll() {
            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
            ATCEnable = false;
            InitializeStartTime = TimeSpan.Zero;
            NeedConfirm = false;

            ATC_WSATC = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_Noset = false;
        }

        public static void Init(TimeSpan time) {
            ATCEnable = true;
            InitializeStartTime = time;
        }

        public static void ResetBrake(VehicleState state,HandleSet handles) {
            if(state.Speed == 0 && handles.BrakeNotch >= 4) {
                if(NeedConfirm)NeedConfirm = false;
            }
        }

        public static void DisableAll() {
            ATCEnable = false;
            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;

            ATC_WSATC = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            ATC_Noset = false;
        }

        private static void Disable_Noset() {
            ATC_WSATC = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
        }
    }
}
