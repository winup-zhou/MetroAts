//using BveEx.PluginHost;
//using BveEx.PluginHost.Panels.Native;
//using BveEx.PluginHost.Sound.Native;
//using BveTypes.ClassWrappers;
//using System;

//namespace MetroAts {
//    internal class ATC {
//        public static INative Native;

//        //InternalValue -> ATC
//        public static int[] ATCLimits = { -2, -2, -2, -2, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
//            -2, -2, -2, -1, -2, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };

//        private static SpeedPattern ORPPattern = new SpeedPattern(), StationPattern = new SpeedPattern();
//        public static bool ATCEnable = false, inDepot = false;
//        private static bool StationStop = false, EBUntilStop = false, ServiceBrake = false, SignalAnn = false;
//        private const double ORPPatternDec = -2.3; //*10
//        private const double StationPatternDec = -4.0;
//        private static double InitializeStartTime = 0, BrakeStartTime = Config.LessInf, LastATCSpeed = 0;

//        public static int BrakeCommand = 0, ATCSpeed = 0, ATCType = 0;

//        //panel -> ATC
//        public static bool ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45,
//            ATC_50, ATC_55, ATC_60, ATC_65, ATC_70, ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110, ATC_Stop, ATC_Proceed,
//            ATC_P, ATC_SeibuATC, ATC_MetroATC, ATC_TokyuATC, ATC_SeibuDepot, ATC_MetroDepot, ATC_TokyuDepot,
//            ATC_SeibuServiceBrake, ATC_MetroAndTokyuServiceBrake, ATC_SeibuEmergencyBrake,
//            ATC_MetroAndTokyuEmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach, ATC_TokyuStationStop,
//            ATC_SeibuStationStop, ATC_SignalAnn, ATC_SeibuNoset, ATC_MetroNoset, ATC_TokyuNoset, ATC_TempLimit;
//        public static int ORPNeedle, ATCNeedle, ATCNeedle_Disappear;
//        private static IAtsSound ATC_Ding, ATC_SignalAnnBeep, ATC_ORPBeep, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

//        public static void Initialize(BveEx.PluginHost.Native.StartedEventArgs e) {
//            ORPPattern = new SpeedPattern();
//            StationPattern = new SpeedPattern();
//            StationStop = false;
//            BrakeCommand = 0;
//            ATCSpeed = 0;
//            ATCEnable = false;
//            EBUntilStop = false;
//            ServiceBrake = false;
//            BrakeStartTime = Config.LessInf;
//            LastATCSpeed = 0;
//            SignalAnn = false;
//            ATCType = 0;
//            inDepot = false;

//            ATC_Ding = MetroAts.ATC_Ding;
//            ATC_SignalAnnBeep = MetroAts.ATC_SignalAnnBeep;
//            ATC_ORPBeep = MetroAts.ATC_ORPBeep;
//            ATC_EmergencyOperationAnnounce = MetroAts.ATC_EmergencyOperationAnnounce;
//            ATC_WarningBell = MetroAts.ATC_WarningBell;

//            ATC_01 = false;
//            ATC_10 = false;
//            ATC_15 = false;
//            ATC_20 = false;
//            ATC_25 = false;
//            ATC_30 = false;
//            ATC_35 = false;
//            ATC_40 = false;
//            ATC_45 = false;
//            ATC_50 = false;
//            ATC_55 = false;
//            ATC_60 = false;
//            ATC_65 = false;
//            ATC_70 = false;
//            ATC_75 = false;
//            ATC_80 = false;
//            ATC_85 = false;
//            ATC_90 = false;
//            ATC_95 = false;
//            ATC_100 = false;
//            ATC_110 = false;

//            ATC_Stop = false;
//            ATC_Proceed = false;

//            ATC_P = false;
//            ATC_X = false;

//            ORPNeedle = 0;
//            ATCNeedle = 0;
//            ATCNeedle_Disappear = 1;

//            ATC_SeibuATC = false;
//            ATC_MetroATC = false;
//            ATC_TokyuATC = false;

