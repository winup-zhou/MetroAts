using AtsEx.PluginHost;
using AtsEx.PluginHost.Panels.Native;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeibuAts {
    internal class SeibuATS {
        public static INative Native;
        public static SpeedLimit ATSPattern = new SpeedLimit(), SignalPattern = new SpeedLimit(), StopPattern = new SpeedLimit(), LimitPattern = new SpeedLimit();
        public static IAtsPanelValue<bool> ATS_Power, ATS_EB, ATS_Limit, ATS_Stop, ATS_Confirm;
        public static IAtsSound ATS_StopAnnounce, ATS_EBAnnounce;
        private static bool Confirm = false, MaxOver95 = false;
        private static double PointLocation = 0, SignalLocation = 0;
        private static int EBType = 0, PointType = 0, PatternType = 0; //4 -> G2,3 -> G1/YG,2 -> Y,1 -> YY,0 -> R
        public static int BrakeCommand = 0;

        public static void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            ATSPattern = new SpeedLimit();
            SignalPattern = new SpeedLimit();
            StopPattern = new SpeedLimit();
            LimitPattern = new SpeedLimit();

            Confirm = MaxOver95 = false;
            PointLocation = 0;
            BrakeCommand = 0;
            EBType = 0;
            PointType = 0;
            SignalLocation = 0;
            PatternType = 0;
        }

        public static void Load() {
            ATS_Power = Native.AtsPanelValues.RegisterBoolean(46); 
            ATS_EB = Native.AtsPanelValues.RegisterBoolean(47); 
            ATS_Limit = Native.AtsPanelValues.RegisterBoolean(49); 
            ATS_Stop = Native.AtsPanelValues.RegisterBoolean(253); 
            ATS_Confirm = Native.AtsPanelValues.RegisterBoolean(48);

            ATS_StopAnnounce = Native.AtsSounds.Register(8);
            ATS_EBAnnounce = Native.AtsSounds.Register(9);

            ATSPattern = new SpeedLimit();
            SignalPattern = new SpeedLimit();
            StopPattern = new SpeedLimit(); 
            LimitPattern = new SpeedLimit();

            Confirm = MaxOver95 = false;
            PointLocation = 0;
            BrakeCommand = 0;
            EBType = 0;
            PointType = 0;
            SignalLocation = 0;
            PatternType = 0;
        }

        public static void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            if (StopPattern != SpeedLimit.inf) StopPattern = SpeedLimit.inf;
        }

        public static void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {//1 2 5 8 20
                case 1:
                    PointUpdate(SeibuAts.state.Location, 1, e.Distance);
                    break;
                case 2:
                    PointUpdate(SeibuAts.state.Location, 2, e.Distance);
                    break;
                case 5:
                    if (StopPattern == SpeedLimit.inf) StopPattern = new SpeedLimit(0, SeibuAts.state.Location + 590);
                    break;
                case 8:
                    break;
                case 20:
                    MaxOver95 = e.Optional == 115;
                    break;
            }
        }

        public static void OnB1Pressed(object sender, EventArgs e) {


        }

        public static void Tick(double Location, double Speed, Section nextSection) {

            if (PointType == 1) {
                if (nextSection.CurrentSignalIndex == 0) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 0);
                } else if (nextSection.CurrentSignalIndex == 1) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 30);
                } else if (nextSection.CurrentSignalIndex == 2) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 65);
                } else if (nextSection.CurrentSignalIndex == 3) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 95);
                } else if (nextSection.CurrentSignalIndex == 4) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, MaxOver95 ? 115 : 95);
                }
            } else if (PointType == 2) {
                if (nextSection.CurrentSignalIndex == 0) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 0);
                } else if (nextSection.CurrentSignalIndex == 1) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 30);
                } else if (nextSection.CurrentSignalIndex == 2) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 65);
                } else if (nextSection.CurrentSignalIndex == 3) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, 95);
                } else if (nextSection.CurrentSignalIndex == 4) {
                    SignalPattern = new SpeedLimit(PointLocation + 200, MaxOver95 ? 115 : 95);
                }
            }

            if (Location > SignalLocation && PointType != 0) { 
                PointType = 0;
                if (SignalPattern.Limit == 0) PatternType = 0;
                else if (SignalPattern.Limit == 30) PatternType = 1;
                else if (SignalPattern.Limit == 65) PatternType = 2;
                else if (SignalPattern.Limit == 95) PatternType = 3;
                else if (SignalPattern.Limit == 115) PatternType = 4;
            }
        }

        private static void PointUpdate(double Location,int Type,double SigLocation) { // 1 ->B1 2 -> B2
            PointType = Type;
            PointLocation = Location;
            SignalLocation = SigLocation + Location;
            if (SignalPattern.Limit == 0) PatternType = 0;
            else if (SignalPattern.Limit == 30) PatternType = 1;
            else if (SignalPattern.Limit == 65) PatternType = 2;
            else if (SignalPattern.Limit == 95) PatternType = 3;
            else if (SignalPattern.Limit == 115) PatternType = 4;
        }

        public static void Dispose() {
            ATS_Power.Dispose();
            ATS_EB.Dispose();
            ATS_Limit.Dispose();
            ATS_Stop.Dispose();
            ATS_Confirm.Dispose();
        }
    }
}
