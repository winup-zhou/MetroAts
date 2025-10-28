using BveEx.Extensions.Native.Input;
using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BveTypes.ClassWrappers;

namespace SeibuSignal {
    internal partial class SeibuATS {
        public static void Init(TimeSpan time) {
            InitializeStartTime = time;
            ATSEnable = true;
        }

        public static void ResetAll() {
            ATS_Power = false;
            ATS_EB = false;
            ATS_Limit = false;
            ATS_Stop = false;
            ATS_Confirm = false;

            ATS_StopAnnounce = AtsSoundControlInstruction.Stop;
            ATS_EBAnnounce = AtsSoundControlInstruction.Stop;

            B1Pattern = SpeedPattern.inf;
            B2Pattern = SpeedPattern.inf;
            StopPattern = SpeedPattern.inf;
            LimitPattern = SpeedPattern.inf;

            InitializeStartTime = TimeSpan.Zero;
            ATSEnable = false;
            MaxOver95 = false;
            NeedConfirm = false;
            MaxOver95 = false;
            lastMaxOver95 = false;
            B1MonitorSectionLocation = 0;
            B2MonitorSectionLocation = 0;
            B1Speed = 0;
            B2Speed = 0;
            BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches + 1;
            EBType = EBTypes.Normal;
        }

        public static void DoorOpened() {
            if (StopPattern != SpeedPattern.inf) StopPattern = SpeedPattern.inf;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {//1 2 5 8 20
                case 1:
                    if (ATS_Confirm) ATS_Confirm = false;
                    B1Pattern.Location = B1MonitorSectionLocation = state.Location + e.Distance;
                    break;
                case 2:
                    if (ATS_Confirm) ATS_Confirm = false;
                    B2Pattern.Location = state.Location + 210;
                    B2MonitorSectionLocation = state.Location + e.Distance;
                    break;
                case 5:
                    if (StopPattern == SpeedPattern.inf && ATSEnable)
                        StopPattern = new SpeedPattern(0, state.Location + 590);
                    break;
                case 8:
                    if (ATSEnable) {
                        if (e.Optional == 0) {
                            LimitPattern = SpeedPattern.inf;
                        } else if (e.Optional >= 2 && e.Optional <= 105) {
                            if (e.Optional < 20)
                                LimitPattern = new SpeedPattern(LimitPattern.AtLocation(state.Location, -4.0), state.Location);
                            else LimitPattern = new SpeedPattern(e.Optional, state.Location);
                        } else if (e.Optional == 1) {
                            LimitPattern = new SpeedPattern(20, state.Location + 380);//1.297
                        }
                    }
                    break;
                case 20:
                    lastMaxOver95 = MaxOver95;
                    MaxOver95 = e.Optional == 115;
                    break;
            }
        }

        public static void ConfirmEB(VehicleState state, HandleSet handles) {
            if (Math.Abs(state.Speed) == 0 && handles.BrakeNotch > 4) {
                if (EBType == EBTypes.CannotReleaseUntilStop) EBType = EBTypes.Normal;
                if (CanConfirm) {
                    if (NeedConfirm)
                        NeedConfirm = false;
                    ATS_Confirm = true;
                } else StopPattern.TargetSpeed = 20;
            }
        }

        public static void Disable() {
            ATS_Power = false;
            ATS_EB = false;
            ATS_Limit = false;
            ATS_Stop = false;
            ATS_Confirm = false;

            ATS_StopAnnounce = AtsSoundControlInstruction.Stop;
            ATS_EBAnnounce = AtsSoundControlInstruction.Stop;
        }
    }
}
