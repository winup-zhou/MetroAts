using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;

namespace TobuSignal {
    internal partial class TSP_ATS {
        public static void Init(TimeSpan time) {
            ATSEnable = true;
            InitializeStartTime = time;
        }

        public static void SwitchFromATC() {
            ATSEnable = true;
        }

        public static void ResetAll() {
            ATSPattern = SpeedPattern.inf;
            MPPPattern = SpeedPattern.inf;
            SignalPattern = SpeedPattern.inf;
            LastBeaconPassTime = TimeSpan.Zero;
            NeedConfirmOperation = false;
            MPPEndLocation = 0;
            StopAnnounce = 0;
            isDoorOpened = false;
            BrakeCommand = 0;
            EBType = EBTypes.Normal;
            InitializeStartTime = TimeSpan.Zero;
            LastBeaconPassTime = TimeSpan.Zero;

            BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;

            ATS_TobuAts = false;
            ATS_ATSEmergencyBrake = false;
            ATS_EmergencyOperation = false;
            ATS_Confirm = false;
            ATS_StopAnnounce = false;
            ATS_60 = false;
            ATS_15 = false;

            ATSEnable = false;
        }

        public static void ResetBrake(VehicleState state, HandleSet handles) {
            if (Math.Abs(state.Speed) == 0 && handles.BrakeNotch == TobuSignal.vehicleSpec.BrakeNotches + 1)
                if (EBType == EBTypes.CannotReleaseUntilStop) EBType = EBTypes.Normal;
            if (NeedConfirmOperation) {
                NeedConfirmOperation = false;
                ATS_Confirm = true;
            }
        }

        public static void Disable() {
            ATSEnable = false;
            BrakeCommand = TobuSignal.vehicleSpec.BrakeNotches + 1;
            ATS_TobuAts = false;
            ATS_ATSEmergencyBrake = false;
            ATS_EmergencyOperation = false;
            ATS_StopAnnounce = false;
            ATS_Confirm = false;
            ATS_60 = false;
            ATS_15 = false;
        }

        public static void DoorOpened() {
            isDoorOpened = true;
            StopAnnounce = 0;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 0:
                    if (e.SignalIndex == 0) {
                        SignalPattern = new SpeedPattern(15, state.Location + e.Distance);
                        EBType = EBTypes.CannotReleaseUntilStop;
                        NeedConfirmOperation = true;
                    } else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.MaxSpeed, state.Location);
                    if (ATS_Confirm) ATS_Confirm = false;
                    break;
                case 1:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedPattern(15, state.Location + 180);
                    else if (e.SignalIndex < 4 && e.SignalIndex > 0) SignalPattern = new SpeedPattern(60, state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.MaxSpeed, state.Location);
                    if (ATS_Confirm) ATS_Confirm = false;
                    break;
                case 2:
                    if (e.SignalIndex < 4) SignalPattern = new SpeedPattern(60, state.Location + 180);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.MaxSpeed, state.Location);
                    if (ATS_Confirm) ATS_Confirm = false;
                    break;
                case 3:
                    if (state.Time.TotalMilliseconds - LastBeaconPassTime.TotalMilliseconds < 1000) EBType = EBTypes.CannotReleaseUntilStop;
                    LastBeaconPassTime = state.Time;
                    break;
                case 5:
                    if (StopAnnounce == 0) StopAnnounce = 1;
                    if (StopAnnounce == 1) StopAnnounce = 2;
                    break;
                case 9:
                    if (MPPPattern == SpeedPattern.inf)
                        MPPPattern = new SpeedPattern(60, state.Location + 237);
                    else if (MPPPattern.TargetSpeed == 60) {
                        MPPPattern = new SpeedPattern(15, state.Location + 111);
                        MPPEndLocation = state.Location + 116;
                    }
                    break;
                case 15:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedPattern(15, state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.MaxSpeed, state.Location);
                    if (ATS_Confirm) ATS_Confirm = false;
                    break;
            }
        }
    }
}
