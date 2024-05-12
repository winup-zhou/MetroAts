using AtsEx.Extensions.PreTrainPatch;
using AtsEx.PluginHost;
using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using SlimDX.DirectInput;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace MetroAts {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class MetroAts : AssemblyPluginBase {
        static MetroAts() {
            Config.Load();
        }

        private SectionManager sectionManager;
        public static AtsEx.PluginHost.Native.VehicleSpec vehicleSpec;
        public static AtsEx.PluginHost.Native.VehicleState state = new AtsEx.PluginHost.Native.VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
        public static AtsEx.PluginHost.Handles.HandleSet handles;
        public static Section CurrentSection, NextSection, Next2Section;

        //sounds
        public static IAtsSound Switchover, KeyOn, KeyOff, ATCCgS, ResetSW;
        public static IAtsSound ATC_Ding, ATC_PatternApproachBeep, ATC_StationStopAnnounce, ATC_EmergencyOperationAnnounce, ATC_WarningBell,
            ATC_SignalAnnBeep, ATC_ORPBeep, ATS_Chime;

        //panels
        public static IAtsPanelValue<bool> ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45,
            ATC_50, ATC_55, ATC_60, ATC_65, ATC_70, ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110, ATC_Stop, ATC_Proceed,
            ATC_P, ATC_TobuATC, ATC_SeibuATC, ATC_MetroATC, ATC_TokyuATC, ATC_TobuDepot, ATC_SeibuDepot, ATC_MetroDepot, ATC_TokyuDepot,
            ATC_TobuServiceBrake, ATC_SeibuServiceBrake, ATC_MetroAndTokyuServiceBrake, ATC_TobuEmergencyBrake, ATC_SeibuEmergencyBrake,
            ATC_MetroAndTokyuEmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach, ATC_TobuStationStop, ATC_TokyuStationStop,
            ATC_SeibuStationStop, ATC_SignalAnn, ATC_SeibuNoset, ATC_MetroNoset, ATC_TokyuNoset, ATC_TempLimit;
        public static IAtsPanelValue<int> ORPNeedle, ATCNeedle, ATCNeedle_Disappear, ATC_EndPointDistance, ATC_SwitcherPosition;
        public static IAtsPanelValue<bool> ATS_TobuATS, ATS_ATSEmergencyBrake, ATS_EmergencyOperation, ATS_Confirm, ATS_60, ATS_15;
        public static IAtsPanelValue<bool> P_Power, P_PatternApproach, P_BrakeActioned, P_EBActioned, P_BrakeOverride, P_PEnable, P_Fail,
            SN_Power, SN_Action;
        public static IAtsPanelValue<int> PowerOutput, BrakeOutput;

        public static double LastUpdateTime = 0, PretrainLocation = 0;
        public static int SignalMode = 0; //0:東武 1:西武 2:ATC 3:相鉄 4:非設
        public static int KeyPosition = 0; //-1:東急 0:OFF 1:東武 2:西武 3:メトロ 4:相鉄
        public static bool SignalEnable = false, CanSwitch = false;

        public MetroAts(PluginBuilder services) : base(services) {
            SignalMode = 0;
            KeyPosition = 0;
            SignalEnable = false;
            CanSwitch = false;
            LastUpdateTime = 0;
            PretrainLocation = 0;

            TSP_ATS.Native = Native;
            T_DATC.Native = Native;
            ATC.Native = Native;

            Switchover = Native.AtsSounds.Register(118);
            KeyOn = Native.AtsSounds.Register(10);
            KeyOff = Native.AtsSounds.Register(11);
            ATCCgS = Native.AtsSounds.Register(22);
            ResetSW = Native.AtsSounds.Register(24);

            ATC_Ding = Native.AtsSounds.Register(2);
            ATC_PatternApproachBeep = Native.AtsSounds.Register(116);
            ATC_StationStopAnnounce = Native.AtsSounds.Register(117);
            ATC_EmergencyOperationAnnounce = Native.AtsSounds.Register(119);
            ATC_WarningBell = Native.AtsSounds.Register(0);
            ATC_SignalAnnBeep = Native.AtsSounds.Register(4);
            ATC_ORPBeep = Native.AtsSounds.Register(3);

            ATS_Chime = Native.AtsSounds.Register(1);

            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.Started += Initialize;

            Native.NativeKeys.AtsKeys[NativeAtsKeyName.A1].Pressed += OnA1Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.A2].Pressed += OnA2Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed += OnB1Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B2].Pressed += OnB2Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.G].Pressed += OnGPressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.H].Pressed += OnHPressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.I].Pressed += OnIPressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.J].Pressed += OnJPressed;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            vehicleSpec = Native.VehicleSpec;

            P_Power = Native.AtsPanelValues.RegisterBoolean(2);
            P_PatternApproach = Native.AtsPanelValues.RegisterBoolean(3);
            P_BrakeActioned = Native.AtsPanelValues.RegisterBoolean(5);
            P_EBActioned = Native.AtsPanelValues.RegisterBoolean(8);
            P_BrakeOverride = Native.AtsPanelValues.RegisterBoolean(4);
            P_PEnable = Native.AtsPanelValues.RegisterBoolean(6);
            P_Fail = Native.AtsPanelValues.RegisterBoolean(7);
            SN_Power = Native.AtsPanelValues.RegisterBoolean(0);
            SN_Action = Native.AtsPanelValues.RegisterBoolean(1);

            ATC_01 = Native.AtsPanelValues.RegisterBoolean(102);
            ATC_10 = Native.AtsPanelValues.RegisterBoolean(104);
            ATC_15 = Native.AtsPanelValues.RegisterBoolean(105);
            ATC_20 = Native.AtsPanelValues.RegisterBoolean(106);
            ATC_25 = Native.AtsPanelValues.RegisterBoolean(107);
            ATC_30 = Native.AtsPanelValues.RegisterBoolean(108);
            ATC_35 = Native.AtsPanelValues.RegisterBoolean(109);
            ATC_40 = Native.AtsPanelValues.RegisterBoolean(110);
            ATC_45 = Native.AtsPanelValues.RegisterBoolean(111);
            ATC_50 = Native.AtsPanelValues.RegisterBoolean(112);
            ATC_55 = Native.AtsPanelValues.RegisterBoolean(113);
            ATC_60 = Native.AtsPanelValues.RegisterBoolean(114);
            ATC_65 = Native.AtsPanelValues.RegisterBoolean(115);
            ATC_70 = Native.AtsPanelValues.RegisterBoolean(116);
            ATC_75 = Native.AtsPanelValues.RegisterBoolean(117);
            ATC_80 = Native.AtsPanelValues.RegisterBoolean(118);
            ATC_85 = Native.AtsPanelValues.RegisterBoolean(119);
            ATC_90 = Native.AtsPanelValues.RegisterBoolean(120);
            ATC_95 = Native.AtsPanelValues.RegisterBoolean(121);
            ATC_100 = Native.AtsPanelValues.RegisterBoolean(122);
            ATC_110 = Native.AtsPanelValues.RegisterBoolean(124);

            ATC_Stop = Native.AtsPanelValues.RegisterBoolean(131);
            ATC_Proceed = Native.AtsPanelValues.RegisterBoolean(132);

            ATC_P = Native.AtsPanelValues.RegisterBoolean(134);
            ATC_X = Native.AtsPanelValues.RegisterBoolean(101);

            ORPNeedle = Native.AtsPanelValues.RegisterInt32(135);
            ATCNeedle = Native.AtsPanelValues.RegisterInt32(127);
            ATCNeedle_Disappear = Native.AtsPanelValues.RegisterInt32(103);

            ATC_TobuATC = Native.AtsPanelValues.RegisterBoolean(74);
            ATC_SeibuATC = Native.AtsPanelValues.RegisterBoolean(20);
            ATC_MetroATC = Native.AtsPanelValues.RegisterBoolean(19);
            ATC_TokyuATC = Native.AtsPanelValues.RegisterBoolean(21);

            ATC_SignalAnn = Native.AtsPanelValues.RegisterBoolean(133);
            ATC_SeibuNoset = Native.AtsPanelValues.RegisterBoolean(28);
            ATC_MetroNoset = Native.AtsPanelValues.RegisterBoolean(29);
            ATC_TokyuNoset = Native.AtsPanelValues.RegisterBoolean(30);
            //ATC_TempLimit = Native.AtsPanelValues.RegisterBoolean();

            ATC_TobuDepot = Native.AtsPanelValues.RegisterBoolean(75);
            ATC_SeibuDepot = Native.AtsPanelValues.RegisterBoolean(33);
            ATC_MetroDepot = Native.AtsPanelValues.RegisterBoolean(31);
            ATC_TokyuDepot = Native.AtsPanelValues.RegisterBoolean(32);

            ATC_TobuServiceBrake = Native.AtsPanelValues.RegisterBoolean(77);
            ATC_SeibuServiceBrake = Native.AtsPanelValues.RegisterBoolean(26);
            ATC_MetroAndTokyuServiceBrake = Native.AtsPanelValues.RegisterBoolean(23);
            ATC_TobuEmergencyBrake = Native.AtsPanelValues.RegisterBoolean(76);
            ATC_SeibuEmergencyBrake = Native.AtsPanelValues.RegisterBoolean(25);
            ATC_MetroAndTokyuEmergencyBrake = Native.AtsPanelValues.RegisterBoolean(22);
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_TobuStationStop = Native.AtsPanelValues.RegisterBoolean(252);
            ATC_TokyuStationStop = Native.AtsPanelValues.RegisterBoolean(254);
            ATC_SeibuStationStop = Native.AtsPanelValues.RegisterBoolean(253);

            ATC_PatternApproach = Native.AtsPanelValues.RegisterBoolean(128);
            ATC_EndPointDistance = Native.AtsPanelValues.RegisterInt32(129);

            ATC_SwitcherPosition = Native.AtsPanelValues.RegisterInt32(130);

            ATS_TobuATS = Native.AtsPanelValues.RegisterBoolean(41);
            ATS_ATSEmergencyBrake = Native.AtsPanelValues.RegisterBoolean(44);
            //ATS_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            //ATS_Confirm = Native.AtsPanelValues.RegisterBoolean(512);
            ATS_60 = Native.AtsPanelValues.RegisterBoolean(43);
            ATS_15 = Native.AtsPanelValues.RegisterBoolean(42);

            PowerOutput = Native.AtsPanelValues.RegisterInt32(66);
            BrakeOutput = Native.AtsPanelValues.RegisterInt32(51);
        }

        private void OnA1Pressed(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void OnA2Pressed(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void OnB1Pressed(object sender, EventArgs e) {
            TSP_ATS.OnB1Pressed(sender, e);
            ATS_P_SN.OnB1Pressed(sender, e);
        }

        private void OnB2Pressed(object sender, EventArgs e) {
            ATS_P_SN.OnB2Pressed(sender, e);
        }

        private void OnGPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (SignalMode != 4) {
                    ATCCgS.Play();
                    SignalMode += 1;
                }
            }
        }

        private void OnHPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (SignalMode != 0) {
                    ATCCgS.Play();
                    SignalMode -= 1;
                }
            }
        }

        //
        private void OnIPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (KeyPosition > 0) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition = 0;
                    KeyOff.Play();
                } else if (KeyPosition == 0) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition = -1;
                    KeyOn.Play();
                }
            }
        }

        private void OnJPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (KeyPosition == -1) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition = 0;
                    KeyOff.Play();
                } else if (KeyPosition != 4) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition += 1;
                    KeyOn.Play();
                }
            }
        }

        private void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            if (e.DefaultBrakePosition == BrakePosition.Removed) SignalEnable = false;
            T_DATC.Initialize(e);
            TSP_ATS.Initialize(e);
            ATC.Initialize(e);
            ATS_P_SN.Initialize(e);
        }

        private void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            T_DATC.DoorOpened(e);
            TSP_ATS.DoorOpened(e);
            ATC.DoorOpened(e);
        }

        private void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            T_DATC.BeaconPassed(e);
            TSP_ATS.BeaconPassed(e);
            ATC.BeaconPassed(e);
            ATS_P_SN.BeaconPassed(e);
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }

        public override TickResult Tick(TimeSpan elapsed) {
            state = Native.VehicleState;
            handles = Native.Handles;
            VehiclePluginTickResult tickResult = new VehiclePluginTickResult();
            NotchCommandBase powerCommand = handles.Power.GetCommandToSetNotchTo(handles.Power.Notch);
            NotchCommandBase brakeCommand = handles.Brake.GetCommandToSetNotchTo(handles.Brake.Notch);
            ReverserPositionCommandBase reverserCommand = ReverserPositionCommandBase.Continue;
            ConstantSpeedCommand? constantSpeedCommand = ConstantSpeedCommand.Continue;

            CanSwitch = handles.Brake.Notch == vehicleSpec.BrakeNotches + 1 && handles.Reverser.Position == ReverserPosition.N && state.Speed == 0;

            //閉塞情報
            int pointer = 0, pointer_ = 0;
            while (sectionManager.Sections[pointer].Location < state.Location)
                pointer++;
            if (pointer >= sectionManager.Sections.Count)
                pointer = sectionManager.Sections.Count - 1;

            while (sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location < state.Location)
                pointer_++;
            if (pointer_ >= sectionManager.StopSignalSectionIndexes.Count)
                pointer_ = sectionManager.StopSignalSectionIndexes.Count - 1;

            CurrentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;
            NextSection = sectionManager.Sections[pointer] as Section;
            Next2Section = sectionManager.Sections[pointer + 1 >= sectionManager.Sections.Count ? sectionManager.Sections.Count - 1 : pointer + 1] as Section;

            PretrainLocation = sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location;

            if (SignalEnable) {
                if (SignalMode == 0) { //東武
                    if (ATC.ATCEnable) ATC.Disable();
                    ATS_P_SN.Disable();
                    if (KeyPosition == 1) {
                        if (CurrentSection.CurrentSignalIndex >= 9 && CurrentSection.CurrentSignalIndex != 34 && CurrentSection.CurrentSignalIndex < 49) {
                            //T-DATC
                            if (T_DATC.ATCEnable) {
                                T_DATC.Tick(state.Location, state.Speed, state.Time.TotalMilliseconds, CurrentSection, NextSection, Next2Section, PretrainLocation);
                                brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(T_DATC.BrakeCommand, handles.Brake.Notch));
                                powerCommand = handles.Power.GetCommandToSetNotchTo(T_DATC.BrakeCommand > 0 ? 0 : handles.Power.Notch);
                            } else {
                                if (TSP_ATS.ATSEnable) {
                                    TSP_ATS.Disable();
                                    T_DATC.Enable(state.Time.TotalMilliseconds - 5000);
                                    Switchover.Play();
                                } else {
                                    T_DATC.Enable(state.Time.TotalMilliseconds);
                                }
                            }
                        } else {
                            //TSP-ATS
                            if (TSP_ATS.ATSEnable) {
                                TSP_ATS.Tick(state.Location, state.Speed, state.Time.TotalMilliseconds, NextSection);
                                brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(TSP_ATS.BrakeCommand, handles.Brake.Notch));
                                powerCommand = handles.Power.GetCommandToSetNotchTo(TSP_ATS.BrakeCommand > 0 ? 0 : handles.Power.Notch);
                            } else {
                                if (T_DATC.ATCEnable) {
                                    T_DATC.Disable();
                                    TSP_ATS.Enable(state.Time.TotalMilliseconds - 5000);
                                    Switchover.Play();
                                } else {
                                    TSP_ATS.Enable(state.Time.TotalMilliseconds);
                                }
                            }
                        }
                    } else {
                        ATCNeedle_Disappear.Value = 1;
                        //reverserCommand = handles.Reverser.Position;
                        brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(vehicleSpec.BrakeNotches + 1, handles.Brake.Notch));
                    }
                    ATC_01.Value = T_DATC.ATC_01;
                    ATC_10.Value = T_DATC.ATC_10;
                    ATC_15.Value = T_DATC.ATC_15;
                    ATC_20.Value = T_DATC.ATC_20;
                    ATC_25.Value = T_DATC.ATC_25;
                    ATC_30.Value = T_DATC.ATC_30;
                    ATC_35.Value = T_DATC.ATC_35;
                    ATC_40.Value = T_DATC.ATC_40;
                    ATC_45.Value = T_DATC.ATC_45;
                    ATC_50.Value = T_DATC.ATC_50;
                    ATC_55.Value = T_DATC.ATC_55;
                    ATC_60.Value = T_DATC.ATC_60;
                    ATC_65.Value = T_DATC.ATC_65;
                    ATC_70.Value = T_DATC.ATC_70;
                    ATC_75.Value = T_DATC.ATC_75;
                    ATC_80.Value = T_DATC.ATC_80;
                    ATC_85.Value = T_DATC.ATC_85;
                    ATC_90.Value = T_DATC.ATC_90;
                    ATC_95.Value = T_DATC.ATC_95;
                    ATC_100.Value = T_DATC.ATC_100;
                    ATC_110.Value = T_DATC.ATC_110;

                    ATC_Stop.Value = T_DATC.ATC_Stop;
                    ATC_Proceed.Value = T_DATC.ATC_Proceed;

                    ATC_P.Value = T_DATC.ATC_P;
                    ATC_X.Value = T_DATC.ATC_X;

                    ORPNeedle.Value = T_DATC.ORPNeedle;
                    ATCNeedle.Value = T_DATC.ATCNeedle;
                    ATCNeedle_Disappear.Value = T_DATC.ATCNeedle_Disappear;
                    ATC_EndPointDistance.Value = T_DATC.ATC_EndPointDistance;
                    ATC_SwitcherPosition.Value = T_DATC.ATC_SwitcherPosition;

                    ATC_TobuATC.Value = T_DATC.ATC_TobuATC;
                    ATC_TobuDepot.Value = T_DATC.ATC_Depot;
                    ATC_TobuServiceBrake.Value = T_DATC.ATC_ServiceBrake;
                    ATC_TobuEmergencyBrake.Value = T_DATC.ATC_EmergencyBrake;
                    //ATC_EmergencyOperation.Value = T_DATC.ATC_EmergencyOperation;
                    ATC_TobuStationStop.Value = T_DATC.ATC_StationStop;
                    ATC_PatternApproach.Value = T_DATC.ATC_PatternApproach;

                    ATS_TobuATS.Value = TSP_ATS.ATS_TobuAts;
                    ATS_ATSEmergencyBrake.Value = TSP_ATS.ATS_ATSEmergencyBrake;
                    //ATS_EmergencyOperation.Value = TSP_ATS.ATS_EmergencyOperation;
                    //ATS_Confirm.Value = TSP_ATS.ATS_Confirm;
                    ATS_60.Value = TSP_ATS.ATS_60;
                    ATS_15.Value = TSP_ATS.ATS_15;

                    P_Power.Value = ATS_P_SN.P_Power;
                    P_PatternApproach.Value = ATS_P_SN.P_PatternApproach;
                    P_BrakeActioned.Value = ATS_P_SN.P_BrakeActioned;
                    P_EBActioned.Value = ATS_P_SN.P_EBActioned;
                    P_BrakeOverride.Value = ATS_P_SN.P_BrakeOverride;
                    P_PEnable.Value = ATS_P_SN.P_PEnable;
                    P_Fail.Value = ATS_P_SN.P_Fail;
                    SN_Power.Value = ATS_P_SN.SN_Power;
                    SN_Action.Value = ATS_P_SN.SN_Action;
                } else if (SignalMode == 1) {//西武
                    if (TSP_ATS.ATSEnable) TSP_ATS.Disable();
                    if (T_DATC.ATCEnable) T_DATC.Disable();
                    ATS_P_SN.Disable();
                } else if (SignalMode == 2) {//ATC
                    ATS_P_SN.Disable();
                    if (KeyPosition != 0) {
                        if (ATC.ATCEnable) {
                            ATC.Tick(state.Location, state.Speed, state.Time.TotalMilliseconds,
                            CurrentSection, NextSection, handles.Brake.Notch == vehicleSpec.BrakeNotches + 1, KeyPosition, false);
                            brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(ATC.BrakeCommand, handles.Brake.Notch));
                            powerCommand = handles.Power.GetCommandToSetNotchTo(ATC.BrakeCommand > 0 ? 0 : handles.Power.Notch);
                        } else {
                            if (TSP_ATS.ATSEnable) TSP_ATS.Disable();
                            if (T_DATC.ATCEnable) T_DATC.Disable();
                            ATC.Enable(state.Time.TotalMilliseconds);
                        }
                    }
                    ATC_01.Value = ATC.ATC_01;
                    ATC_10.Value = ATC.ATC_10;
                    ATC_15.Value = ATC.ATC_15;
                    ATC_20.Value = ATC.ATC_20;
                    ATC_25.Value = ATC.ATC_25;
                    ATC_30.Value = ATC.ATC_30;
                    ATC_35.Value = ATC.ATC_35;
                    ATC_40.Value = ATC.ATC_40;
                    ATC_45.Value = ATC.ATC_45;
                    ATC_50.Value = ATC.ATC_50;
                    ATC_55.Value = ATC.ATC_55;
                    ATC_60.Value = ATC.ATC_60;
                    ATC_65.Value = ATC.ATC_65;
                    ATC_70.Value = ATC.ATC_70;
                    ATC_75.Value = ATC.ATC_75;
                    ATC_80.Value = ATC.ATC_80;
                    ATC_85.Value = ATC.ATC_85;
                    ATC_90.Value = ATC.ATC_90;
                    ATC_95.Value = ATC.ATC_95;
                    ATC_100.Value = ATC.ATC_100;
                    ATC_110.Value = ATC.ATC_110;

                    ATC_Stop.Value = ATC.ATC_Stop;
                    ATC_Proceed.Value = ATC.ATC_Proceed;

                    ATC_P.Value = ATC.ATC_P;
                    ATC_X.Value = ATC.ATC_X;

                    ORPNeedle.Value = ATC.ORPNeedle;
                    ATCNeedle.Value = ATC.ATCNeedle;
                    ATCNeedle_Disappear.Value = ATC.ATCNeedle_Disappear;

                    ATC_SeibuATC.Value = ATC.ATC_SeibuATC;
                    ATC_MetroATC.Value = ATC.ATC_MetroATC;
                    ATC_TokyuATC.Value = ATC.ATC_TokyuATC;

                    ATC_SignalAnn.Value = ATC.ATC_SignalAnn;
                    ATC_SeibuNoset.Value = ATC.ATC_SeibuNoset;
                    ATC_TokyuNoset.Value = ATC.ATC_TokyuNoset;
                    ATC_MetroNoset.Value = ATC.ATC_MetroNoset;
                    //ATC_TempLimit.Value = ATC.ATC_TempLimit;

                    ATC_TokyuDepot.Value = ATC.ATC_TokyuDepot;
                    ATC_SeibuDepot.Value = ATC.ATC_SeibuDepot;
                    ATC_MetroDepot.Value = ATC.ATC_MetroDepot;
                    ATC_SeibuServiceBrake.Value = ATC.ATC_SeibuServiceBrake;
                    ATC_MetroAndTokyuServiceBrake.Value = ATC.ATC_MetroAndTokyuServiceBrake;
                    ATC_SeibuEmergencyBrake.Value = ATC.ATC_SeibuEmergencyBrake;
                    ATC_MetroAndTokyuEmergencyBrake.Value = ATC.ATC_MetroAndTokyuEmergencyBrake;
                    //ATC_EmergencyOperation = ;
                    ATC_TokyuStationStop.Value = ATC.ATC_TokyuStationStop;
                    ATC_SeibuStationStop.Value = ATC.ATC_SeibuStationStop;

                    ATC_EndPointDistance.Value = T_DATC.ATC_EndPointDistance;
                    ATC_SwitcherPosition.Value = T_DATC.ATC_SwitcherPosition;

                    ATC_TobuATC.Value = T_DATC.ATC_TobuATC;
                    ATC_TobuDepot.Value = T_DATC.ATC_Depot;
                    ATC_TobuServiceBrake.Value = T_DATC.ATC_ServiceBrake;
                    ATC_TobuEmergencyBrake.Value = T_DATC.ATC_EmergencyBrake;
                    //ATC_EmergencyOperation.Value = T_DATC.ATC_EmergencyOperation;
                    ATC_TobuStationStop.Value = T_DATC.ATC_StationStop;
                    ATC_PatternApproach.Value = T_DATC.ATC_PatternApproach;

                    ATS_TobuATS.Value = TSP_ATS.ATS_TobuAts;
                    ATS_ATSEmergencyBrake.Value = TSP_ATS.ATS_ATSEmergencyBrake;
                    //ATS_EmergencyOperation.Value = TSP_ATS.ATS_EmergencyOperation;
                    //ATS_Confirm.Value = TSP_ATS.ATS_Confirm;
                    ATS_60.Value = TSP_ATS.ATS_60;
                    ATS_15.Value = TSP_ATS.ATS_15;

                    P_Power.Value = ATS_P_SN.P_Power;
                    P_PatternApproach.Value = ATS_P_SN.P_PatternApproach;
                    P_BrakeActioned.Value = ATS_P_SN.P_BrakeActioned;
                    P_EBActioned.Value = ATS_P_SN.P_EBActioned;
                    P_BrakeOverride.Value = ATS_P_SN.P_BrakeOverride;
                    P_PEnable.Value = ATS_P_SN.P_PEnable;
                    P_Fail.Value = ATS_P_SN.P_Fail;
                    SN_Power.Value = ATS_P_SN.SN_Power;
                    SN_Action.Value = ATS_P_SN.SN_Action;
                } else if (SignalMode == 3) {//相鉄
                    if (ATS_P_SN.ATSEnable) {
                        ATS_P_SN.Tick(state.Location, state.Speed, state.Time.TotalMilliseconds);
                        brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(ATS_P_SN.BrakeCommand, handles.Brake.Notch));
                        powerCommand = handles.Power.GetCommandToSetNotchTo(ATS_P_SN.BrakeCommand > 0 ? 0 : handles.Power.Notch);
                    } else {
                        ATS_P_SN.Enable(state.Time.TotalMilliseconds);
                    }
                    ATC_01.Value = ATC.ATC_01;
                    ATC_10.Value = ATC.ATC_10;
                    ATC_15.Value = ATC.ATC_15;
                    ATC_20.Value = ATC.ATC_20;
                    ATC_25.Value = ATC.ATC_25;
                    ATC_30.Value = ATC.ATC_30;
                    ATC_35.Value = ATC.ATC_35;
                    ATC_40.Value = ATC.ATC_40;
                    ATC_45.Value = ATC.ATC_45;
                    ATC_50.Value = ATC.ATC_50;
                    ATC_55.Value = ATC.ATC_55;
                    ATC_60.Value = ATC.ATC_60;
                    ATC_65.Value = ATC.ATC_65;
                    ATC_70.Value = ATC.ATC_70;
                    ATC_75.Value = ATC.ATC_75;
                    ATC_80.Value = ATC.ATC_80;
                    ATC_85.Value = ATC.ATC_85;
                    ATC_90.Value = ATC.ATC_90;
                    ATC_95.Value = ATC.ATC_95;
                    ATC_100.Value = ATC.ATC_100;
                    ATC_110.Value = ATC.ATC_110;

                    ATC_Stop.Value = ATC.ATC_Stop;
                    ATC_Proceed.Value = ATC.ATC_Proceed;

                    ATC_P.Value = ATC.ATC_P;
                    ATC_X.Value = ATC.ATC_X;

                    ORPNeedle.Value = ATC.ORPNeedle;
                    ATCNeedle.Value = ATC.ATCNeedle;
                    ATCNeedle_Disappear.Value = ATC.ATCNeedle_Disappear;

                    ATC_SeibuATC.Value = ATC.ATC_SeibuATC;
                    ATC_MetroATC.Value = ATC.ATC_MetroATC;
                    ATC_TokyuATC.Value = ATC.ATC_TokyuATC;

                    ATC_SignalAnn.Value = ATC.ATC_SignalAnn;
                    ATC_SeibuNoset.Value = ATC.ATC_SeibuNoset;
                    ATC_TokyuNoset.Value = ATC.ATC_TokyuNoset;
                    ATC_MetroNoset.Value = ATC.ATC_MetroNoset;
                    //ATC_TempLimit.Value = ATC.ATC_TempLimit;

                    ATC_TokyuDepot.Value = ATC.ATC_TokyuDepot;
                    ATC_SeibuDepot.Value = ATC.ATC_SeibuDepot;
                    ATC_MetroDepot.Value = ATC.ATC_MetroDepot;
                    ATC_SeibuServiceBrake.Value = ATC.ATC_SeibuServiceBrake;
                    ATC_MetroAndTokyuServiceBrake.Value = ATC.ATC_MetroAndTokyuServiceBrake;
                    ATC_SeibuEmergencyBrake.Value = ATC.ATC_SeibuEmergencyBrake;
                    ATC_MetroAndTokyuEmergencyBrake.Value = ATC.ATC_MetroAndTokyuEmergencyBrake;
                    //ATC_EmergencyOperation = ;
                    ATC_TokyuStationStop.Value = ATC.ATC_TokyuStationStop;
                    ATC_SeibuStationStop.Value = ATC.ATC_SeibuStationStop;

                    ATC_EndPointDistance.Value = T_DATC.ATC_EndPointDistance;
                    ATC_SwitcherPosition.Value = T_DATC.ATC_SwitcherPosition;

                    ATC_TobuATC.Value = T_DATC.ATC_TobuATC;
                    ATC_TobuDepot.Value = T_DATC.ATC_Depot;
                    ATC_TobuServiceBrake.Value = T_DATC.ATC_ServiceBrake;
                    ATC_TobuEmergencyBrake.Value = T_DATC.ATC_EmergencyBrake;
                    //ATC_EmergencyOperation.Value = T_DATC.ATC_EmergencyOperation;
                    ATC_TobuStationStop.Value = T_DATC.ATC_StationStop;
                    ATC_PatternApproach.Value = T_DATC.ATC_PatternApproach;

                    ATS_TobuATS.Value = TSP_ATS.ATS_TobuAts;
                    ATS_ATSEmergencyBrake.Value = TSP_ATS.ATS_ATSEmergencyBrake;
                    //ATS_EmergencyOperation.Value = TSP_ATS.ATS_EmergencyOperation;
                    //ATS_Confirm.Value = TSP_ATS.ATS_Confirm;
                    ATS_60.Value = TSP_ATS.ATS_60;
                    ATS_15.Value = TSP_ATS.ATS_15;

                    P_Power.Value = ATS_P_SN.P_Power;
                    P_PatternApproach.Value = ATS_P_SN.P_PatternApproach;
                    P_BrakeActioned.Value = ATS_P_SN.P_BrakeActioned;
                    P_EBActioned.Value = ATS_P_SN.P_EBActioned;
                    P_BrakeOverride.Value = ATS_P_SN.P_BrakeOverride;
                    P_PEnable.Value = ATS_P_SN.P_PEnable;
                    P_Fail.Value = ATS_P_SN.P_Fail;
                    SN_Power.Value = ATS_P_SN.SN_Power;
                    SN_Action.Value = ATS_P_SN.SN_Action;
                } else if (SignalMode == 4) {//非設
                    TSP_ATS.Disable();
                    T_DATC.Disable();
                    ATS_P_SN.Disable();
                    if (KeyPosition != 0) {
                        if (ATC.ATCEnable) {
                            ATC.Tick(state.Location, state.Speed, state.Time.TotalMilliseconds,
                            CurrentSection, NextSection, handles.Brake.Notch == vehicleSpec.BrakeNotches + 1, KeyPosition, true);
                            brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(ATC.BrakeCommand, handles.Brake.Notch));
                            powerCommand = handles.Power.GetCommandToSetNotchTo(ATC.BrakeCommand > 0 ? 0 : handles.Power.Notch);
                        } else {
                            ATC.Enable(state.Time.TotalMilliseconds);
                        }
                    }
                    ATC_01.Value = ATC.ATC_01;
                    ATC_10.Value = ATC.ATC_10;
                    ATC_15.Value = ATC.ATC_15;
                    ATC_20.Value = ATC.ATC_20;
                    ATC_25.Value = ATC.ATC_25;
                    ATC_30.Value = ATC.ATC_30;
                    ATC_35.Value = ATC.ATC_35;
                    ATC_40.Value = ATC.ATC_40;
                    ATC_45.Value = ATC.ATC_45;
                    ATC_50.Value = ATC.ATC_50;
                    ATC_55.Value = ATC.ATC_55;
                    ATC_60.Value = ATC.ATC_60;
                    ATC_65.Value = ATC.ATC_65;
                    ATC_70.Value = ATC.ATC_70;
                    ATC_75.Value = ATC.ATC_75;
                    ATC_80.Value = ATC.ATC_80;
                    ATC_85.Value = ATC.ATC_85;
                    ATC_90.Value = ATC.ATC_90;
                    ATC_95.Value = ATC.ATC_95;
                    ATC_100.Value = ATC.ATC_100;
                    ATC_110.Value = ATC.ATC_110;

                    ATC_Stop.Value = ATC.ATC_Stop;
                    ATC_Proceed.Value = ATC.ATC_Proceed;

                    ATC_P.Value = ATC.ATC_P;
                    ATC_X.Value = ATC.ATC_X;

                    ORPNeedle.Value = ATC.ORPNeedle;
                    ATCNeedle.Value = ATC.ATCNeedle;
                    ATCNeedle_Disappear.Value = ATC.ATCNeedle_Disappear;

                    ATC_SeibuATC.Value = ATC.ATC_SeibuATC;
                    ATC_MetroATC.Value = ATC.ATC_MetroATC;
                    ATC_TokyuATC.Value = ATC.ATC_TokyuATC;

                    ATC_SignalAnn.Value = ATC.ATC_SignalAnn;
                    ATC_SeibuNoset.Value = ATC.ATC_SeibuNoset;
                    ATC_TokyuNoset.Value = ATC.ATC_TokyuNoset;
                    ATC_MetroNoset.Value = ATC.ATC_MetroNoset;
                    //ATC_TempLimit.Value = ATC.ATC_TempLimit;

                    ATC_TokyuDepot.Value = ATC.ATC_TokyuDepot;
                    ATC_SeibuDepot.Value = ATC.ATC_SeibuDepot;
                    ATC_MetroDepot.Value = ATC.ATC_MetroDepot;
                    ATC_SeibuServiceBrake.Value = ATC.ATC_SeibuServiceBrake;
                    ATC_MetroAndTokyuServiceBrake.Value = ATC.ATC_MetroAndTokyuServiceBrake;
                    ATC_SeibuEmergencyBrake.Value = ATC.ATC_SeibuEmergencyBrake;
                    ATC_MetroAndTokyuEmergencyBrake.Value = ATC.ATC_MetroAndTokyuEmergencyBrake;
                    //ATC_EmergencyOperation = ;
                    ATC_TokyuStationStop.Value = ATC.ATC_TokyuStationStop;
                    ATC_SeibuStationStop.Value = ATC.ATC_SeibuStationStop;

                    ATC_EndPointDistance.Value = T_DATC.ATC_EndPointDistance;
                    ATC_SwitcherPosition.Value = T_DATC.ATC_SwitcherPosition;

                    ATC_TobuATC.Value = T_DATC.ATC_TobuATC;
                    ATC_TobuDepot.Value = T_DATC.ATC_Depot;
                    ATC_TobuServiceBrake.Value = T_DATC.ATC_ServiceBrake;
                    ATC_TobuEmergencyBrake.Value = T_DATC.ATC_EmergencyBrake;
                    //ATC_EmergencyOperation.Value = T_DATC.ATC_EmergencyOperation;
                    ATC_TobuStationStop.Value = T_DATC.ATC_StationStop;
                    ATC_PatternApproach.Value = T_DATC.ATC_PatternApproach;

                    ATS_TobuATS.Value = TSP_ATS.ATS_TobuAts;
                    ATS_ATSEmergencyBrake.Value = TSP_ATS.ATS_ATSEmergencyBrake;
                    //ATS_EmergencyOperation.Value = TSP_ATS.ATS_EmergencyOperation;
                    //ATS_Confirm.Value = TSP_ATS.ATS_Confirm;
                    ATS_60.Value = TSP_ATS.ATS_60;
                    ATS_15.Value = TSP_ATS.ATS_15;
                }
            } else {
                if (TSP_ATS.ATSEnable) TSP_ATS.Disable();
                if (T_DATC.ATCEnable) T_DATC.Disable();
                if (ATC.ATCEnable) ATC.Disable();

                ATC_01.Value = false;
                ATC_10.Value = false;
                ATC_15.Value = false;
                ATC_20.Value = false;
                ATC_25.Value = false;
                ATC_30.Value = false;
                ATC_35.Value = false;
                ATC_40.Value = false;
                ATC_45.Value = false;
                ATC_50.Value = false;
                ATC_55.Value = false;
                ATC_60.Value = false;
                ATC_65.Value = false;
                ATC_70.Value = false;
                ATC_75.Value = false;
                ATC_80.Value = false;
                ATC_85.Value = false;
                ATC_90.Value = false;
                ATC_95.Value = false;
                ATC_100.Value = false;
                ATC_110.Value = false;

                ATC_Stop.Value = false;
                ATC_Proceed.Value = false;

                ATC_P.Value = false;
                ATC_X.Value = false;

                ORPNeedle.Value = 0;
                ATCNeedle.Value = 0;
                ATCNeedle_Disappear.Value = 0;

                ATC_SeibuATC.Value = false;
                ATC_MetroATC.Value = false;
                ATC_TokyuATC.Value = false;

                ATC_SignalAnn.Value = false;
                ATC_SeibuNoset.Value = false;
                ATC_TokyuNoset.Value = false;
                ATC_MetroNoset.Value = false;
                //ATC_TempLimit.Value = ATC.ATC_TempLimit;

                ATC_TokyuDepot.Value = false;
                ATC_SeibuDepot.Value = false;
                ATC_MetroDepot.Value = false;
                ATC_SeibuServiceBrake.Value = false;
                ATC_MetroAndTokyuServiceBrake.Value = false;
                ATC_SeibuEmergencyBrake.Value = false;
                ATC_MetroAndTokyuEmergencyBrake.Value = false;
                //ATC_EmergencyOperation = ;
                ATC_TokyuStationStop.Value = false;
                ATC_SeibuStationStop.Value = false;

                ATC_EndPointDistance.Value = 0;
                ATC_SwitcherPosition.Value = 0;

                ATC_TobuATC.Value = false;
                ATC_TobuDepot.Value = false;
                ATC_TobuServiceBrake.Value = false;
                ATC_TobuEmergencyBrake.Value = false;
                //ATC_EmergencyOperation.Value = T_DATC.ATC_EmergencyOperation;
                ATC_TobuStationStop.Value = false;
                ATC_PatternApproach.Value = false;

                ATS_TobuATS.Value = false;
                ATS_ATSEmergencyBrake.Value = false;
                //ATS_EmergencyOperation.Value = TSP_ATS.ATS_EmergencyOperation;
                //ATS_Confirm.Value = TSP_ATS.ATS_Confirm;
                ATS_60.Value = false;
                ATS_15.Value = false;

                ATCNeedle_Disappear.Value = 1;
                //reverserCommand = handles.Reverser.Position;
                brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(vehicleSpec.BrakeNotches + 1, handles.Brake.Notch));
                if (!SignalEnable && handles.Reverser.Position != ReverserPosition.N && handles.Brake.Notch != vehicleSpec.BrakeNotches + 1)
                    SignalEnable = true;
            }

            PowerOutput.Value = (int)powerCommand.GetOverridenNotch(handles.Power.Notch);
            BrakeOutput.Value = (int)brakeCommand.GetOverridenNotch(handles.Brake.Notch);

            tickResult.HandleCommandSet = new HandleCommandSet(powerCommand, brakeCommand, reverserCommand, constantSpeedCommand);

            return tickResult;
        }

        public override void Dispose() {
            SignalMode = 0;
            KeyPosition = 0;
            SignalEnable = false;
            CanSwitch = false;
            LastUpdateTime = 0;
            PretrainLocation = 0;

            Switchover.Dispose();
            KeyOn.Dispose();
            KeyOff.Dispose();
            ATCCgS.Dispose();
            ResetSW.Dispose();

            ATC_Ding.Dispose();
            ATC_PatternApproachBeep.Dispose();
            ATC_StationStopAnnounce.Dispose();
            ATC_EmergencyOperationAnnounce.Dispose();
            ATC_WarningBell.Dispose();
            ATC_SignalAnnBeep.Dispose();
            ATC_ORPBeep.Dispose();

            ATS_Chime.Dispose();

            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
            Native.Started -= Initialize;

            Native.NativeKeys.AtsKeys[NativeAtsKeyName.A1].Pressed -= OnA1Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.A2].Pressed -= OnA2Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed -= OnB1Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B2].Pressed -= OnB2Pressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.G].Pressed -= OnGPressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.H].Pressed -= OnHPressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.I].Pressed -= OnIPressed;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.J].Pressed -= OnJPressed;

            BveHacker.ScenarioCreated -= OnScenarioCreated;

            P_Power.Dispose();
            P_PatternApproach.Dispose();
            P_BrakeActioned.Dispose();
            P_EBActioned.Dispose();
            P_BrakeOverride.Dispose();
            P_PEnable.Dispose();
            P_Fail.Dispose();
            SN_Power.Dispose();
            SN_Action.Dispose();

            ATC_01.Dispose();
            ATC_10.Dispose();
            ATC_15.Dispose();
            ATC_20.Dispose();
            ATC_25.Dispose();
            ATC_30.Dispose();
            ATC_35.Dispose();
            ATC_40.Dispose();
            ATC_45.Dispose();
            ATC_50.Dispose();
            ATC_55.Dispose();
            ATC_60.Dispose();
            ATC_65.Dispose();
            ATC_70.Dispose();
            ATC_75.Dispose();
            ATC_80.Dispose();
            ATC_85.Dispose();
            ATC_90.Dispose();
            ATC_95.Dispose();
            ATC_100.Dispose();
            ATC_110.Dispose();

            ATC_Stop.Dispose();
            ATC_Proceed.Dispose();

            ATC_P.Dispose();
            ATC_X.Dispose();

            ORPNeedle.Dispose();
            ATCNeedle.Dispose();
            ATCNeedle_Disappear.Dispose();

            ATC_TobuATC.Dispose();
            ATC_SeibuATC.Dispose();
            ATC_MetroATC.Dispose();
            ATC_TokyuATC.Dispose();

            ATC_SignalAnn.Dispose();
            ATC_SeibuNoset.Dispose();
            ATC_MetroNoset.Dispose();
            ATC_TokyuNoset.Dispose();
            //ATC_TempLimit = Native.AtsPanelValues.RegisterBoolean();

            ATC_TobuDepot.Dispose();
            ATC_SeibuDepot.Dispose();
            ATC_MetroDepot.Dispose();
            ATC_TokyuDepot.Dispose();

            ATC_TobuServiceBrake.Dispose();
            ATC_SeibuServiceBrake.Dispose();
            ATC_MetroAndTokyuServiceBrake.Dispose();
            ATC_TobuEmergencyBrake.Dispose();
            ATC_SeibuEmergencyBrake.Dispose();
            ATC_MetroAndTokyuEmergencyBrake.Dispose();
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_TobuStationStop.Dispose();
            ATC_TokyuStationStop.Dispose();
            ATC_SeibuStationStop.Dispose();

            ATC_PatternApproach.Dispose();
            ATC_EndPointDistance.Dispose();

            ATC_SwitcherPosition.Dispose();

            ATS_TobuATS.Dispose();
            ATS_ATSEmergencyBrake.Dispose();
            //ATS_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            //ATS_Confirm = Native.AtsPanelValues.RegisterBoolean(512);
            ATS_60.Dispose();
            ATS_15.Dispose();
            PowerOutput.Dispose();
            BrakeOutput.Dispose();
        }
    }
}