//            ATC_SignalAnn = false;
//            ATC_SeibuNoset = false;
//            ATC_TokyuNoset = false;
//            ATC_MetroNoset = false;
//            ATC_TempLimit = false;

//            ATC_TokyuDepot = false;
//            ATC_SeibuDepot = false;
//            ATC_MetroDepot = false;
//            ATC_SeibuServiceBrake = false;
//            ATC_MetroAndTokyuServiceBrake = false;
//            ATC_SeibuEmergencyBrake = false;
//            ATC_MetroAndTokyuEmergencyBrake = false;
//            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
//            ATC_TokyuStationStop = false;
//            ATC_SeibuStationStop = false;
//        }

//        public static void Enable(double Time) {
//            ATCEnable = true;
//            InitializeStartTime = Time;
//        }

//        public static void BeaconPassed(BveEx.PluginHost.Native.BeaconPassedEventArgs e) {
//            switch (e.Type) {
//                case 12:
//                    if (ORPPattern != SpeedPattern.inf) ORPPattern.Location = MetroAts.state.Location + e.Optional;
//                    break;
//                case 32:
//                    StationStop = true;
//                    StationPattern = new SpeedLimit(0, MetroAts.state.Location + 510);
//                    break;
//            }
//        }

//        public static void DoorOpened(BveEx.PluginHost.Native.DoorEventArgs e) {
//            StationStop = false;
//        }

//        public static void Tick(double Location, double Speed, double Time, Section CurrentSection, Section NextSection, bool handleOnEB, int AtcType, bool Noset) {
//            ATCType = AtcType;
//            if (ATCEnable) {
//                if (ATCType == 2) {
//                    ATC_SeibuServiceBrake = BrakeCommand > 0;
//                    ATC_SeibuEmergencyBrake = BrakeCommand == MetroAts.vehicleSpec.BrakeNotches + 1;
//                    ATC_MetroAndTokyuServiceBrake = false;
//                    ATC_MetroAndTokyuEmergencyBrake = false;
//                } else {
//                    ATC_MetroAndTokyuServiceBrake = BrakeCommand > 0;
//                    ATC_MetroAndTokyuEmergencyBrake = BrakeCommand == MetroAts.vehicleSpec.BrakeNotches + 1;
//                    ATC_SeibuServiceBrake = false;
//                    ATC_SeibuEmergencyBrake = false;
//                }

//                if (CurrentSection.CurrentSignalIndex < 9 || CurrentSection.CurrentSignalIndex == 34 || CurrentSection.CurrentSignalIndex >= 49) {
//                    if (!Noset) {
//                        if (ATCType == -1) {
//                            ATC_TokyuNoset = true;
//                            ATC_SeibuNoset = ATC_MetroNoset = false;
//                        } else if (ATCType == 2) {
//                            ATC_SeibuNoset = true;
//                            ATC_TokyuNoset = ATC_MetroNoset = false;
//                        } else if (ATCType == 3) {
//                            ATC_MetroNoset = true;
//                            ATC_SeibuNoset = ATC_TokyuNoset = false;
//                        }
//                        ATC_X = true;
//                        ATC_Stop = ATC_Proceed = ATC_P = false;
//                        if (!Config.ATCLimitUseNeedle) {
//                            ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
//                            = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
//                            = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
//                            = ATC_95 = ATC_100 = ATC_110 = false;
//                        } else {
//                            ATCNeedle = 0;
//                            ATCNeedle_Disappear = 1;
//                        }
//                        ATC_WarningBell.PlayLoop();

//                        BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;
//                    } else {
//                        BrakeCommand = 0;

//                        ATC_Ding.Stop();
//                        ATC_SignalAnnBeep.Stop();
//                        ATC_ORPBeep.Stop();
//                        ATC_EmergencyOperationAnnounce.Stop();
//                        ATC_WarningBell.Stop();

