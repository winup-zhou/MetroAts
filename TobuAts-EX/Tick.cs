using AtsEx.PluginHost;
using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Sound;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace TobuAts_EX
{
    public partial class TobuAts : AssemblyPluginBase {
        [PluginType(PluginType.VehiclePlugin)]

        private SectionManager sectionManager;

        //panel -> ATC
        private readonly IAtsPanelValue<bool> ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45, ATC_50, ATC_55, ATC_60, ATC_65, ATC_70,
            ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110,
            ATC_Stop, ATC_Proceed, ATC_P, ATC_TobuATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_PatternApproach;
        private readonly IAtsPanelValue<double> ORPNeedle;
        private readonly IAtsPanelValue<int> ATC_EndPointDistance;

        //panel -> ATS
        private readonly IAtsPanelValue<bool> ATS_TobuATS, ATS_ATSEmergencyBrake, ATS_EmergencyOperation, ATS_Confirm, ATS_60, ATS_15;

        //InternalValue -> ATC
        public static int[] ATCLimits = { -2, 25, 55, 75, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120, -1, -2, -2, -1, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };
        
        public static SpeedLimit ATCPattern = new SpeedLimit(), StationPattern = new SpeedLimit();

        public static int NowSig, LastSig, TrackPos, MaxDis;

        public static double CurrentDis = 0 , SelfTrainLocation = 0;

        public static bool Ding = false, Plamp = false, NextStop = false, inDepot = false, RfSig = false;

        public static int[] SectionLimits = { -5, -5, -5, -5, -5, -5, -5, -5, -5 };
        public static double[] SectionDistance = { -5, -5, -5, -5, -5, -5, -5, -5, -5 };

        const double SignalPatternDec = -2.25; //*10
        const double StationPatternDec = -4.0;

        public static int DingStartTime;

        public TobuAts(PluginBuilder services) : base(services) {
            Native.BeaconPassed += BeaconPassed;

            BveHacker.ScenarioCreated += OnScenarioCreated;
        }

        private void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 42:
                    if (e.Optional <= 3) TrackPos = e.Optional;
                    break;
                case 43:
                    NextStop = true;
                    StationPattern = new SpeedLimit { Limit = 0, Location = SelfTrainLocation + 510 };
                    break;
                case 44:
                    NextStop = true;
                    StationPattern = new SpeedLimit { Limit = 0, Location = SelfTrainLocation };
                    break;
            }
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }

        public override TickResult Tick(TimeSpan elapsed) {
            var state = Native.VehicleState;

            AtsEx.PluginHost.Handles.HandleSet handles = Native.Handles;
            VehiclePluginTickResult tickResult = new VehiclePluginTickResult();

            SelfTrainLocation = state.Location;

            return tickResult;
        }

        public override void Dispose() {
            BveHacker.ScenarioCreated -= OnScenarioCreated;
        }
    }
}
