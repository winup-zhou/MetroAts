using BveEx.Extensions.Native.Input;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    internal partial class D_ATS_P {
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
                case 22:
                    ValidDataFromBeacon = 2;
                    break;
                case 5:
                    MaxSpeed = e.Optional;
                    break;
                case 4:
                    if (ATSEnable) {
                        if (e.Optional == -1) {
                            LimitPattern = SpeedPattern.inf;
                        } else {
                            LimitPattern = new SpeedPattern(e.Optional % 1000, e.Optional / 1000 + state.Location, MaxSpeed);
                        }
                    }
                    break;

            }
        }

        public static void ConfirmEB(VehicleState state, HandleSet handles) {

        }

        public static void Disable() {

        }
    }
}
