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
           
        }

        private void DoorClosed(object sender, EventArgs e) {
            
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
         
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
           
        }

    }
}
