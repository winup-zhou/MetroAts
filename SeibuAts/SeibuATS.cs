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
        private static double B1Point = 0, B2Point = 0;
        private static int EBType = 0;
        public static int BrakeCommand = 0;

        public static void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            ATSPattern = new SpeedLimit();
            SignalPattern = new SpeedLimit();
            StopPattern = new SpeedLimit();
            LimitPattern = new SpeedLimit();

            Confirm = MaxOver95 = false;
            B1Point = B2Point = 0;
            BrakeCommand = 0;
            EBType = 0;
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
            B1Point = B2Point = 0;
            BrakeCommand = 0;
            EBType = 0;
        }

        public static void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            if (StopPattern != SpeedLimit.inf) StopPattern = SpeedLimit.inf;
        }

        public static void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {//1 2 5 8 20
                case 1:
                    B1Point = SeibuAts.state.Location;
                    break;
                case 2:
                    B2Point = SeibuAts.state.Location;
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

        }

        public static void Dispose() {

        }
    }
}
