using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Sound.Native;
using AtsEx.PluginHost;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
