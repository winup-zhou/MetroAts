using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JR_SotetsuSignal {
    internal partial class ATS_SN {
        private static TimeSpan InitStartTime = TimeSpan.Zero, WarnStartTime = TimeSpan.Zero;
        private static bool EB = false, Warn = false;
        public static bool ATSEnable = false;
        public static int BrakeCommand = 0;

        //panel
        public static bool SN_Power, SN_Action;
        public static AtsSoundControlInstruction SN_WarningBell, SN_Chime;

        public static void Tick(VehicleState state) {
            if (ATSEnable) {
                if (state.Time.TotalMilliseconds - InitStartTime.TotalMilliseconds < 1000) {
                    SN_WarningBell = SN_Chime = AtsSoundControlInstruction.PlayLooping;
                    SN_Power = false;
                    SN_Action = true;
                } else {
                    if (Warn && state.Time.TotalMilliseconds - WarnStartTime.TotalMilliseconds > 5000)
                        EB = true;
                    if (Warn && !EB) {
                        SN_Power = false;
                        SN_Action = true;
                    } else if (EB) {
                        SN_Power = false;
                        SN_Action = state.Time.TotalMilliseconds % 750 < 375;
                    } else {
                        SN_Power = true;
                        SN_Action = false;
                    }
                    SN_WarningBell = EB || Warn ? AtsSoundControlInstruction.PlayLooping : AtsSoundControlInstruction.Stop;
                    BrakeCommand = EB ? JR_SotetsuSignal.vehicleSpec.BrakeNotches + 1 : 0;
                }
            } else {
                Disable();
            }
        }
    }
}
