﻿using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class WS_ATC {
        private static bool NeedConfirm = false, Confirmed = false;
        private static TimeSpan InitializeStartTime = TimeSpan.Zero;

        public static int BrakeCommand = 0;
        public static bool ATCEnable = false;
        public static bool ATC_WSATC, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_Noset;

        private static int IndexToSpeed(int index) {
            if (index == 50) {
                return -1;
            } else if (index == 51) {
                return 25;
            } else if (index == 52) {
                return 40;
            } else if (index == 53) {
                return 65;
            } else if (index == 54) {
                return (int)Config.LessInf;
            } else if (index == 0) {
                return -2;
            } else return -2;
        }

        public static void Tick(VehicleState state, Section CurrentSection, bool Noset) {
            if (ATCEnable) {
                ATC_ServiceBrake = BrakeCommand > 0;
                ATC_EmergencyBrake = BrakeCommand == MetroSignal.vehicleSpec.BrakeNotches + 1;
                if (CurrentSection.CurrentSignalIndex != 0 && (CurrentSection.CurrentSignalIndex < 50 || CurrentSection.CurrentSignalIndex > 54)) {
                    if (Noset) {
                        ATC_Noset = true;
                        Disable_Noset();
                    } else {
                        ATC_Noset = false;
                        NeedConfirm = true;
                        BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                    }
                } else {
                    if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 3000) {
                        BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                    } else {
                        ATC_WSATC = true;
                        if (IndexToSpeed(CurrentSection.CurrentSignalIndex) == -2) {
                            if (!Confirmed) NeedConfirm = true;
                            else if (Math.Abs(state.Speed) > 15) NeedConfirm = true;
                        }else Confirmed = false;

                        if (Math.Abs(state.Speed) > IndexToSpeed(CurrentSection.CurrentSignalIndex) + 2.5 || IndexToSpeed(CurrentSection.CurrentSignalIndex) == -1)
                            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches;
                        else if (NeedConfirm) BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
                        else BrakeCommand = 0;
                    }
                }
            } else {
                DisableAll();
            }
        }
    }
}
