using BveEx.Extensions.Native;
using BveEx.Extensions.PreTrainPatch;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class TobuSignal : AssemblyPluginBase {

        public override void Tick(TimeSpan elapsed) {
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
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
                    //T-DATC
                    if (T_DATC.ATCEnable) {
                        T_DATC.Tick(state, sectionManager, handles);
                        handles.BrakeNotch = Math.Max(handles.BrakeNotch, T_DATC.BrakeCommand);
                        if (T_DATC.BrakeCommand > 0) handles.PowerNotch = 0;
                    } else {
                        if (TSP_ATS.ATSEnable) {
                            TSP_ATS.Disable();
                            T_DATC.SwitchFromATS();
                            Sound_Switchover = AtsSoundControlInstruction.Play;
                        } else {
                            T_DATC.Init(state.Time);
                        }
                    }
                } else {
                    //TSP-ATS
                    if (TSP_ATS.ATSEnable) {
                        TSP_ATS.Tick(state);
                        handles.BrakeNotch = Math.Max(handles.BrakeNotch, TSP_ATS.BrakeCommand);
                        if (TSP_ATS.BrakeCommand > 0) handles.PowerNotch = 0;
                    } else {
                        if (T_DATC.ATCEnable) {
                            T_DATC.Disable();
                            TSP_ATS.SwitchFromATC();
                            Sound_Switchover = AtsSoundControlInstruction.Play;
                        } else {
                            TSP_ATS.Init(state.Time);
                        }
                    }
                }

                //panel
                panel[290] = Convert.ToInt32(T_DATC.ATC_01);
                panel[291] = Convert.ToInt32(T_DATC.ATC_10);
                panel[292] = Convert.ToInt32(T_DATC.ATC_15);
                panel[293] = Convert.ToInt32(T_DATC.ATC_20);
                panel[294] = Convert.ToInt32(T_DATC.ATC_25);
                panel[295] = Convert.ToInt32(T_DATC.ATC_30);
                panel[296] = Convert.ToInt32(T_DATC.ATC_35);
                panel[297] = Convert.ToInt32(T_DATC.ATC_40);
                panel[298] = Convert.ToInt32(T_DATC.ATC_45);
                panel[299] = Convert.ToInt32(T_DATC.ATC_50);
                panel[300] = Convert.ToInt32(T_DATC.ATC_55);
                panel[301] = Convert.ToInt32(T_DATC.ATC_60);
                panel[302] = Convert.ToInt32(T_DATC.ATC_65);
                panel[303] = Convert.ToInt32(T_DATC.ATC_70);
                panel[304] = Convert.ToInt32(T_DATC.ATC_75);
                panel[305] = Convert.ToInt32(T_DATC.ATC_80);
                panel[306] = Convert.ToInt32(T_DATC.ATC_85);
                panel[307] = Convert.ToInt32(T_DATC.ATC_90);
                panel[308] = Convert.ToInt32(T_DATC.ATC_95);
                panel[309] = Convert.ToInt32(T_DATC.ATC_100);
                panel[310] = Convert.ToInt32(T_DATC.ATC_110);

                panel[288] = Convert.ToInt32(T_DATC.ATC_Stop);
                panel[289] = Convert.ToInt32(T_DATC.ATC_Proceed);

                panel[315] = Convert.ToInt32(T_DATC.ATC_P);
                panel[287] = Convert.ToInt32(T_DATC.ATC_X);

                panel[316] = T_DATC.ORPNeedle;
                panel[313] = T_DATC.ATCNeedle;
                panel[312] = Convert.ToInt32(T_DATC.ATCNeedle_Disappear);
                panel[322] = T_DATC.ATC_EndPointDistance;
                panel[323] = T_DATC.ATC_SwitcherPosition;

                panel[317] = Convert.ToInt32(T_DATC.ATC_TobuATC);
                panel[318] = Convert.ToInt32(T_DATC.ATC_Depot);
                panel[320] = Convert.ToInt32(T_DATC.ATC_ServiceBrake);
                panel[319] = Convert.ToInt32(T_DATC.ATC_EmergencyBrake);
                panel[324] = Convert.ToInt32(T_DATC.ATC_EmergencyOperation);
                panel[254] = Convert.ToInt32(T_DATC.ATC_StationStop);
                panel[321] = Convert.ToInt32(T_DATC.ATC_PatternApproach);

                panel[325] = Convert.ToInt32(TSP_ATS.ATS_TobuAts);
                panel[328] = Convert.ToInt32(TSP_ATS.ATS_ATSEmergencyBrake);
                panel[330] = Convert.ToInt32(TSP_ATS.ATS_EmergencyOperation);
                panel[329] = Convert.ToInt32(TSP_ATS.ATS_Confirm);
                panel[326] = Convert.ToInt32(TSP_ATS.ATS_60);
                panel[327] = Convert.ToInt32(TSP_ATS.ATS_15);

                //sound
                //sound[0] = (int)T_DATC.ATC_WarningBell;
                //sound[1] = (int)T_DATC.ATC_WarningBell;
                sound[2] = (int)T_DATC.ATC_Ding;
                sound[116] = (int)T_DATC.ATC_PatternApproachBeep;
                sound[117] = (int)T_DATC.ATC_StationStopAnnounce;
                sound[118] = (int)Sound_Switchover;
                sound[119] = (int)T_DATC.ATC_EmergencyOperationAnnounce;

            } else {
                for (var i = 287; i <= 330; ++i) panel[i] = 0;
                panel[312] = 1;
                if (!SignalEnable && handles.ReverserPosition != ReverserPosition.N && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1
                    && Keyin && StandAloneMode)
                    SignalEnable = true;
                handles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                handles.ReverserPosition = ReverserPosition.N;



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
