using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    internal partial class D_ATS_P {
        private static SpeedPattern SignalPattern = SpeedPattern.inf, LimitPattern = SpeedPattern.inf;
        private static TimeSpan InitStartTime = TimeSpan.Zero, LastEBResetTime = TimeSpan.Zero;
        private static bool NeedConfirm = false, Confirmed = false, Noset = false, lastPatternApproach = false;
        private static double lastcurrentSectionLocation, MaxSpeed = 100;
        private static int ValidDataFromBeacon = 2;
        private static int lastBrakeNotch, lastPowerNotch;

        public static bool ATS_Power, ATS_PatternApproach, ATS_SpeedCaution, ATS_Triggered, ATS_Noset, ATS_NoSignal, ATS_EmergencyOperation, EB_NeedConfirm, ATS_Pbeacon;
        public static AtsSoundControlInstruction WarnBell, PatternApproach, EB_buzzer, SpeedCaution_buzzer;
        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;

        public static void Tick(VehicleState state, Section currentSection, Section NextSection, HandleSet handles) {
            if (ATSEnable) {
                PatternApproach = AtsSoundControlInstruction.Continue;
                ATS_Power = true;
                ATS_Noset = Noset;
                if (state.Time.TotalMilliseconds - InitStartTime.TotalMilliseconds < 250) {
                    BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches + 1;
                } else {
                    if (lastBrakeNotch != handles.BrakeNotch || lastPowerNotch != handles.PowerNotch || state.Speed == 0) LastEBResetTime = state.Time;
                    lastBrakeNotch = handles.BrakeNotch;
                    lastPowerNotch = handles.PowerNotch;

                    if (lastcurrentSectionLocation != currentSection.Location)
                        ValidDataFromBeacon--;
                    lastcurrentSectionLocation = currentSection.Location;

                    SignalPattern.Location = NextSection.Location - 25;

                    ATS_Pbeacon = ValidDataFromBeacon > 0;

                    if (currentSection.CurrentSignalIndex == 0 || currentSection.CurrentSignalIndex == 5) {
                        ATS_NoSignal = true;
                        if (Noset) LimitPattern = SignalPattern = SpeedPattern.inf;
                        else {
                            SignalPattern.MaxSpeed = -1;
                            SignalPattern.TargetSpeed = -1;
                        }
                    } else {
                        ATS_NoSignal = false;
                        if (Noset) {
                            LimitPattern = SignalPattern = SpeedPattern.inf;
                        } else {
                            if (NextSection.CurrentSignalIndex == 0) {
                                ATS_SpeedCaution = true;
                                SpeedCaution_buzzer = AtsSoundControlInstruction.PlayLooping;
                                SignalPattern.TargetSpeed = 10;
                                SignalPattern.MaxSpeed = currentSection.CurrentSignalIndex == 1 ? 25 : 45;
                            } else {
                                if (ValidDataFromBeacon < 1) {
                                    SignalPattern.MaxSpeed = 10;
                                    SignalPattern.TargetSpeed = 10;
                                } else {
                                    ATS_SpeedCaution = false;
                                    SpeedCaution_buzzer = AtsSoundControlInstruction.Stop;
                                    if (NextSection.CurrentSignalIndex == 1) {
                                        SignalPattern.TargetSpeed = 25;
                                        SignalPattern.MaxSpeed = 45;
                                    } else if (NextSection.CurrentSignalIndex == 2) {
                                        SignalPattern.TargetSpeed = 45;
                                        SignalPattern.MaxSpeed = 75;
                                    } else if (NextSection.CurrentSignalIndex == 3) {
                                        SignalPattern.TargetSpeed = 75;
                                        SignalPattern.MaxSpeed = MaxSpeed;
                                    } else if (NextSection.CurrentSignalIndex == 4) {
                                        SignalPattern.MaxSpeed = SignalPattern.TargetSpeed = MaxSpeed;
                                    }
                                }
                            }
                        }
                    }
                    var monitorSpeed = Math.Min(SignalPattern.AtLocation(state.Location, -3.3), LimitPattern.AtLocation(state.Location, -3.3));
                    ATS_PatternApproach = Math.Abs(state.Speed) - monitorSpeed < 5;
                    if (!lastPatternApproach && ATS_PatternApproach) PatternApproach = AtsSoundControlInstruction.Play;
                    lastPatternApproach = ATS_PatternApproach;

                    if (state.Time.TotalSeconds - LastEBResetTime.TotalSeconds > 60) {
                        EB_buzzer = AtsSoundControlInstruction.PlayLooping;
                        EB_NeedConfirm = true;
                        if (state.Time.TotalSeconds - LastEBResetTime.TotalSeconds > 65) NeedConfirm = true;
                    } else {
                        EB_buzzer = AtsSoundControlInstruction.Stop;
                        EB_NeedConfirm = false;
                    }

                    if (Math.Abs(state.Speed) > monitorSpeed) {
                        if (SignalPattern.AtLocation(state.Location, -3.3) < 25 && Math.Abs(state.Speed) > SignalPattern.AtLocation(state.Location, -3.3)) {
                            NeedConfirm = true;
                        } else {
                            BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches;
                            ATS_Triggered = true;
                        }
                    } else if (NeedConfirm) {
                        WarnBell = AtsSoundControlInstruction.PlayLooping;
                        BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches + 1;
                        ATS_Triggered = state.Time.TotalSeconds % 1000 < 500;
                    } else {
                        WarnBell = AtsSoundControlInstruction.Stop;
                        ATS_Triggered = false;
                        BrakeCommand = 0;
                    }


                }
            } else {
                Disable();
            }
        }
    }
}
