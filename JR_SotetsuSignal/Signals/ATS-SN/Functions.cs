using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JR_SotetsuSignal {
    internal partial class ATS_SN {
        public static void ResetAll() {
            TimeSpan InitStartTime = TimeSpan.Zero;
            WarnStartTime = TimeSpan.Zero;
            EB = false;
            Warn = false;
            ATSEnable = false;
            BrakeCommand = 0;
            SN_Power = false;
            SN_Action = false;

            SN_WarningBell = AtsSoundControlInstruction.Stop;
            SN_Chime = AtsSoundControlInstruction.Stop;
        }

        public static void Init(TimeSpan time) {
            InitStartTime = time;
            ATSEnable = true;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 1:
                    if (e.SignalIndex == 0) {
                        EB = true;
                        Warn = true;
                        SN_Chime = AtsSoundControlInstruction.PlayLooping;
                    }
                    break;
                case 0:
                    if (e.SignalIndex == 0) {
                        WarnStartTime = state.Time;
                        Warn = true;
                        SN_Chime = AtsSoundControlInstruction.PlayLooping;
                    }
                    break;
                case 9:
                    if (state.Speed > e.Optional) {
                        EB = true;
                        Warn = true;
                        SN_Chime = AtsSoundControlInstruction.PlayLooping;
                    }
                    break;
            }
        }

        public static void ResetBrake(HandleSet handles) {
            if (handles.BrakeNotch == JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1) EB = false;
        }

        public static void ConfirmButtonUp() {
            ConfirmButtonPressed = false;
        }

        public static void ResetWarn(HandleSet handles) {
            ConfirmButtonPressed = true;
            if (Warn && handles.BrakeNotch >= JR_SotetsuSignal.vehicleSpec.AtsNotch) Warn = false;
        }

        public static void ResetChime() {
            if (SN_Chime == AtsSoundControlInstruction.PlayLooping)
                SN_Chime = AtsSoundControlInstruction.Stop;
        }

        public static void Disable() {
            ATSEnable = false;
            BrakeCommand = 0;
            SN_Power = false;
            SN_Action = false;

            SN_WarningBell = AtsSoundControlInstruction.Stop;
            SN_Chime = AtsSoundControlInstruction.Stop;
        }
    }
}