//                        ATC_01 = false;
//                        ATC_10 = false;
//                        ATC_15 = false;
//                        ATC_20 = false;
//                        ATC_25 = false;
//                        ATC_30 = false;
//                        ATC_35 = false;
//                        ATC_40 = false;
//                        ATC_45 = false;
//                        ATC_50 = false;
//                        ATC_55 = false;
//                        ATC_60 = false;
//                        ATC_65 = false;
//                        ATC_70 = false;
//                        ATC_75 = false;
//                        ATC_80 = false;
//                        ATC_85 = false;
//                        ATC_90 = false;
//                        ATC_95 = false;
//                        ATC_100 = false;
//                        ATC_110 = false;

//                        ATC_Stop = false;
//                        ATC_Proceed = false;

//                        ATC_P = false;
//                        ATC_X = false;

//                        ORPNeedle = 0;
//                        ATCNeedle = 0;
//                        ATCNeedle_Disappear = 1;

//                        ATC_SeibuATC = false;
//                        ATC_MetroATC = false;
//                        ATC_TokyuATC = false;

//                        ATC_SignalAnn = false;
//                        ATC_TempLimit = false;

//                        ATC_TokyuDepot = false;
//                        ATC_SeibuDepot = false;
//                        ATC_MetroDepot = false;
//                        ATC_SeibuServiceBrake = false;
//                        ATC_MetroAndTokyuServiceBrake = false;
//                        ATC_SeibuEmergencyBrake = false;
//                        ATC_MetroAndTokyuEmergencyBrake = false;
//                        //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
//                        ATC_TokyuStationStop = false;
//                        ATC_SeibuStationStop = false;
//                    }
//                } else {
//                    if (Time - InitializeStartTime < 3000) {
//                        ATC_X = true;
//                        ATC_Stop = ATC_Proceed = ATC_P = false;
//                        if (!Config.ATCLimitUseNeedle) {
//                            ATC_01 = ATC_10 = ATC_15 = ATC_20 = ATC_25 = ATC_30
//                            = ATC_35 = ATC_40 = ATC_45 = ATC_50 = ATC_55 = ATC_60
//                            = ATC_65 = ATC_70 = ATC_75 = ATC_80 = ATC_85 = ATC_90
//                            = ATC_95 = ATC_100 = ATC_110 = false;
//                        } else {
//                            ATCNeedle = 0;
//                            ATCNeedle_Disappear = 1;
//                        }

//                        if (ATCType == -1) ATC_WarningBell.PlayLoop();

//                        BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;
//                    } else {
//                        if (ATCType == -1) {
//                            ATC_TokyuATC = true;
//                            ATC_SeibuATC = ATC_MetroATC = false;
//                        } else if (ATCType == 2) {
//                            ATC_SeibuATC = true;
//                            ATC_TokyuATC = ATC_MetroATC = false;
//                        } else if (ATCType == 3) {
//                            ATC_MetroATC = true;
//                            ATC_SeibuATC = ATC_TokyuATC = false;
//                        }

//                        BrakeCommand = 0;

//                        var lastinDepot = inDepot;
//                        inDepot = CurrentSection.CurrentSignalIndex >= 38 && CurrentSection.CurrentSignalIndex <= 48;
//                        if (lastinDepot != inDepot) ATC_Ding.Play();

//                        if (Noset) {
//                            if (ATCType == -1) {
//                                ATC_TokyuNoset = true;
//                                ATC_SeibuNoset = ATC_MetroNoset = false;
//                            } else if (ATCType == 2) {
//                                ATC_SeibuNoset = true;
//                                ATC_TokyuNoset = ATC_MetroNoset = false;
//                            } else if (ATCType == 3) {
//                                ATC_MetroNoset = true;
//                                ATC_SeibuNoset = ATC_TokyuNoset = false;
//                            }
//                            ATC_WarningBell.PlayLoop();
//                        } else {
//                            ATC_TokyuNoset = ATC_SeibuNoset = ATC_MetroNoset = false;
//                            ATC_WarningBell.PlayLoop();
//                        }

//                        if (ATC_WarningBell.PlayState == BveEx.PluginHost.Sound.PlayState.PlayingLoop && !Noset)
//                            ATC_WarningBell.Stop();

