using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    internal partial class WS_ATC {
        public static void ResetAll() {
            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
            ATCEnable = false;
            InitializeStartTime = TimeSpan.Zero;
        }

        public static void Init(TimeSpan time) {
            ATCEnable = true;
            InitializeStartTime = time;
        }

        public static void DisableAll() {
            ATCEnable = false;

            BrakeCommand = MetroSignal.vehicleSpec.BrakeNotches + 1;
        }

        private static void Disable_Noset_inDepot() {

        }
    }
}
