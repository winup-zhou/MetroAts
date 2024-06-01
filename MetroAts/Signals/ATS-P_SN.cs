using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroAts {
    internal class ATS_P_SN {
        const double SignalDec = -2.445;

        private static double P_OverrideStartTime = -60000;
        private static double P_MaxSpeed = Config.LessInf;
        public static int BrakeCommand = 0;
        private static double InitStartTime = 0;
        public static bool ATSEnable = false, BrakeUntilStop = false, BrakeCanRelease = false;
        private static bool EBBeacon = false;
        private static bool SN_Enable = false, P_Enable = false;
        private static SpeedLimit P_SignalPattern = new SpeedLimit(),
            P_StationStopPattern = new SpeedLimit(),
            P_SpeedLimit1 = new SpeedLimit(),//分岐器速度制限
            P_SpeedLimit2 = new SpeedLimit(),//下り勾配速度制限
            P_SpeedLimit3 = new SpeedLimit(),//曲線速度制限
            P_SpeedLimit4 = new SpeedLimit(),//臨時速度制限
            P_SpeedLimit5 = new SpeedLimit();//誘導信号機速度制限
            

        //panel
        public static bool P_Power, P_PatternApproach, P_BrakeActioned, P_EBActioned, P_BrakeOverride, P_PEnable, P_Fail,
            SN_Power, SN_Action;

        private static IAtsSound P_Ding, SN_WarningBell, SN_Chime;


        public static void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            BrakeUntilStop = false;
            BrakeCanRelease = false;
            BrakeCommand = 0;
            EBBeacon = false;
            P_OverrideStartTime = -60000;
            P_MaxSpeed = Config.LessInf;
            InitStartTime = 0;
            ATSEnable = false;
            SN_Enable = false;
            P_Enable = false;
            P_SignalPattern = new SpeedLimit();
            P_StationStopPattern = new SpeedLimit();
            P_SpeedLimit1 = new SpeedLimit();//分岐器速度制限
            P_SpeedLimit2 = new SpeedLimit();//下り勾配速度制限
            P_SpeedLimit3 = new SpeedLimit();//曲線速度制限
            P_SpeedLimit4 = new SpeedLimit();//臨時速度制限
            P_SpeedLimit5 = new SpeedLimit();//誘導信号機速度制限
            

            P_Ding = MetroAts.ATC_Ding;
            SN_WarningBell = MetroAts.ATC_WarningBell;
            SN_Chime = MetroAts.ATS_Chime;
        }

        public static void Enable(double Time) {
            InitStartTime = Time;
            ATSEnable = true;
            P_Ding.Play();
        }

        public static void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {

        }

        public static void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;

                case 3:
                    if (ATSEnable)
                        if (!P_Enable) { P_Enable = true; P_Ding.Play(); }
                    P_SignalPattern = new SpeedLimit(-1, e.Distance + MetroAts.state.Location);
                    EBBeacon = false;
                    break;
                case 4:
                    if (ATSEnable)
                        if (!P_Enable) { P_Enable = true; P_Ding.Play(); }
                    P_SignalPattern = new SpeedLimit(-1, e.Distance + MetroAts.state.Location);
                    if (e.Distance < 50) {
                        BrakeUntilStop = true;
                        EBBeacon = true;
                    }
                    break;
                case 5:
                    if (ATSEnable)
                        if (!P_Enable) { P_Enable = true; P_Ding.Play(); }
                    P_SignalPattern = new SpeedLimit(-1, e.Distance + MetroAts.state.Location);
                    if (e.Distance < 50) {
                        BrakeUntilStop = true;
                        EBBeacon = true;
                    }
                    break;
                case 6:
                    if (ATSEnable) P_SpeedLimit1 = new SpeedLimit(e.Optional / 1000, e.Optional % 1000 + MetroAts.state.Location);
                    break;
                case 7:
                    if (ATSEnable) P_MaxSpeed = e.Optional;
                    break;
                case 8:
                    if (ATSEnable) P_SpeedLimit2 = new SpeedLimit(e.Optional / 1000, e.Optional % 1000 + MetroAts.state.Location);
                    break;
                case 9:
                    if (ATSEnable)
                        if (!P_Enable) {
                            if (MetroAts.state.Speed > e.Optional) { }
                        } else {
                            P_SpeedLimit3 = new SpeedLimit(e.Optional / 1000, e.Optional % 1000 + MetroAts.state.Location);
                        }
                    break;
                case 10:
                    if (ATSEnable) P_SpeedLimit4 = new SpeedLimit(e.Optional / 1000, e.Optional % 1000 + MetroAts.state.Location);
                    break;
                case 11:
                    if (ATSEnable) P_SpeedLimit5 = new SpeedLimit(e.Optional / 1000, e.Optional % 1000 + MetroAts.state.Location);
                    break;
                case 16:
                    if (ATSEnable) P_SpeedLimit1 = SpeedLimit.inf;
                    break;
                case 18:
                    if (ATSEnable) P_SpeedLimit2 = SpeedLimit.inf;
                    break;
                case 19:
                    if (ATSEnable) P_SpeedLimit3 = SpeedLimit.inf;
                    break;
                case 20:
                    if (ATSEnable) P_SpeedLimit4 = SpeedLimit.inf;
                    break;
                case 21:
                    if (ATSEnable) P_SpeedLimit5 = SpeedLimit.inf;
                    break;
                case 25:
                    if (ATSEnable)
                        if (e.Optional == 0) {
                            if (P_Enable) {
                                P_Ding.Play();
                                P_Enable = false;
                            }
                            SN_Enable = true;
                        } else {
                            SN_Enable = false;
                        }
                    break;
            }
        }

        public static void OnB1Pressed(object sender, EventArgs e) {
            if (EBBeacon) {
                if (MetroAts.handles.Brake.Notch == MetroAts.vehicleSpec.BrakeNotches + 1) {
                    EBBeacon = false;
                    BrakeUntilStop = false;
                    P_SignalPattern.Limit = 15;
                }
            } else if (MetroAts.handles.Brake.Notch == MetroAts.vehicleSpec.BrakeNotches) {
                BrakeUntilStop = false;
            }
        }

        public static void OnB2Pressed(object sender, EventArgs e) {
            if (ATSEnable)
                if (!P_BrakeOverride) {
                    P_OverrideStartTime = MetroAts.state.Time.TotalMilliseconds;
                    P_BrakeOverride = true;
                    P_Ding.Play();
                } else {
                    P_BrakeOverride = false;
                    P_OverrideStartTime = -60000;
                    P_Ding.Play();
                }
        }

        public static void Tick(double Location, double Speed, double Time) {
            if (ATSEnable) {
                P_Power = true;
                if (Time - InitStartTime < 3000) {
                    P_Fail = true;
                    P_PEnable = false;
                    BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;
                } else {
                    BrakeCommand = 0;
                    P_Fail = false;
                    P_PEnable = P_Enable;
                    if (P_Enable) {
                        var lastPatternApproach = P_PatternApproach;
                        var lastPBrakeActioned = P_BrakeActioned;
                        var lastPEBActioned = P_EBActioned;

                        P_PatternApproach = CalculatePattern1(Location + 50) - 5 < Speed ||
                            CalculatePattern2(Location + 50) - 5 < Speed
                            || P_MaxSpeed - 5 < Speed || P_EBActioned;

                        if (Speed > CalculatePattern1(Location)) {
                            BrakeUntilStop = true;
                        }

                        if (Speed > CalculatePattern2(Location)) {
                            BrakeCanRelease = true;
                        }

                        if (Time - P_OverrideStartTime > 60000) {
                            if (BrakeUntilStop) {
                                P_BrakeActioned = true;
                                if (EBBeacon) {
                                    P_EBActioned = true;
                                    BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;
                                } else {
                                    BrakeCommand = MetroAts.vehicleSpec.BrakeNotches;
                                }
                            } else {
                                BrakeCommand = 0;
                                P_EBActioned = P_BrakeActioned = false;
                                if (BrakeCanRelease) {
                                    BrakeCommand = MetroAts.vehicleSpec.BrakeNotches;
                                    P_BrakeActioned = true;
                                    var ReleaseSpeed = Math.Min(P_SpeedLimit1.Limit, Math.Min(P_SpeedLimit2.Limit, Math.Min(P_SpeedLimit3.Limit, P_SpeedLimit4.Limit)));
                                    if (Speed < ReleaseSpeed - 5) BrakeCanRelease = false;
                                }
                            }
                        } else {
                            BrakeCommand = 0;
                            P_EBActioned = P_BrakeActioned = false;
                        }


                        if (lastPatternApproach != P_PatternApproach || lastPBrakeActioned != P_BrakeActioned || lastPEBActioned != P_EBActioned) P_Ding.Play();
                    }
                }
            } else {
                BrakeCanRelease = false;
                BrakeUntilStop = false;
                BrakeCommand = 0;
                EBBeacon = false;
                P_OverrideStartTime = -60000;
                P_MaxSpeed = Config.LessInf;
                InitStartTime = 0;
                ATSEnable = false;
                SN_Enable = false;
                P_Enable = false;
                P_SignalPattern = new SpeedLimit();
                P_StationStopPattern = new SpeedLimit();
                P_SpeedLimit1 = new SpeedLimit();//分岐器速度制限
                P_SpeedLimit2 = new SpeedLimit();//下り勾配速度制限
                P_SpeedLimit3 = new SpeedLimit();//曲線速度制限
                P_SpeedLimit4 = new SpeedLimit();//臨時速度制限
                P_SpeedLimit5 = new SpeedLimit();//誘導信号機速度制限
            }

        }

        public static void Disable() {
            BrakeCanRelease = false;
            BrakeUntilStop = false;
            BrakeCommand = 0;
            EBBeacon = false;
            P_OverrideStartTime = -60000;
            P_MaxSpeed = Config.LessInf;
            InitStartTime = 0;
            ATSEnable = false;
            SN_Enable = false;
            P_Enable = false;
            P_SignalPattern = new SpeedLimit();
            P_StationStopPattern = new SpeedLimit();
            P_SpeedLimit1 = new SpeedLimit();//分岐器速度制限
            P_SpeedLimit2 = new SpeedLimit();//下り勾配速度制限
            P_SpeedLimit3 = new SpeedLimit();//曲線速度制限
            P_SpeedLimit4 = new SpeedLimit();//臨時速度制限
            P_SpeedLimit5 = new SpeedLimit();//誘導信号機速度制限
            

            P_Ding.Stop();
            SN_WarningBell.Stop();
            SN_Chime.Stop();
        }

        private static double CalculatePattern1(double Location) {
            var ResultSpeed = Config.LessInf;
            ResultSpeed = Math.Min(ResultSpeed, P_SignalPattern.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_StationStopPattern.AtLocation(Location, SignalDec));
            return ResultSpeed;
        }

        private static double CalculatePattern2(double Location) { //速度制限
            var ResultSpeed = Config.LessInf;
            ResultSpeed = Math.Min(ResultSpeed, P_MaxSpeed);
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit1.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit2.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit3.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit4.AtLocation(Location, SignalDec));
            ResultSpeed = Math.Min(ResultSpeed, P_SpeedLimit5.AtLocation(Location, SignalDec));
            return ResultSpeed;
        }
    }
}
