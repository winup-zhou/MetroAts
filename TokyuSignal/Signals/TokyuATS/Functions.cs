using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace TokyuSignal {
    internal partial class TokyuATS {
        public static void ResetAll() {
            BrakeCommand = TokyuSignal.vehicleSpec.BrakeNotches + 1;
            ATSEnable = false;
            InitializeStartTime = TimeSpan.Zero;
            LoopRPassTime = TimeSpan.Zero; 
            LoopYYPassTime = TimeSpan.Zero;
            LoopYPassTime = TimeSpan.Zero; 
            LoopYGPassTime = TimeSpan.Zero; 
            LoopLimitPassTime = TimeSpan.Zero;
            WarnStartTime = TimeSpan.Zero;

            EB = false;
            Warn = true;

            ATS_EBBell = AtsSoundControlInstruction.Stop;
            ATS_WarnBell = AtsSoundControlInstruction.Stop;
            ATS_TokyuATS = false; 
            ATS_EB = false;
            ATS_WarnNormal = false;
            ATS_WarnTriggered = false;

        }

        public static void Init(TimeSpan time) {
            ATSEnable = true;
            InitializeStartTime = time;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 0:
                    if (e.SignalIndex == 0) {
                        var lastLoopRPassTime = LoopRPassTime;
                        LoopRPassTime = state.Time;
                        if (LoopRPassTime.TotalMilliseconds - lastLoopRPassTime.TotalMilliseconds < 959) EB = true;
                    }
                    break;
                case 2:
                    if (e.SignalIndex == 1) LoopYYPassTime = state.Time;
                    break;
                case 3:
                    if (e.SignalIndex == 2) LoopYPassTime = state.Time;
                    break;
                case 4:
                    if (e.SignalIndex == 3) LoopYGPassTime = state.Time;
                    break;
                case 5:
                    if (e.SignalIndex == 1) {
                        if (state.Time.TotalMilliseconds - LoopYYPassTime.TotalMilliseconds < 1008) EB = true;
                    } else if (e.SignalIndex == 2) {
                        if (state.Time.TotalMilliseconds - LoopYPassTime.TotalMilliseconds < 1040) EB = true;
                    } else if (e.SignalIndex == 3) {
                        if (state.Time.TotalMilliseconds - LoopYGPassTime.TotalMilliseconds < 1019) EB = true;
                    }
                    break;
                case 8:
                    var lastLoopLimitPassTime = LoopLimitPassTime;
                    LoopLimitPassTime = state.Time;
                    if (LoopLimitPassTime.TotalMilliseconds - lastLoopLimitPassTime.TotalMilliseconds < 1000) EB = true;
                    break;
            }
        }

        public static void ResetBrake(VehicleState state, HandleSet handles) {
            if (Math.Abs(state.Speed) == 0 && handles.BrakeNotch == TokyuSignal.vehicleSpec.BrakeNotches + 1) {
                if(EB)EB = false;
            }
        }

        public static void ResetWarn() {
            if (Warn) { 
                Warn = false;
                WarnStartTime = TimeSpan.Zero;
            }
        }

        public static void SignalUpdated(VehicleState state, SignalUpdatedEventArgs e) {
            if (e.SignalIndex < 3)
                WarnStartTime = state.Time;
        }

        public static void Disable() {
            ATSEnable = false;
            BrakeCommand = TokyuSignal.vehicleSpec.BrakeNotches + 1;
            ATS_EBBell = AtsSoundControlInstruction.Stop;
            ATS_WarnBell = AtsSoundControlInstruction.Stop;
            ATS_TokyuATS = false;
            ATS_EB = false;
            ATS_WarnNormal = false;
            ATS_WarnTriggered = false;
        }
    }
}
