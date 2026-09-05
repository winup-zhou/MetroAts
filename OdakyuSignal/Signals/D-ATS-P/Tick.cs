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
        public static SoundPlayMode WarnBell, PatternApproach, EB_buzzer, SpeedCaution_buzzer;
        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;

        public static void Tick(VehicleState state, Section currentSection, Section NextSection, HandleSet handles) {
            if (ATSEnable) {
                PatternApproach = SoundPlayMode.Continue;
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

                    // 现实模型：信号現示由轨道电路连续传输（上方每帧依 NextSection.CurrentSignalIndex 设定目标速度），
                    // “到下一信号机的距离”则由 22 号 P 地上子在信号机 20m 手前读取并写入 SignalPattern.Location。
                    // 因此这里不再每帧覆盖 Location；仅当地上子数据失效（有效闭塞耗尽）时以 section 位置兜底，
                    // 避免照查终点失真导致失控。
                    if (ValidDataFromBeacon <= 0)
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
                                SpeedCaution_buzzer = SoundPlayMode.PlayLooping;
                                SignalPattern.TargetSpeed = 10;
                                SignalPattern.MaxSpeed = currentSection.CurrentSignalIndex == 1 ? 25 : 45;
                            } else {
                                if (ValidDataFromBeacon < 1) {
                                    SignalPattern.MaxSpeed = 10;
                                    SignalPattern.TargetSpeed = 10;
                                } else {
                                    ATS_SpeedCaution = false;
                                    SpeedCaution_buzzer = SoundPlayMode.Stop;
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
                    if (!lastPatternApproach && ATS_PatternApproach) PatternApproach = SoundPlayMode.Play;
                    lastPatternApproach = ATS_PatternApproach;

                    if (state.Time.TotalSeconds - LastEBResetTime.TotalSeconds > 60) {
                        EB_buzzer = SoundPlayMode.PlayLooping;
                        EB_NeedConfirm = true;
                        if (state.Time.TotalSeconds - LastEBResetTime.TotalSeconds > 65) NeedConfirm = true;
                    } else {
                        EB_buzzer = SoundPlayMode.Stop;
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
                        WarnBell = SoundPlayMode.PlayLooping;
                        BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches + 1;
                        ATS_Triggered = state.Time.TotalSeconds % 1000 < 500;
                    } else {
                        WarnBell = SoundPlayMode.Stop;
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
