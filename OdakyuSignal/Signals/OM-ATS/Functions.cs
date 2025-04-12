using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    internal partial class OM_ATS {
        public static void Init(TimeSpan time) {
            InitStartTime = time;
            ATSEnable = true;
        }

        public static void SwitchOver() {
            ATSEnable = true;
        }

        public static void ResetAll() {

        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {

            }
        }

        public static void ConfirmEB(VehicleState state, HandleSet handles) {

        }

        public static void Disable() {

        }
    }
}
