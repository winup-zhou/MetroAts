using BveEx.Extensions.Native.Input;
using BveEx.Extensions.Native;
using BveEx.PluginHost;
using BveEx.PluginHost.Input;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConductorlessAddon {
    public partial class ConductorlessAddon : AssemblyPluginBase {

        private void Initialize(object sender, StartedEventArgs e) {

        }

        private void DoorOpened(object sender, EventArgs e) {
            isDoorOpen = true;
        }

        private void DoorClosed(object sender, EventArgs e) {
            isDoorOpen = false;
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
         
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
           
        }

        private void SetBeaconData(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            switch (e.Type) {
                case 42:
                    switch (e.Optional / 10) {
                        default: LineDef = KeyPosList.None; break;
                        case 1: LineDef = KeyPosList.Metro; break;
                        case 2: LineDef = KeyPosList.Tobu; break;
                        case 3: LineDef = KeyPosList.Tokyu; break;
                        case 4: LineDef = KeyPosList.Seibu; break;
                        case 5: LineDef = KeyPosList.Sotetsu; break;
                        case 6: LineDef = KeyPosList.JR; break;
                        case 7: LineDef = KeyPosList.Odakyu; break;
                        case 8: LineDef = KeyPosList.ToyoKosoku; break;
                    }
                    Direction = e.Optional % 10;
                    break;
            }
        }

    }
}
