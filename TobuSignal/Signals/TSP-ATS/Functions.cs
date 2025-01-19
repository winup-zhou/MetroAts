using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;

namespace TobuSignal {
    internal partial class TSP_ATS {
        public static void Init(double Time) {
            ATSEnable = true;
            InitializeStartTime = Time;
        }

        public static void SwitchFromATC() {
            ATSEnable = true;
        }

        public static void ResetAll() {
            ATSPattern = new SpeedPattern();
            MPPPattern = new SpeedPattern();
            SignalPattern = new SpeedPattern();
            LastBeaconPassTime = 0;
            ConfirmOperation = false;
            MPPEndLocation = 0;
            isDoorOpened = false;
            BrakeCommand = 0;
            EBType = EBTypes.Normal;
            InitializeStartTime = 0;

            ATS_TobuAts = false;
            ATS_ATSEmergencyBrake = false;
            ATS_EmergencyOperation = false;
            ATS_Confirm = false;
            ATS_60 = false;
            ATS_15 = false;

            ATSEnable = false;
        }

        public static void ResetBrake(VehicleState state, HandleSet handles) {
            if (state.Speed == 0 && handles.BrakeNotch == TobuSignal.vehicleSpec.BrakeNotches + 1)
                if (EBType == EBTypes.CannotReleaseUntilStop) EBType = EBTypes.Normal;
        }

        public static void Disable() {
            ATSEnable = false;
            BrakeCommand = 0;
            ATS_TobuAts = false;
            ATS_ATSEmergencyBrake = false;
            ATS_EmergencyOperation = false;
            ATS_Confirm = false;
            ATS_60 = false;
            ATS_15 = false;
        }

        public static void DoorOpened() {
            isDoorOpened = true;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 0:
                    if (e.SignalIndex == 0) {
                        SignalPattern = new SpeedPattern(15, state.Location + e.Distance);
                        EBType = EBTypes.CannotReleaseUntilStop;
                    } else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.TobuMaxSpeed, state.Location);
                    break;
                case 1:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedPattern(15, state.Location + 180);
                    else if (e.SignalIndex < 4 && e.SignalIndex > 0) SignalPattern = new SpeedPattern(60, state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.TobuMaxSpeed, state.Location);
                    break;
                case 2:
                    if (e.SignalIndex < 4) SignalPattern = new SpeedPattern(60, state.Location + 180);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.TobuMaxSpeed, state.Location);
                    break;
                case 3:
                    if (state.Time.TotalMilliseconds - LastBeaconPassTime < 1000) EBType = EBTypes.CannotReleaseUntilStop;
                    LastBeaconPassTime = state.Time.TotalMilliseconds;
                    break;
                case 5:
                    if (MPPPattern == SpeedPattern.inf)
                        MPPPattern = new SpeedPattern(60, state.Location + 400);
                    else if (MPPPattern.TargetSpeed == 60) {
                        MPPPattern = new SpeedPattern(15, state.Location + 100);
                        MPPEndLocation = state.Location + 105;
                    }
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
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedPattern(Config.TobuMaxSpeed, state.Location);
                    break;
            }
        }
    }
}
