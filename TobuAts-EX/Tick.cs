using AtsEx.PluginHost;
using AtsEx.PluginHost.Handles;
using AtsEx.PluginHost.Input.Native;
using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Sound.Native;
using BveTypes.ClassWrappers;
using System;


namespace TobuAts_EX
{
    [PluginType(PluginType.VehiclePlugin)]
    public partial class TobuAts : AssemblyPluginBase {
        private SectionManager sectionManager;
        public static AtsEx.PluginHost.Native.VehicleSpec vehicleSpec;
        public static AtsEx.PluginHost.Native.VehicleState state = new AtsEx.PluginHost.Native.VehicleState(0,0,TimeSpan.Zero,0,0,0,0,0,0);
        public static AtsEx.PluginHost.Handles.HandleSet handles;
        public static IAtsSound Switchover;

        public static int SignalMode = 0, LastSignalMode = 0;//0:TSP 1:T-DATC

        public TobuAts(PluginBuilder services) : base(services) {
            SignalMode = LastSignalMode = 0;

            TSP_ATS.Native = Native;
            TSP_ATS.Load();
            T_DATC.Native = Native;
            T_DATC.Load();

            Switchover = Native.AtsSounds.Register(118);

            Native.BeaconPassed += BeaconPassed;
            Native.DoorOpened += DoorOpened;
            Native.Started += Initialize;

            Native.NativeKeys.AtsKeys[NativeAtsKeyName.B1].Pressed += OnB1Pressed;

            BveHacker.ScenarioCreated += OnScenarioCreated;

            vehicleSpec = Native.VehicleSpec;
        }

        private void Initialize(AtsEx.PluginHost.Native.StartedEventArgs e) {
            T_DATC.Initialize(e);
            TSP_ATS.Initialize(e);
        }

        private void OnB1Pressed(object sender, EventArgs e) {
            TSP_ATS.OnB1Pressed(sender, e);
        }

        private void DoorOpened(AtsEx.PluginHost.Native.DoorEventArgs e) {
            T_DATC.DoorOpened(e);
            TSP_ATS.DoorOpened(e);
        }

        private void BeaconPassed(AtsEx.PluginHost.Native.BeaconPassedEventArgs e) {
            T_DATC.BeaconPassed(e);
            TSP_ATS.BeaconPassed(e);
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

            if (CurrentSection.CurrentSignalIndex > 9 && CurrentSection.CurrentSignalIndex < 49) {
                SignalMode = 1;
                T_DATC.Tick(state.Location, state.Speed, sectionManager);
            } else {
                SignalMode = 0;
                T_DATC.ATCNeedle.Value = 0;
                T_DATC.ATCNeedle_Disappear.Value = 1;
                TSP_ATS.Tick(state.Location, state.Speed, NextSection);
            }

            if (SignalMode != LastSignalMode) Switchover.Play();
            LastSignalMode = SignalMode;

            NotchCommandBase powerCommand = handles.Power.GetCommandToSetNotchTo(handles.Power.Notch);
            NotchCommandBase brakeCommand = handles.Brake.GetCommandToSetNotchTo(Math.Max(SignalMode == 0 ? TSP_ATS.BrakeCommand : T_DATC.BrakeCommand, handles.Brake.Notch));
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

            T_DATC.Dispose();
            TSP_ATS.Dispose();
        }
    }
}
