using AtsEx.PluginHost;
using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;
using System.ComponentModel;

namespace MetroAts {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class MetroAts : AssemblyPluginBase {
        private SectionManager sectionManager;
        public static AtsEx.PluginHost.Native.VehicleSpec vehicleSpec;
        public static AtsEx.PluginHost.Native.VehicleState state = new AtsEx.PluginHost.Native.VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
        public static AtsEx.PluginHost.Handles.HandleSet handles;
        public static Section CurrentSection, NextSection, Next2Section;

        //sounds
        public static IAtsSound Switchover;

        //panels
        public static IAtsPanelValue<bool> ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45,
            ATC_50, ATC_55, ATC_60, ATC_65, ATC_70, ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110, ATC_Stop, ATC_Proceed,
            ATC_P, ATC_TobuATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach, ATC_StationStop;
        public static IAtsPanelValue<int> ORPNeedle, ATCNeedle, ATCNeedle_Disappear, ATC_EndPointDistance, ATC_SwitcherPosition;
        public static IAtsPanelValue<bool> ATS_TobuATS, ATS_ATSEmergencyBrake, ATS_EmergencyOperation, ATS_Confirm, ATS_60, ATS_15;
        public static IAtsSound ATC_Ding, ATC_PatternApproachBeep, ATC_StationStopAnnounce, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        public static int SignalMode = 0;
        public static bool SignalEnable = false;

        public MetroAts(PluginBuilder services) : base(services) {
            SignalMode = 0;
            SignalEnable = false;

            TSP_ATS.Native = Native;
            T_DATC.Native = Native;

            Switchover = Native.AtsSounds.Register(118);
            ATC_Ding = Native.AtsSounds.Register(2);
            ATC_PatternApproachBeep = Native.AtsSounds.Register(116);
            ATC_StationStopAnnounce = Native.AtsSounds.Register(117);
            ATC_EmergencyOperationAnnounce = Native.AtsSounds.Register(119);
            ATC_WarningBell = Native.AtsSounds.Register(0);

            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.Started += Initialize;

            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed += OnB1Pressed;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            vehicleSpec = Native.VehicleSpec;

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
            ATC_Depot = Native.AtsPanelValues.RegisterBoolean(75);
            ATC_ServiceBrake = Native.AtsPanelValues.RegisterBoolean(77);
            ATC_EmergencyBrake = Native.AtsPanelValues.RegisterBoolean(76);
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_StationStop = Native.AtsPanelValues.RegisterBoolean(252);

            ATC_PatternApproach = Native.AtsPanelValues.RegisterBoolean(128);
            ATC_EndPointDistance = Native.AtsPanelValues.RegisterInt32(129);

            ATC_SwitcherPosition = Native.AtsPanelValues.RegisterInt32(130);

            ATS_TobuATS = Native.AtsPanelValues.RegisterBoolean(41);
            ATS_ATSEmergencyBrake = Native.AtsPanelValues.RegisterBoolean(44);
            //ATS_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            //ATS_Confirm = Native.AtsPanelValues.RegisterBoolean(512);
            ATS_60 = Native.AtsPanelValues.RegisterBoolean(43);
            ATS_15 = Native.AtsPanelValues.RegisterBoolean(42);
        }

        private void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            if (e.DefaultBrakePosition == BrakePosition.Removed) SignalEnable = false;
            T_DATC.Initialize(e);
            TSP_ATS.Initialize(e);
        }

        private void OnB1Pressed(object sender, EventArgs e) {
            TSP_ATS.OnB1Pressed(sender, e);
        }

        private void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            T_DATC.DoorOpened(e);
            TSP_ATS.DoorOpened(e);
        }

        private void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            T_DATC.BeaconPassed(e);
            TSP_ATS.BeaconPassed(e);
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

            var PretrainLocation = sectionManager.Sections[sectionManager.StopSignalSectionIndexes[pointer_]].Location;

            if (SignalEnable) {
                if (CurrentSection.CurrentSignalIndex >= 9 && CurrentSection.CurrentSignalIndex < 49) {
                    //T-DATC
                    if (T_DATC.ATCEnable) {
                        T_DATC.Tick(state.Location, state.Speed, state.Time.TotalMilliseconds, CurrentSection, NextSection, Next2Section, PretrainLocation);
                        brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(T_DATC.BrakeCommand, handles.Brake.Notch));
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
                ATC_Depot.Value = T_DATC.ATC_Depot;
                ATC_ServiceBrake.Value = T_DATC.ATC_ServiceBrake;
                ATC_EmergencyBrake.Value = T_DATC.ATC_EmergencyBrake;
                //ATC_EmergencyOperation.Value = T_DATC.ATC_EmergencyOperation;
                ATC_StationStop.Value = T_DATC.ATC_StationStop;
                ATC_PatternApproach.Value = T_DATC.ATC_PatternApproach;

                ATS_TobuATS.Value = TSP_ATS.ATS_TobuAts;
                ATS_ATSEmergencyBrake.Value = TSP_ATS.ATS_ATSEmergencyBrake;
                //ATS_EmergencyOperation.Value = TSP_ATS.ATS_EmergencyOperation;
                //ATS_Confirm.Value = TSP_ATS.ATS_Confirm;
                ATS_60.Value = TSP_ATS.ATS_60;
                ATS_15.Value = TSP_ATS.ATS_15;
            } else {
                //reverserCommand = handles.Reverser.Position;
                brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(vehicleSpec.BrakeNotches + 1, handles.Brake.Notch));
                if (!SignalEnable && handles.Reverser.Position != ReverserPosition.N && handles.Brake.Notch != vehicleSpec.BrakeNotches + 1)
                    SignalEnable = true;
            }

            tickResult.HandleCommandSet = new HandleCommandSet(powerCommand, brakeCommand, reverserCommand, constantSpeedCommand);

            return tickResult;
        }

        public override void Dispose() {
            BveHacker.ScenarioCreated -= OnScenarioCreated;
            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed -= OnB1Pressed;
        }
    }
}