//                        if (ATC_X) { ATC_X = false; ATC_Ding.Play(); }

//                        var lastATCSpeed = ATCSpeed;

//                        var ORPSpeed = 0.0;
//                        if (CurrentSection.CurrentSignalIndex == 35 || CurrentSection.CurrentSignalIndex == 38) {
//                            if (ATCType == 2) ATCSpeed = 0;
//                            else {
//                                if (ORPPattern == SpeedPattern.inf) {
//                                    ORPPattern = new SpeedPattern(0, NextSection.Location);
//                                    LastATCSpeed = ATCSpeed;
//                                }
//                                ORPSpeed = Math.Min(ORPPattern.AtLocation(Location, ORPPatternDec), LastATCSpeed);
//                                if (ATCType == -1) {
//                                    if (ORPSpeed - Speed < 5 || ORPSpeed == 7) ATC_ORPBeep.PlayLoop();
//                                    else ATC_ORPBeep.Stop();
//                                }
//                            }
//                        } else {
//                            ORPPattern = SpeedPattern.inf;
//                            if (ATC_ORPBeep.PlayState == BveEx.PluginHost.Sound.PlayState.PlayingLoop) ATC_ORPBeep.Stop();
//                        }

//                        ATCSpeed = ATCLimits[CurrentSection.CurrentSignalIndex] < 0 ? -1 : ATCLimits[CurrentSection.CurrentSignalIndex];

//                        if (ATCLimits[CurrentSection.CurrentSignalIndex] == 0 && ATCSpeed != -1) {
//                            BrakeCommand = (int)Math.Ceiling(MetroAts.vehicleSpec.BrakeNotches * 0.5);
//                            ATCSpeed = 0;
//                        }

//                        if (lastATCSpeed != ATCSpeed && !inDepot) ATC_Ding.Play();

//                        if (inDepot) {
//                            if (ATCType == -1) {
//                                ATC_TokyuDepot = true;
//                                ATC_SeibuDepot = ATC_MetroDepot = false;
//                            } else if (ATCType == 2) {
//                                ATC_SeibuDepot = true;
//                                ATC_TokyuDepot = ATC_MetroDepot = false;
//                            } else if (ATCType == 3) {
//                                ATC_MetroDepot = true;
//                                ATC_SeibuDepot = ATC_TokyuDepot = false;
//                            }
//                        } else {
//                            ATC_TokyuDepot = ATC_SeibuDepot = ATC_MetroDepot = false;
//                        }

//                        var lastAnn = SignalAnn;
//                        SignalAnn = ATCSpeed > (ATCLimits[NextSection.CurrentSignalIndex] < 0 ? 0 : ATCLimits[NextSection.CurrentSignalIndex]) && !inDepot;
//                        if (SignalAnn) {
//                            if (ATCType == -1) ATC_SignalAnn = Time % 2000 < 1000;
//                            else if (ATCType == 2) ATC_SignalAnn = false;
//                            else if (ATCType == 3) ATC_SignalAnn = true;
//                        } else ATC_SignalAnn = false;

//                        if (lastAnn != SignalAnn && ATCType == -1) ATC_SignalAnnBeep.StopAndPlay();

//                        if (Speed > StationPattern.AtLocation(Location, Config.EBDec) || Speed > ORPPattern.AtLocation(Location, ORPPatternDec))
//                            EBUntilStop = true;

//                        if (Speed > ATCSpeed + 1 && ATCSpeed != -1) {
//                            if (Speed >= ATCSpeed + 3) {
//                                if (!ServiceBrake) ServiceBrake = true;
//                                if (BrakeStartTime == Config.LessInf) BrakeStartTime = Time;
//                            } else {
//                                ServiceBrake = false;
//                                BrakeStartTime = Config.LessInf;
//                                if (ATCType != 2)
//                                    BrakeCommand = (int)Math.Ceiling(MetroAts.vehicleSpec.BrakeNotches * 0.5);
//                                else BrakeCommand = MetroAts.vehicleSpec.BrakeNotches;
//                            }
//                        } else {
//                            ServiceBrake = false;
//                            BrakeStartTime = Config.LessInf;
//                        }

