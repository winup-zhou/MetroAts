using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeibuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class SeibuSignal : AssemblyPluginBase {
        public override void Tick(TimeSpan elapsed) {
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;

            int pointer = 0;
            while (sectionManager.Sections[pointer].Location < state.Location)
                pointer++;
            if (pointer >= sectionManager.Sections.Count)
                pointer = sectionManager.Sections.Count - 1;

            var currentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;

            if (SignalEnable) {
                if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {

                } else {
                    if (SeibuATS.ATSEnable) {
                        SeibuATS.Tick(state, sectionManager);
                        AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, SeibuATS.BrakeCommand);
                        if (SeibuATS.BrakeCommand > 0) AtsHandles.PowerNotch = 0;
                    } else {
                        SeibuATS.Init(state.Time);
                    }
                }
                panel[332] = Convert.ToInt32(SeibuATS.ATS_Power);
                panel[333] = Convert.ToInt32(SeibuATS.ATS_EB);
                panel[334] = Convert.ToInt32(SeibuATS.ATS_Stop);
                panel[335] = Convert.ToInt32(SeibuATS.ATS_Confirm);
                panel[336] = Convert.ToInt32(SeibuATS.ATS_Limit);
                //panel[337] = Convert.ToInt32(SeibuATS.ATS_Power);

                sound[8] = (int)SeibuATS.ATS_StopAnnounce;
                sound[9] = (int)SeibuATS.ATS_EBAnnounce;
            } else {
                for (var i = 332; i <= 337; ++i) panel[i] = 0;
                if (!SignalEnable && Keyin && StandAloneMode)
                    SignalEnable = true;
                AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                AtsHandles.ReverserPosition = ReverserPosition.N;
            }
            sound[10] = (int)Sound_Keyin;
            sound[11] = (int)Sound_Keyout;
            sound[24] = (int)Sound_ResetSW;

            //sound reset
            Sound_Keyin = Sound_Keyout = Sound_ResetSW = Sound_Switchover = AtsSoundControlInstruction.Continue;
            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }
    }
}
