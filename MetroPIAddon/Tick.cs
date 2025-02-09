using BveEx.Extensions.PreTrainPatch;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MetroPIAddon {
    public partial class MetroPIAddon : AssemblyPluginBase {
        public override void Tick(TimeSpan elapsed) {
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;
            if (isStopAnnounce) {
                if (StopAnnounce == AtsSoundControlInstruction.Stop) {
                    StopAnnounce = AtsSoundControlInstruction.PlayLooping;
                }
                if (handles.BrakeNotch > 0 && StopAnnounce != AtsSoundControlInstruction.Stop) {
                    StopAnnounce = AtsSoundControlInstruction.Stop;
                    StopAnnounce_Confirmed = AtsSoundControlInstruction.PlayLooping;
                }
            } else {
                StopAnnounce = StopAnnounce_Confirmed = AtsSoundControlInstruction.Stop;
            }
        }
    }
}
