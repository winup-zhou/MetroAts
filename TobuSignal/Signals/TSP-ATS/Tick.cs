using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuSignal {
    internal partial class TSP_ATS {
        //InternalValue -> ATS
        private static SpeedPattern ATSPattern = SpeedPattern.inf, MPPPattern = SpeedPattern.inf, SignalPattern = SpeedPattern.inf;
        private static double MPPEndLocation = 0;
        private static TimeSpan LastBeaconPassTime = TimeSpan.Zero, InitializeStartTime = TimeSpan.Zero;
        private static bool NeedConfirmOperation = false, isDoorOpened = false;
        private enum EBTypes {
            Normal = 0,
            CannotReleaseUntilStop,
            CanReleaseWithoutstop
        }

        private static EBTypes EBType = EBTypes.Normal;
        //0:no EB 1:EB until stop 2:EB can release without stop
        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;

        //panel -> ATS
        public static bool ATS_TobuAts, ATS_ATSEmergencyBrake, ATS_EmergencyOperation, ATS_Confirm, ATS_60, ATS_15;
        public static void Tick(VehicleState state) {
            if (ATSEnable) {
                ATS_TobuAts = true;
                if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 3000) {
                    ATS_ATSEmergencyBrake = true;
                    BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;
                } else {
                    if (state.Location > MPPEndLocation && isDoorOpened) {
                        MPPPattern = SpeedPattern.inf;
                        isDoorOpened = false;
                    }

                    ATSPattern = SignalPattern.AtLocation(state.Location, -3.5) < MPPPattern.AtLocation(state.Location, -3.5) ? SignalPattern : MPPPattern;

                    if (SignalPattern.AtLocation(state.Location, -3.5) < MPPPattern.AtLocation(state.Location, -3.5)) {
                        if (state.Speed > ATSPattern.AtLocation(state.Location, -3.5)) EBType = EBTypes.CanReleaseWithoutstop;
                    } else {
                        if (state.Speed > ATSPattern.AtLocation(state.Location, -3.5)) EBType = EBTypes.CannotReleaseUntilStop;
                    }

                    if (EBType == EBTypes.CanReleaseWithoutstop) {
                        if (state.Speed < ATSPattern.TargetSpeed) EBType = EBTypes.Normal;
                    }

                    ATS_60 = ATSPattern.TargetSpeed == 60;
                    ATS_15 = ATSPattern.TargetSpeed == 15;

                    ATS_ATSEmergencyBrake = EBType > 0;

                    BrakeCommand = EBType == EBTypes.Normal ? 0 : TobuSignal.vehicleSpec.BrakeNotches + 1;

                }
            } else {
                Disable();
            }
        }
    }
}
