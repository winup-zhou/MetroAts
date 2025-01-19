using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuSignal {
    internal partial class T_DATC {
        public static void Initialize(BveEx.PluginHost.Native.StartedEventArgs e) {
            ATCPattern = new SpeedLimit();
            StationPattern = new SpeedLimit();
            DistanceDisplayPattern = new SpeedLimit();
            TrackPos = 0;
            StationStop = false;
            DistanceDisplay = false;
            BrakeCommand = 0;
            ORPlamp = false;
            ATCLimitSpeed = 0;
            LastSignal = 0;
            TargetSpeedUp = false;
            LastDingTime = Config.LessInf;
            TrackPosDisplayEndPos = 0;
            ATCEnable = false;

            ATC_Ding = MetroAts.ATC_Ding;
            ATC_PatternApproachBeep = MetroAts.ATC_PatternApproachBeep;
            ATC_StationStopAnnounce = MetroAts.ATC_StationStopAnnounce;
            //ATC_EmergencyOperationAnnounce = MetroAts.ATC_EmergencyOperationAnnounce;
            ATC_WarningBell = MetroAts.ATC_WarningBell;

            ATC_01 = false;
            ATC_10 = false;
            ATC_15 = false;
            ATC_20 = false;
            ATC_25 = false;
            ATC_30 = false;
            ATC_35 = false;
            ATC_40 = false;
            ATC_45 = false;
            ATC_50 = false;
            ATC_55 = false;
            ATC_60 = false;
            ATC_65 = false;
            ATC_70 = false;
            ATC_75 = false;
            ATC_80 = false;
            ATC_85 = false;
            ATC_90 = false;
            ATC_95 = false;
            ATC_100 = false;
            ATC_110 = false;

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_P = false;
            ATC_X = false;

            ORPNeedle = 0;
            ATCNeedle = 0;
            ATCNeedle_Disappear = 1;

            ATC_TobuATC = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_StationStop = false;

            ATC_PatternApproach = false;
            ATC_EndPointDistance = 0;

            ATC_SwitcherPosition = 0;
        }

        public static void Enable(double Time) {
            ATCEnable = true;
            InitializeStartTime = Time;
        }

        public static void BeaconPassed(BveEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 42:
                    if (e.Optional <= 3) {
                        TrackPos = e.Optional;
                        TrackPosDisplayEndPos = MetroAts.state.Location + e.Distance;
                        if (MetroAts.state.Location == 0) Flag = true;
                    }
                    break;
                case 43:
                    StationStop = true;
                    StationPattern = new SpeedLimit(0, MetroAts.state.Location + 510);
                    break;
                case 44:
                    StationStop = true;
                    StationPattern = new SpeedLimit(0, MetroAts.state.Location);
                    break;
            }
        }

        public static void DoorOpened(BveEx.PluginHost.Native.DoorEventArgs e) {
            StationStop = false;
        }



        public static void Disable() {
            ATCEnable = false;

            BrakeCommand = 0;

            ATC_Ding.Stop();
            ATC_PatternApproachBeep.Stop();
            ATC_StationStopAnnounce.Stop();
            //ATC_EmergencyOperationAnnounce.Stop();
            ATC_WarningBell.Stop();

            ATC_01 = false;
            ATC_10 = false;
            ATC_15 = false;
            ATC_20 = false;
            ATC_25 = false;
            ATC_30 = false;
            ATC_35 = false;
            ATC_40 = false;
            ATC_45 = false;
            ATC_50 = false;
            ATC_55 = false;
            ATC_60 = false;
            ATC_65 = false;
            ATC_70 = false;
            ATC_75 = false;
            ATC_80 = false;
            ATC_85 = false;
            ATC_90 = false;
            ATC_95 = false;
            ATC_100 = false;
            ATC_110 = false;

            ATC_Stop = false;
            ATC_Proceed = false;

            ATC_P = false;
            ATC_X = false;

            ORPNeedle = 0;
            ATCNeedle = 0;
            ATCNeedle_Disappear = 1;

            ATC_TobuATC = false;
            ATC_Depot = false;
            ATC_ServiceBrake = false;
            ATC_EmergencyBrake = false;
            //ATC_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            ATC_StationStop = false;

            ATC_PatternApproach = false;
            ATC_EndPointDistance = 0;

            ATC_SwitcherPosition = 0;
        }
    }
}
