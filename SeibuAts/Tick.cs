using AtsEx.PluginHost;
using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using SlimDX.XInput;
using System;

namespace SeibuAts
{
    [PluginType(PluginType.VehiclePlugin)]
    public class SeibuAts : AssemblyPluginBase {
        public static AtsEx.PluginHost.Native.VehicleSpec vehicleSpec;
        public static AtsEx.PluginHost.Native.VehicleState state = new AtsEx.PluginHost.Native.VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
        public static AtsEx.PluginHost.Handles.HandleSet handles;
        private static SectionManager sectionManager;

        public SeibuAts(PluginBuilder services) : base(services) {
            vehicleSpec = Native.VehicleSpec;

            BveHacker.ScenarioCreated += OnScenarioCreated;
        }
        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }

        public override TickResult Tick(TimeSpan elapsed) {
            state = Native.VehicleState;
            handles = Native.Handles;
            VehiclePluginTickResult tickResult = new VehiclePluginTickResult();

            int pointer = 0;
            while (sectionManager.Sections[pointer].Location < state.Location) pointer++;
            if (pointer >= sectionManager.Sections.Count) pointer = sectionManager.Sections.Count - 1;

            var NextSection = sectionManager.Sections[pointer] as Section;

            SeibuATS.Tick(state.Location, state.Speed, NextSection);

            NotchCommandBase powerCommand = handles.Power.GetCommandToSetNotchTo(handles.Power.Notch);
            NotchCommandBase brakeCommand = handles.Brake.GetCommandToSetNotchTo(handles.Brake.Notch);
            ReverserPositionCommandBase reverserCommand = ReverserPositionCommandBase.Continue;
            ConstantSpeedCommand? constantSpeedCommand = ConstantSpeedCommand.Continue;

            tickResult.HandleCommandSet = new HandleCommandSet(powerCommand, brakeCommand, reverserCommand, constantSpeedCommand);

            return tickResult;
        }
        public override void Dispose() {
            BveHacker.ScenarioCreated -= OnScenarioCreated;
        }

    }
}
