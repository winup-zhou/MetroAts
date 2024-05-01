using AtsEx.Extensions.PreTrainPatch;
using AtsEx.PluginHost;
using AtsEx.PluginHost.Panels.Native;
using BveTypes.ClassWrappers;
using System;


namespace MetroAts {
    internal class TSP_ATS {
        public static INative Native;
        //InternalValue -> ATS
        private static SpeedLimit ATSPattern = new SpeedLimit(), MPPPattern = new SpeedLimit(), SignalPattern = new SpeedLimit();
        private static double LastBeaconPassTime = 0, MPPEndLocation = 0, InitializeStartTime = 0;
        private static bool ConfirmOperation = false, IsDoorOpened = false;
        public static int BrakeCommand = 0, EBType = 0; //0:no EB 1:EB until stop 2:EB can release
        public static bool ATSEnable = false;

        //panel -> ATS
        public static bool ATS_TobuAts, ATS_ATSEmergencyBrake, ATS_EmergencyOperation, ATS_Confirm, ATS_60, ATS_15;

        public static void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            ATSPattern = new SpeedLimit();
            MPPPattern = new SpeedLimit();
            SignalPattern = new SpeedLimit();
            LastBeaconPassTime = 0;
            ConfirmOperation = false;
            MPPEndLocation = 0;
            IsDoorOpened = false;
            BrakeCommand = 0;
            EBType = 0;
            InitializeStartTime = 0;

            ATS_TobuAts = false;
            ATS_ATSEmergencyBrake = false;
            //ATS_EmergencyOperation = false;
            //ATS_Confirm = false;
            ATS_60 = false;
            ATS_15 = false;

            ATSEnable = false;
        }

        public static void Enable(double Time) {
            ATSEnable = true;
            InitializeStartTime = Time;
        }

        public static void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            IsDoorOpened = true;
        }

        public static void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            switch (e.Type) {//0 1 2 3 5 9 15
                case 0:
                    if (e.SignalIndex == 0) {
                        SignalPattern = new SpeedLimit(15, MetroAts.state.Location + e.Distance);
                        EBType = 1;
                    } else if (e.SignalIndex == 4) SignalPattern = new SpeedLimit(Config.TobuMaxSpeed, MetroAts.state.Location);
                    break;
                case 1:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedLimit(15, MetroAts.state.Location + 180);
                    else if (e.SignalIndex < 4 && e.SignalIndex > 0) SignalPattern = new SpeedLimit(60, MetroAts.state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedLimit(Config.TobuMaxSpeed, MetroAts.state.Location);
                    break;
                case 2:
                    if (e.SignalIndex < 4) SignalPattern = new SpeedLimit(60, MetroAts.state.Location + 180);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedLimit(Config.TobuMaxSpeed, MetroAts.state.Location);
                    break;
                case 3:
                    if (MetroAts.state.Time.TotalMilliseconds - LastBeaconPassTime < 1000) EBType = 1;
                    LastBeaconPassTime = MetroAts.state.Time.TotalMilliseconds;
                    break;
                case 5:
                    if (MPPPattern == SpeedLimit.inf)
                        MPPPattern = new SpeedLimit(60, MetroAts.state.Location + 400);
                    else if (MPPPattern.Limit == 60) {
                        MPPPattern = new SpeedLimit(15, MetroAts.state.Location + 100);
                        MPPEndLocation = MetroAts.state.Location + 105;
                    }
                    break;
                case 9:
                    if (MPPPattern == SpeedLimit.inf)
                        MPPPattern = new SpeedLimit(60, MetroAts.state.Location + 237);
                    else if (MPPPattern.Limit == 60) {
                        MPPPattern = new SpeedLimit(15, MetroAts.state.Location + 111);
                        MPPEndLocation = MetroAts.state.Location + 116;
                    }
                    break;
                case 15:
                    if (e.SignalIndex == 0) SignalPattern = new SpeedLimit(15, MetroAts.state.Location + e.Distance);
                    else if (e.SignalIndex == 4) SignalPattern = new SpeedLimit(Config.TobuMaxSpeed, MetroAts.state.Location);
                    break;
            }
        }

        public static void OnB1Pressed(object sender, EventArgs e) {
            if (EBType == 1 && MetroAts.handles.Brake.Notch == MetroAts.vehicleSpec.BrakeNotches + 1) {
                if (MetroAts.state.Speed == 0) EBType = 0;
            }
        }

        public static void Tick(double Location, double Speed, double Time, Section nextSection) {
            if (ATSEnable) {
                if (Time - InitializeStartTime < 3000) {
                    ATS_ATSEmergencyBrake = true;
                    BrakeCommand = MetroAts.vehicleSpec.BrakeNotches + 1;
                } else {
                    if (Location > MPPEndLocation && IsDoorOpened) {
                        MPPPattern = SpeedLimit.inf;
                        IsDoorOpened = false;
                    }

                    ATS_TobuAts = true;

                    ATSPattern = (nextSection.CurrentSignalIndex > 9 && nextSection.CurrentSignalIndex < 49) ? new SpeedLimit(60, nextSection.Location)
                        : (SignalPattern.AtLocation(Location, -3.5) < MPPPattern.AtLocation(Location, -3.5) ? SignalPattern : MPPPattern);

                    if (SignalPattern.AtLocation(Location, -3.5) < MPPPattern.AtLocation(Location, -3.5)) {
                        if (Speed > ATSPattern.AtLocation(Location, -3.5)) EBType = 2;
                    } else {
                        if (Speed > ATSPattern.AtLocation(Location, -3.5)) EBType = 1;
                    }

                    if (EBType == 2) {
                        if (Speed < ATSPattern.Limit) EBType = 0;
                    }

                    ATS_60 = ATSPattern.Limit == 60;
                    ATS_15 = ATSPattern.Limit == 15;

                    ATS_ATSEmergencyBrake = EBType > 0;

                    BrakeCommand = EBType > 0 ? MetroAts.vehicleSpec.BrakeNotches + 1 : 0;

                }
            } else {
                ATS_TobuAts = false;
                ATS_ATSEmergencyBrake = false;
                //ATS_EmergencyOperation = false;
                //ATS_Confirm = false;
                ATS_60 = false;
                ATS_15 = false;

                ATSEnable = false;
            }
        }

        public static void Disable() {
            ATSEnable = false;

            ATS_TobuAts = false;
            ATS_ATSEmergencyBrake = false;
            //ATS_EmergencyOperation = false;
            //ATS_Confirm = false;
            ATS_60 = false;
            ATS_15 = false;
        }
    }
}
