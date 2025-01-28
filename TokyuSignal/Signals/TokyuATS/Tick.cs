using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokyuSignal {
    internal partial class TokyuATS {
        private static TimeSpan InitializeStartTime = TimeSpan.Zero, LoopRPassTime = TimeSpan.Zero, LoopYYPassTime = TimeSpan.Zero,
            LoopYPassTime = TimeSpan.Zero, LoopYGPassTime = TimeSpan.Zero, LoopLimitPassTime = TimeSpan.Zero, WarnStartTime = TimeSpan.Zero;
        private static bool EB = false, Warn = false;

        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;
        public static bool ATS_TokyuATS, ATS_EB, ATS_WarnNormal, ATS_WarnTriggered;
        public static AtsSoundControlInstruction ATS_EBBell, ATS_WarnBell;

        public static void Tick(VehicleState state) {
            if (ATSEnable) {
                ATS_TokyuATS = true;
                if (state.Time.TotalMilliseconds - WarnStartTime.TotalMilliseconds > 2000 && WarnStartTime != TimeSpan.Zero)
                    Warn = true;
                ATS_WarnNormal = !Warn;
                ATS_WarnTriggered = Warn;
                ATS_EB = EB;
                BrakeCommand = EB ? TokyuSignal.vehicleSpec.BrakeNotches + 1 : 0;
                ATS_EBBell = EB ? AtsSoundControlInstruction.PlayLooping : AtsSoundControlInstruction.Stop;
                ATS_WarnBell = Warn ? AtsSoundControlInstruction.PlayLooping : AtsSoundControlInstruction.Stop;
            } else {
                Disable();
            }
        }
    }
}
