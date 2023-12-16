using AtsEx.PluginHost;
using AtsEx.PluginHost.Panels.Native;
using BveTypes.ClassWrappers;
using System;


namespace TobuAts_EX {
    internal class TSP_ATS {
        public static INative Native;
        //InternalValue -> ATS
        private static SpeedLimit ATSPattern = new SpeedLimit(), MPPPattern = new SpeedLimit(), SignalPattern = new SpeedLimit();
        private static double LastBeaconPassTime = 0, MPPEndLocation = 0;
        private static bool ConfirmOperation = false;
        private static int MPPCount_TJ = 0, MPPCount_TS = 0;
        public static int BrakeCommand = 0, EBType = 0; //0:no EB 1:EB until stop 2:EB can release

        //panel -> ATS
        private static IAtsPanelValue<bool> ATS_TobuATS, ATS_ATSEmergencyBrake, ATS_EmergencyOperation, ATS_Confirm, ATS_60, ATS_15;

        public static void Load() {
            ATSPattern = new SpeedLimit();
            MPPPattern = new SpeedLimit();
            SignalPattern = new SpeedLimit();
            LastBeaconPassTime = 0;
            ConfirmOperation = false;
            MPPEndLocation = 0;
            MPPCount_TJ = 0;
            MPPCount_TS = 0;
            BrakeCommand = 0;
            EBType = 0;

            ATS_TobuATS = Native.AtsPanelValues.RegisterBoolean(41);
            ATS_ATSEmergencyBrake = Native.AtsPanelValues.RegisterBoolean(44);
            //ATS_EmergencyOperation = Native.AtsPanelValues.RegisterBoolean(512);
            //ATS_Confirm = Native.AtsPanelValues.RegisterBoolean(512);
            ATS_60 = Native.AtsPanelValues.RegisterBoolean(43);
            ATS_15 = Native.AtsPanelValues.RegisterBoolean(42);
        }

        public static void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            MPPCount_TJ = MPPCount_TS = 0;
        }

        public static void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {//0 1 2 3 5 9 15
                case 0:
                    if (e.SignalIndex == 0) {
                        SignalPattern = new SpeedLimit(15, TobuAts.state.Location + e.Distance);
                        EBType = 1;
                    } else if (e.SignalIndex == 4) SignalPattern  = new SpeedLimit(Config.MaxSpeed, TobuAts.state.Location);
                    break;
                case 1:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedLimit(15, TobuAts.state.Location + 180);
                    else if (e.SignalIndex < 4 && e.SignalIndex > 0) SignalPattern = new SpeedLimit(60, TobuAts.state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedLimit(Config.MaxSpeed, TobuAts.state.Location);
                    break;
                case 2:
                    if (e.SignalIndex < 4) SignalPattern = new SpeedLimit(60, TobuAts.state.Location + 180);
                    else if(e.SignalIndex == 4) SignalPattern  = new SpeedLimit(Config.MaxSpeed, TobuAts.state.Location);
                    break;
                case 3:
                    if (TobuAts.state.Time.TotalMilliseconds - LastBeaconPassTime < 1000) EBType = 1;
                    LastBeaconPassTime = TobuAts.state.Time.TotalMilliseconds;
                    break;
                case 5:
                    if (MPPCount_TS == 0)
                        MPPPattern = new SpeedLimit(60, TobuAts.state.Location + 400);
                    else if (MPPCount_TS == 1) {
                        MPPPattern = new SpeedLimit(15, TobuAts.state.Location + 100);
                        MPPEndLocation = TobuAts.state.Location + 105;
                    }
                    MPPCount_TS++;
                    break;
                case 9:
                    if (MPPCount_TJ == 0) 
                        MPPPattern = new SpeedLimit(60, TobuAts.state.Location + 237);
                    else if (MPPCount_TJ == 1) {
                        MPPPattern = new SpeedLimit(15, TobuAts.state.Location + 111);
                        MPPEndLocation = TobuAts.state.Location + 116;
                    }
                    MPPCount_TJ++;
                    break;
                case 15:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedLimit(15, TobuAts.state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedLimit(Config.MaxSpeed, TobuAts.state.Location);
                    break;
            }
        }

        public static void OnB1Pressed(object sender, EventArgs e) {
            if (EBType == 1 && TobuAts.handles.Brake.Notch == TobuAts.vehicleSpec.BrakeNotches + 1) {
                if (TobuAts.state.Speed == 0) EBType = 0;
            }
        }

        public static void Tick(double Location, double Speed, Section nextSection) {
            if (Location > MPPEndLocation) {
                MPPPattern = new SpeedLimit();
                MPPCount_TJ = MPPCount_TS = 0;
            }

            ATS_TobuATS.Value = TobuAts.SignalMode == 0;

            ATSPattern = (nextSection.CurrentSignalIndex > 9 && nextSection.CurrentSignalIndex < 49) ? new SpeedLimit(60, nextSection.Location)
                : (SignalPattern.AtLocation(Location, -3.5) < MPPPattern.AtLocation(Location, -3.5) ? SignalPattern : MPPPattern);

            if (SignalPattern.AtLocation(Location, -3.5) < MPPPattern.AtLocation(Location, -3.5)) {
                if (Speed > ATSPattern.AtLocation(Location, -3.5)) EBType = 2;
            } else {
                if (Speed > ATSPattern.AtLocation(Location, -3.5)) EBType = 1;
            }

            if(EBType == 2) {
                if (Speed < ATSPattern.Limit) EBType = 0;
            }

            ATS_60.Value = ATSPattern.Limit == 60;
            ATS_15.Value = ATSPattern.Limit == 15;

            ATS_ATSEmergencyBrake.Value = EBType > 0;

            BrakeCommand = EBType > 0 ? TobuAts.vehicleSpec.BrakeNotches + 1 : 0;
        }

        public static void Dispose() {
            ATS_TobuATS.Dispose();
            ATS_ATSEmergencyBrake.Dispose();
            //ATS_EmergencyOperation.Dispose();
            //ATS_Confirm.Dispose();
            ATS_60.Dispose();
            ATS_15.Dispose();
        }
    }
}
