using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;

namespace ATC {
    [PluginType(PluginType.VehiclePlugin)]
    public class ATC : AssemblyPluginBase {
        private SectionManager sectionManager;
        public static AtsEx.PluginHost.Native.VehicleSpec vehicleSpec;
        public static AtsEx.PluginHost.Native.VehicleState state = new AtsEx.PluginHost.Native.VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
        public static AtsEx.PluginHost.Handles.HandleSet handles;
        public static IAtsSound Switchover;

        public static int SignalMode = 0, LastSignalMode = 0;//0:TSP 1:T-DATC

        public ATC(PluginBuilder services) : base(services) {
            SignalMode = LastSignalMode = 0;


            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.Started += Initialize;

            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed += OnB1Pressed;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            vehicleSpec = Native.VehicleSpec;
        }

        private void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {

        }

        private void OnB1Pressed(object sender, EventArgs e) {

        }

        private void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {

        }

        private void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {

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

            var CurrentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;
            var NextSection = sectionManager.Sections[pointer] as Section;


            if (SignalMode != LastSignalMode) Switchover.Play();
            LastSignalMode = SignalMode;

            NotchCommandBase powerCommand = handles.Power.GetCommandToSetNotchTo(handles.Power.Notch);
            NotchCommandBase brakeCommand = handles.Brake.GetCommandToSetNotchTo(handles.Brake.Notch);
            ReverserPositionCommandBase reverserCommand = ReverserPositionCommandBase.Continue;
            ConstantSpeedCommand? constantSpeedCommand = ConstantSpeedCommand.Continue;

            tickResult.HandleCommandSet = new HandleCommandSet(powerCommand, brakeCommand, reverserCommand, constantSpeedCommand);

            return tickResult;
        }

        public override void Dispose() {
            BveHacker.ScenarioCreated -= OnScenarioCreated;
            Native.BeaconPassed -= BeaconPassed;
            Native.DoorOpened -= DoorOpened;
            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed -= OnB1Pressed;

            BveHacker.ScenarioCreated -= OnScenarioCreated;
        }
    }
}
