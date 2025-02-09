using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeibuSignal {
    internal partial class SeibuATS {
        private static SpeedPattern B1Pattern = SpeedPattern.inf, B2Pattern = SpeedPattern.inf, StopPattern = SpeedPattern.inf, LimitPattern = SpeedPattern.inf;
        private static bool NeedConfirm = false, MaxOver95 = false, lastMaxOver95 = false;
        private static double B1MonitorSectionLocation = 0, B2MonitorSectionLocation = 0, B1Speed = 0, B2Speed = 0;
        private static TimeSpan InitializeStartTime = TimeSpan.Zero;
        private enum EBTypes {
            Normal = 0,
            CannotReleaseUntilStop,
            CanReleaseWithoutstop
        }

        private static EBTypes EBType = EBTypes.Normal;
        //4 -> G2,3 -> G1/YG,2 -> Y,1 -> YY,0 -> R

        public static bool ATS_Power, ATS_EB, ATS_Limit, ATS_Stop, ATS_Confirm;
        public static AtsSoundControlInstruction ATS_StopAnnounce, ATS_EBAnnounce;
        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;

        public static void Tick(VehicleState state, SectionManager sectionManager) {
            if (ATSEnable) {
                ATS_StopAnnounce = AtsSoundControlInstruction.Continue;

                int pointer1 = 0, pointer2 = 0, pointer3 = 0;
                while (sectionManager.Sections[pointer1].Location < B1MonitorSectionLocation) {
                    pointer1++;
                    if (pointer1 >= sectionManager.Sections.Count) {
                        pointer1 = sectionManager.Sections.Count - 1;
                        break;
                    }
                }


                while (sectionManager.Sections[pointer2].Location < B2MonitorSectionLocation) {
                    pointer2++;
                    if (pointer2 >= sectionManager.Sections.Count) {
                        pointer2 = sectionManager.Sections.Count - 1;
                        break;
                    }
                }

                while (sectionManager.Sections[pointer3].Location < state.Location) {
                    pointer3++;
                    if (pointer3 >= sectionManager.Sections.Count) {
                        pointer3 = sectionManager.Sections.Count - 1;
                        break;
                    }
                }

                var B1MonitorSection = sectionManager.Sections[pointer1] as Section;
                var B2MonitorSection = sectionManager.Sections[pointer2] as Section;
                var CurrentSection = sectionManager.Sections[pointer3 > 0 ? pointer3 - 1 : 0] as Section;
                var PreviousSection = sectionManager.Sections[pointer3 > 1 ? pointer3 - 2 : 0] as Section;

                if (state.Time.TotalMilliseconds - InitializeStartTime.TotalMilliseconds < 2000) {
                    if (CurrentSection.CurrentSignalIndex > 4) InitializeStartTime = state.Time;
                    B1MonitorSectionLocation = B2MonitorSectionLocation = sectionManager.Sections[pointer3].Location;
                    EBType = EBTypes.CanReleaseWithoutstop;
                    ATS_EBAnnounce = AtsSoundControlInstruction.PlayLooping;
                    ATS_EB = true;
                    ATS_Power = false;
                    BrakeCommand = SeibuSignal.vehicleSpec.BrakeNotches + 1;
                } else {
                    ATS_Power = true;
                    //if (state.Location > B2MonitorSectionLocation) B2MonitorSectionLocation = sectionManager.LasSection.Location;
                    if (CurrentSection.CurrentSignalIndex >= 9 && CurrentSection.CurrentSignalIndex < 49 && CurrentSection.CurrentSignalIndex != 34) {
                        B1Speed = B2Speed = 30;
                    } else {
                        if (B1MonitorSection.CurrentSignalIndex == 0) { //fR
                            B1Pattern.Location = B1MonitorSection.Location - 10;
                            B1Pattern.TargetSpeed = 0;
                            B1Pattern.MaxSpeed = CurrentSection.CurrentSignalIndex == 1 ? 30 : 65;
                            B1Speed = B1Pattern.AtLocation(state.Location, -2.93);
                        } else if (B1MonitorSection.CurrentSignalIndex == 1 ||
                        (B1MonitorSection.CurrentSignalIndex >= 9 && B1MonitorSection.CurrentSignalIndex < 49 && B1MonitorSection.CurrentSignalIndex != 34)) {//fYY
                            if (ATS_Confirm && state.Location < B1MonitorSectionLocation) ATS_Confirm = false;
                            B1Pattern.TargetSpeed = 30;
                            B1Pattern.MaxSpeed = 65;
                            B1Speed = B1Pattern.AtLocation(state.Location, -2.93);
                        } else if (B1MonitorSection.CurrentSignalIndex == 2) {//fY
                            if (ATS_Confirm && state.Location < B1MonitorSectionLocation) ATS_Confirm = false;
                            B1Pattern.TargetSpeed = 65;
                            B1Pattern.MaxSpeed = 95;
                            B1Speed = B1Pattern.AtLocation(state.Location, -3.33);
                        } else if (B1MonitorSection.CurrentSignalIndex == 3) {//fGY
                            if (ATS_Confirm && state.Location < B1MonitorSectionLocation) ATS_Confirm = false;
                            B1Pattern.TargetSpeed = 95;
                            B1Pattern.MaxSpeed = MaxOver95 ? 115 : 95;
                            B1Speed = B1Pattern.AtLocation(state.Location, -2.93);
                        } else if (B1MonitorSection.CurrentSignalIndex == 4) {//fG
                            if (ATS_Confirm && state.Location < B1MonitorSectionLocation) ATS_Confirm = false;
                            if (MaxOver95) {
                                B1Pattern.TargetSpeed = 115;
                                B1Pattern.MaxSpeed = 115;
                            } else {
                                B1Pattern.TargetSpeed = 95;
                                B1Pattern.MaxSpeed = lastMaxOver95 ? 115 : 95;
                            }
                            B1Speed = B1Pattern.AtLocation(state.Location, -2.93);
                        }

                        if (B2MonitorSection.CurrentSignalIndex == 0 || B2MonitorSection.CurrentSignalIndex == 1 ||
                            (B2MonitorSection.CurrentSignalIndex >= 9 && B2MonitorSection.CurrentSignalIndex < 49 && B2MonitorSection.CurrentSignalIndex != 34)) {//fY -> fYY/fR 
                            if (B2MonitorSection.CurrentSignalIndex > 0 && ATS_Confirm && state.Location < B2MonitorSectionLocation) ATS_Confirm = false;
                            B2Pattern.TargetSpeed = 65;
                            B2Pattern.MaxSpeed = 95;
                            B2Speed = B2Pattern.AtLocation(state.Location, -3.33);
                        } else if (B2MonitorSection.CurrentSignalIndex == 2) {//fGY -> fY
                            if (ATS_Confirm && state.Location < B2MonitorSectionLocation) ATS_Confirm = false;
                            var lastTargetSpeed = B2Pattern.TargetSpeed;
                            B2Pattern.TargetSpeed = 95;
                            B2Pattern.MaxSpeed = MaxOver95 ? 115 : 95;
                            B2Speed = B2Pattern.AtLocation(state.Location, -2.93);
                        } else if (B2MonitorSection.CurrentSignalIndex == 3 || B2MonitorSection.CurrentSignalIndex == 4) {
                            if (ATS_Confirm && state.Location < B2MonitorSectionLocation) ATS_Confirm = false;
                            if (!lastMaxOver95 && MaxOver95) {
                                B2Pattern.TargetSpeed = 115;
                                B2Pattern.MaxSpeed = 115;
                            } else if (lastMaxOver95 && !MaxOver95) {
                                B2Pattern.TargetSpeed = 95;
                                B2Pattern.MaxSpeed = lastMaxOver95 ? 115 : 95;
                            }
                            B2Speed = B2Pattern.AtLocation(state.Location, -2.93);
                        }

                        if (lastMaxOver95 && state.Location >= B1MonitorSectionLocation && state.Location >= B2MonitorSectionLocation)
                            lastMaxOver95 = false;
                        if (ATS_Confirm) B1Speed = B2Speed = 30;
                    }

                    ATS_Limit = LimitPattern != SpeedPattern.inf;

                    var lastATS_Stop = ATS_Stop;
                    ATS_Stop = StopPattern != SpeedPattern.inf;
                    if (!lastATS_Stop && ATS_Stop) ATS_StopAnnounce = AtsSoundControlInstruction.Play;

                    var PatternSpeed = Math.Min(Math.Min(B1Speed, B2Speed),
                        Math.Min(StopPattern.AtLocation(state.Location, -4.0), LimitPattern.AtLocation(state.Location, -4.0)));
                    if (Math.Abs(state.Speed) > PatternSpeed) {
                        if (StopPattern.AtLocation(state.Location, -4.0) < LimitPattern.AtLocation(state.Location, -4.6)
                            && StopPattern.AtLocation(state.Location, -4.0) < Math.Min(B1Speed, B2Speed)) {
                            EBType = EBTypes.CannotReleaseUntilStop;
                        } else if (LimitPattern.AtLocation(state.Location, -4.0) < StopPattern.AtLocation(state.Location, -4.0)
                            && LimitPattern.AtLocation(state.Location, -4.0) < Math.Min(B1Speed, B2Speed)) {
                            EBType = EBTypes.CanReleaseWithoutstop;
                        } else {
                            if (B1Pattern.TargetSpeed == 0) {
                                EBType = EBTypes.CannotReleaseUntilStop;
                                NeedConfirm = true;
                            } else EBType = EBTypes.CanReleaseWithoutstop;
                        }
                    } else {
                        if (EBType != EBTypes.CannotReleaseUntilStop) EBType = EBTypes.Normal;
                    }

                    BrakeCommand = EBType != EBTypes.Normal ? SeibuSignal.vehicleSpec.BrakeNotches + 1 : 0;
                    ATS_EB = EBType != EBTypes.Normal;
                    ATS_EBAnnounce = EBType != EBTypes.Normal ? AtsSoundControlInstruction.PlayLooping : AtsSoundControlInstruction.Stop;
                }
            } else {
                Disable();
            }
        }
    }
}