//                        if (ServiceBrake) {
//                            if (ATCType != 2) {
//                                if (Time - BrakeStartTime < 1500) BrakeCommand = (int)Math.Ceiling(MetroAts.vehicleSpec.BrakeNotches * 0.5);
//                                else BrakeCommand = MetroAts.vehicleSpec.BrakeNotches;
//                            } else BrakeCommand = MetroAts.vehicleSpec.BrakeNotches;
//                        }

//                        if (EBUntilStop) {
//                            BrakeCommand = Math.Max(BrakeCommand, MetroAts.vehicleSpec.BrakeNotches + 1);
//                            if (Speed == 0 && handleOnEB) EBUntilStop = false;
//                        }

//                        if (ORPPattern != SpeedPattern.inf && (Speed > ORPPattern.AtLocation(Location, ORPPatternDec) || Speed < 5)) {
//                            ORPPattern = new SpeedPattern(7, Location);
//                        }

//                        if (ORPPattern != SpeedPattern.inf) {
//                            if (ATCType == -1) {
//                                ATC_P = Time % 1000 < 500;
//                            } else if (ATCType == 3) {
//                                ATC_P = true;
//                                ORPNeedle = (int)ORPSpeed * 10;
//                            }
//                        } else {
//                            ORPNeedle = (int)ORPSpeed * 10;
//                            ATC_P = false;
//                        }

//                        //ATC速度指示
//                        if (!inDepot) {
//                            if (!Config.ATCLimitUseNeedle) {
//                                ATC_01 = ATCSpeed == 0;
//                                ATC_10 = ATCSpeed == 10;
//                                ATC_15 = ATCSpeed == 15;
//                                ATC_20 = ATCSpeed == 20;
//                                ATC_25 = ATCSpeed == 25;
//                                ATC_30 = ATCSpeed == 30;
//                                ATC_35 = ATCSpeed == 35;
//                                ATC_40 = ATCSpeed == 40;
//                                ATC_45 = ATCSpeed == 45;
//                                ATC_50 = ATCSpeed == 50;
//                                ATC_55 = ATCSpeed == 55;
//                                ATC_60 = ATCSpeed == 60;
//                                ATC_65 = ATCSpeed == 65;
//                                ATC_70 = ATCSpeed == 70;
//                                ATC_75 = ATCSpeed == 75;
//                                ATC_80 = ATCSpeed == 80;
//                                ATC_85 = ATCSpeed == 85;
//                                ATC_90 = ATCSpeed == 90;
//                                ATC_95 = ATCSpeed == 95;
//                                ATC_100 = ATCSpeed == 100;
//                                ATC_110 = ATCSpeed == 110;
//                            } else {
//                                ATCNeedle = ATCSpeed;
//                                ATCNeedle_Disappear = (ATCSpeed != -1 || ORPNeedle > 0) ? 1 : 0;
//                            }

//                            //進行・停止
//                            ATC_Stop = ATCSpeed == 0 || ATCSpeed == -1;
//                            ATC_Proceed = ATCSpeed > 0;
//                        } else {
//                            ATC_01 = false;
//                            ATC_10 = false;
//                            ATC_15 = false;
//                            ATC_20 = false;
//                            ATC_25 = false;
//                            ATC_30 = false;
//                            ATC_35 = false;
//                            ATC_40 = false;
//                            ATC_45 = false;
//                            ATC_50 = false;
//                            ATC_55 = false;
//                            ATC_60 = false;
//                            ATC_65 = false;
//                            ATC_70 = false;
//                            ATC_75 = false;
//                            ATC_80 = false;
//                            ATC_85 = false;
//                            ATC_90 = false;
//                            ATC_95 = false;
//                            ATC_100 = false;
//                            ATC_110 = false;

//                            ATCNeedle = 0;
//                            ATCNeedle_Disappear = 1;

//                            //進行・停止
//                            ATC_Stop = ATC_Proceed = false;
//                        }
//                    }
//                }
//            } else {
//                BrakeCommand = 0;

