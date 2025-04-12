using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    internal partial class OM_ATS {
        private static bool SpeedCaution = false;
        private static double EBTriggerSpeed = Config.LessInf;

        public static bool ATS_Power, ATS_Triggered, ATS_SpeedCaution, ATS_EmergencyOperation;
        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;

        public static void Tick(VehicleState state) {

        }
    }
}
