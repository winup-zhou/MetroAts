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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MetroAts {
    public partial class MetroAts : AssemblyPluginBase {
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
                            brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(vehicleSpec.BrakeNotches + 1, handles.Brake.Notch));
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
                        brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(vehicleSpec.BrakeNotches + 1, handles.Brake.Notch));
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
                            brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(vehicleSpec.BrakeNotches + 1, handles.Brake.Notch));
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

            string KeyPos = "", ATCCgSPos = "";

            switch (KeyPosition) {
                case 0:
                    KeyPos = "未挿入";
                    break;
                case -1:
                    KeyPos = "東急";
                    break;
                case 1:
                    KeyPos = "東武";
                    break;
                case 2:
                    KeyPos = "西武";
                    break;
                case 3:
                    KeyPos = "メトロ";
                    break;
                case 4:
                    KeyPos = "相鉄";
                    break;
            }

            switch (SignalMode) {
                case 0:
                    ATCCgSPos = "東武";
                    break;
                case 1:
                    ATCCgSPos = "西武";
                    break;
                case 2:
                    ATCCgSPos = "ATC";
                    break;
                case 3:
                    ATCCgSPos = "相鉄";
                    break;
                case 4:
                    ATCCgSPos = "非設";
                    break;
            }


            description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
            leverText = (LeverText)BveHacker.MainForm.AssistantDrawer.Items.First(item => item is LeverText);
            leverText.Text = $"{description} | マスコンキー: {KeyPos}  保安装置: {ATCCgSPos}";

            PowerOutput.Value = (int)powerCommand.GetOverridenNotch(handles.Power.Notch);
            BrakeOutput.Value = (int)brakeCommand.GetOverridenNotch(handles.Brake.Notch);

            tickResult.HandleCommandSet = new HandleCommandSet(powerCommand, brakeCommand, reverserCommand, constantSpeedCommand);

            return tickResult;
        }
    }
}
