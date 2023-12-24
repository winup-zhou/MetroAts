using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC {
    [PluginType(PluginType.VehiclePlugin)]
    public class ATC : AssemblyPluginBase {
        public static AtsEx.PluginHost.Native.VehicleSpec vehicleSpec;
        public static AtsEx.PluginHost.Native.VehicleState state = new AtsEx.PluginHost.Native.VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
        public static AtsEx.PluginHost.Handles.HandleSet handles;

        public ATC(PluginBuilder services) : base(services) {
            vehicleSpec = Native.VehicleSpec;
        }

        public override TickResult Tick(TimeSpan elapsed) {
            state = Native.VehicleState;
            handles = Native.Handles;
            VehiclePluginTickResult tickResult = new VehiclePluginTickResult();

            NotchCommandBase powerCommand = handles.Power.GetCommandToSetNotchTo(handles.Power.Notch);
            NotchCommandBase brakeCommand = handles.Brake.GetCommandToSetNotchTo(handles.Brake.Notch);
            ReverserPositionCommandBase reverserCommand = ReverserPositionCommandBase.Continue;
            ConstantSpeedCommand? constantSpeedCommand = ConstantSpeedCommand.Continue;

            tickResult.HandleCommandSet = new HandleCommandSet(powerCommand, brakeCommand, reverserCommand, constantSpeedCommand);

            return tickResult;
        }
        public override void Dispose() {

        }
    }
}
