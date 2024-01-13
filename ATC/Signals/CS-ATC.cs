using AtsEx.PluginHost;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;

namespace ATC.Signals {
    internal class CS_ATC {
        public static INative Native;

        //InternalValue -> ATC
        public static int[] ATCLimits = { -2, 25, 55, 75, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120,
            -1, -2, -2, -1, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };
        private static SpeedLimit ORPPattern = new SpeedLimit(), StationPattern = new SpeedLimit(), DisplayPattern = new SpeedLimit();
        private static bool StationStop = false, SignalDown = false;

        private const double ORPPatternDec = -2.25; //*10
        private const double StationPatternDec = -4.0;

        public static int BrakeCommand = 0;


        //panel -> ATC
        private static IAtsPanelValue<bool> ATC_X, ATC_01, ATC_10, ATC_15, ATC_20, ATC_25, ATC_30, ATC_35, ATC_40, ATC_45, ATC_50, ATC_55, ATC_60, ATC_65, ATC_70,
            ATC_75, ATC_80, ATC_85, ATC_90, ATC_95, ATC_100, ATC_110,
            ATC_Stop, ATC_Proceed, ATC_Plamp, ATC_SeibuATC, ATC_Depot, ATC_ServiceBrake, ATC_EmergencyBrake, ATC_EmergencyOperation, ATC_StationStop, ATC_SignalDown;
        public static IAtsPanelValue<int> ORPNeedle, ATCNeedle, ATCNeedle_Disappear;
        private static IAtsSound ATC_Ding, ATC_EmergencyOperationAnnounce, ATC_WarningBell;

        public static void Tick(double Speed, double Location, Section currentSection, Section nextSection) {

        }

    }
}