//                ATC_Ding.Stop();
//                ATC_SignalAnnBeep.Stop();
//                ATC_ORPBeep.Stop();
//                ATC_EmergencyOperationAnnounce.Stop();
//                ATC_WarningBell.Stop();

//                ATC_01 = false;
//                ATC_10 = false;
//                ATC_15 = false;
//                ATC_20 = false;
//                ATC_25 = false;
//                ATC_30 = false;
//                ATC_35 = false;
//                ATC_40 = false;
//                ATC_45 = false;
//                ATC_50 = false;
//                ATC_55 = false;
//                ATC_60 = false;
//                ATC_65 = false;
//                ATC_70 = false;
//                ATC_75 = false;
//                ATC_80 = false;
//                ATC_85 = false;
//                ATC_90 = false;
//                ATC_95 = false;
//                ATC_100 = false;
//                ATC_110 = false;

//                ATC_Stop = false;
//                ATC_Proceed = false;

//                ATC_P = false;
//                ATC_X = false;

//                ORPNeedle = 0;
//                ATCNeedle = 0;
//                ATCNeedle_Disappear = 1;

//                ATC_SeibuATC = false;
//                ATC_MetroATC = false;
//                ATC_TokyuATC = false;

//                ATC_SignalAnn = false;
//                ATC_SeibuNoset = false;
//                ATC_TokyuNoset = false;
//                ATC_MetroNoset = false;
//                ATC_TempLimit = false;

//                ATC_TokyuDepot = false;
//                ATC_SeibuDepot = false;
//                ATC_MetroDepot = false;
//                ATC_SeibuServiceBrake = false;
//                ATC_MetroAndTokyuServiceBrake = false;
//                ATC_SeibuEmergencyBrake = false;
//                ATC_MetroAndTokyuEmergencyBrake = false;
//                //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
//                ATC_TokyuStationStop = false;
//                ATC_SeibuStationStop = false;
//            }
//        }

//        public static void Disable() {
//            ATCEnable = false;

//            BrakeCommand = 0;

//            ATC_Ding.Stop();
//            ATC_SignalAnnBeep.Stop();
//            ATC_ORPBeep.Stop();
//            ATC_EmergencyOperationAnnounce.Stop();
//            ATC_WarningBell.Stop();

//            ATC_01 = false;
//            ATC_10 = false;
//            ATC_15 = false;
//            ATC_20 = false;
//            ATC_25 = false;
//            ATC_30 = false;
//            ATC_35 = false;
//            ATC_40 = false;
//            ATC_45 = false;
//            ATC_50 = false;
//            ATC_55 = false;
//            ATC_60 = false;
//            ATC_65 = false;
//            ATC_70 = false;
//            ATC_75 = false;
//            ATC_80 = false;
//            ATC_85 = false;
//            ATC_90 = false;
//            ATC_95 = false;
//            ATC_100 = false;
//            ATC_110 = false;

//            ATC_Stop = false;
//            ATC_Proceed = false;

//            ATC_P = false;
//            ATC_X = false;

//            ORPNeedle = 0;
//            ATCNeedle = 0;
//            ATCNeedle_Disappear = 1;

//            ATC_SeibuATC = false;
//            ATC_MetroATC = false;
//            ATC_TokyuATC = false;

//            ATC_SignalAnn = false;
//            ATC_SeibuNoset = false;
//            ATC_TokyuNoset = false;
//            ATC_MetroNoset = false;
//            ATC_TempLimit = false;

//            ATC_TokyuDepot = false;
//            ATC_SeibuDepot = false;
//            ATC_MetroDepot = false;
//            ATC_SeibuServiceBrake = false;
//            ATC_MetroAndTokyuServiceBrake = false;
//            ATC_SeibuEmergencyBrake = false;
//            ATC_MetroAndTokyuEmergencyBrake = false;
//            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
//            ATC_TokyuStationStop = false;
//            ATC_SeibuStationStop = false;

//        }
//    }
//}
