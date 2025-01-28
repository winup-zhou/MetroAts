using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class WS_ATC {
        private static bool NeedConfirm = false;
        private static TimeSpan InitializeStartTime = TimeSpan.Zero;

        public static int BrakeCommand = 0;
        public static bool ATCEnable = false;
        public static bool ATC_WSATC, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_Noset;

        private static int IndexToSpeed(int index) {
            if (index == 50) {
                return 0;
            } else if (index == 51) {
                return 25;
            } else if (index == 51) {
                return 40;
            } else if (index == 53) {
                return 65;
            } else if (index == 54) {
                return (int)Config.LessInf;
            } else {
                return 0;
            }
        }

        public static void Tick(VehicleState state, Section CurrentSection, bool Noset) {
            if (ATCEnable) {
                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == MetroSignal.vehicleSpec.BrakeNotches + 1;
                if (CurrentSection.CurrentSignalIndex < 50 || CurrentSection.CurrentSignalIndex > 54) {
                    if (Noset) {
                        ATC_Noset = true;
                        Disable_Noset();
                    } else {
                        ATC_Noset = false;
                        NeedConfirm = true;
                        BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                    }
                } else {
                    if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 1000) {
                        BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                    } else {
                        ATC_WSATC = true;
                        if (state.Speed > IndexToSpeed(CurrentSection.CurrentSignalIndex) && IndexToSpeed(CurrentSection.CurrentSignalIndex) == 0)
                            NeedConfirm = true;
                        if (state.Speed > IndexToSpeed(CurrentSection.CurrentSignalIndex) || NeedConfirm)
                            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches;
                    }
                }
            } else {
                DisableAll();
            }
        }
    }
}
