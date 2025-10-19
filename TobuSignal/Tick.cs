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
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;

            int pointer = 0;
            while (sectionManager.Sections[pointer].Location < state.Location) {
                pointer++;
                if (pointer >= sectionManager.Sections.Count) {
                    pointer = sectionManager.Sections.Count - 1;
                    break;
                }
            }

            var currentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;

            if (SignalEnable) {
                if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;
                if (currentSection.CurrentSignalIndex > 4 && Config.EnableATC) {
                    //T-DATC
                    if (T_DATC.ATCEnable) {
                        T_DATC.Tick(state, sectionManager, handles);
                        if (T_DATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, T_DATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = T_DATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
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
                        if (TSP_ATS.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, TSP_ATS.BrakeCommand);
                            else AtsHandles.BrakeNotch = TSP_ATS.BrakeCommand;
                            BrakeTriggered = true;
                        }
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
                if (currentSection.CurrentSignalIndex >= 109 && currentSection.CurrentSignalIndex != 134 && currentSection.CurrentSignalIndex < 149 && !StandAloneMode)
                    sound[256] = corePlugin.SignalSWPos == MetroAts.SignalSWList.Tobu ?
                        (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                if (!StandAloneMode) {
                    if (corePlugin.KeyPos != MetroAts.KeyPosList.Tobu || corePlugin.SignalSWPos != MetroAts.SignalSWList.Tobu) {
                        BrakeTriggered = false;
                        SignalEnable = false;
                        T_DATC.ResetAll();
                        TSP_ATS.ResetAll();
                        sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }          
                }
                if (BrakeTriggered) {
                    AtsHandles.PowerNotch = 0;
                    if (handles.PowerNotch == 0) BrakeTriggered = false;
                }
                UpdatePanelAndSound(panel, sound);
                panel[Config.Panel_poweroutput] = AtsHandles.PowerNotch;
                panel[Config.Panel_brakeoutput] = AtsHandles.BrakeNotch;
            } else {
                if (!StandAloneMode) {
                    Keyin = corePlugin.KeyPos == MetroAts.KeyPosList.Tobu;
                    if (!SignalEnable && Keyin && (corePlugin.SignalSWPos == MetroAts.SignalSWList.Tobu) && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1)
                        SignalEnable = true;
                } else {
                    if (!SignalEnable && Keyin && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1)
                        SignalEnable = true;
                    AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                    AtsHandles.ReverserPosition = ReverserPosition.N;
                }
            }

            if (StandAloneMode) {
                var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
                leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
                leverText.Text = $"キー:{(Keyin ? "入" : "切")} \n{description}";
                if (isDoorOpen) AtsHandles.ReverserPosition = ReverserPosition.N;
                sound[270] = (int)Sound_Keyin;
                sound[271] = (int)Sound_Keyout;
                panel[Config.Panel_keyoutput] = Convert.ToInt32(Keyin);
            }
            

            //sound reset
            Sound_Keyin = Sound_Keyout = Sound_ResetSW = Sound_Switchover = AtsSoundControlInstruction.Continue;

            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }

        private static void UpdatePanelAndSound(IList<int> panel,IList<int> sound) {
            sound[273] = (int)Sound_ResetSW;

            //panel
            panel[287] = Convert.ToInt32(T_DATC.ATC_01);
            panel[288] = Convert.ToInt32(T_DATC.ATC_10);
            panel[289] = Convert.ToInt32(T_DATC.ATC_15);
            panel[290] = Convert.ToInt32(T_DATC.ATC_20);
            panel[291] = Convert.ToInt32(T_DATC.ATC_25);
            panel[292] = Convert.ToInt32(T_DATC.ATC_30);
            panel[293] = Convert.ToInt32(T_DATC.ATC_35);
            panel[294] = Convert.ToInt32(T_DATC.ATC_40);
            panel[295] = Convert.ToInt32(T_DATC.ATC_45);
            panel[296] = Convert.ToInt32(T_DATC.ATC_50);
            panel[297] = Convert.ToInt32(T_DATC.ATC_55);
            panel[298] = Convert.ToInt32(T_DATC.ATC_60);
            panel[299] = Convert.ToInt32(T_DATC.ATC_65);
            panel[300] = Convert.ToInt32(T_DATC.ATC_70);
            panel[301] = Convert.ToInt32(T_DATC.ATC_75);
            panel[302] = Convert.ToInt32(T_DATC.ATC_80);
            panel[303] = Convert.ToInt32(T_DATC.ATC_85);
            panel[304] = Convert.ToInt32(T_DATC.ATC_90);
            panel[305] = Convert.ToInt32(T_DATC.ATC_95);
            panel[306] = Convert.ToInt32(T_DATC.ATC_100);
            panel[307] = Convert.ToInt32(T_DATC.ATC_105);
            panel[308] = Convert.ToInt32(T_DATC.ATC_110);

            if (!Config.SeparateATCGRlamp) {
                panel[285] = Convert.ToInt32(T_DATC.ATC_Stop);
                panel[286] = Convert.ToInt32(T_DATC.ATC_Proceed);
            } else {
                panel[316] = Convert.ToInt32(T_DATC.ATC_Stop);
                panel[317] = Convert.ToInt32(T_DATC.ATC_Proceed);
            }
            

            panel[313] = Convert.ToInt32(T_DATC.ATC_P);
            panel[284] = Convert.ToInt32(T_DATC.ATC_X);

            panel[314] = T_DATC.ORPNeedle;
            panel[311] = T_DATC.ATCNeedle;
            panel[310] = Convert.ToInt32(T_DATC.ATCNeedle_Disappear);
            panel[323] = T_DATC.ATC_EndPointDistance;
            panel[324] = T_DATC.ATC_SwitcherPosition;

            panel[318] = Convert.ToInt32(T_DATC.ATC_TobuATC);
            panel[319] = Convert.ToInt32(T_DATC.ATC_Depot);
            panel[321] = Convert.ToInt32(T_DATC.ATC_ServiceBrake);
            panel[320] = Convert.ToInt32(T_DATC.ATC_EmergencyBrake);
            panel[326] = Convert.ToInt32(T_DATC.ATC_EmergencyOperation);
            panel[325] = Convert.ToInt32(T_DATC.ATC_StationStop);
            panel[322] = Convert.ToInt32(T_DATC.ATC_PatternApproach);

            panel[327] = Convert.ToInt32(TSP_ATS.ATS_TobuAts);
            panel[330] = Convert.ToInt32(TSP_ATS.ATS_ATSEmergencyBrake);
            panel[332] = Convert.ToInt32(TSP_ATS.ATS_StopAnnounce);
            panel[333] = Convert.ToInt32(TSP_ATS.ATS_EmergencyOperation);
            panel[331] = Convert.ToInt32(TSP_ATS.ATS_Confirm);
            panel[328] = Convert.ToInt32(TSP_ATS.ATS_60);
            panel[329] = Convert.ToInt32(TSP_ATS.ATS_15);

            //sound
            sound[258] = (int)T_DATC.ATC_Ding;
            var soundPlayMode = SoundPlayCommands.GetMode(sound[265]);
            if (soundPlayMode == SoundPlayMode.Continue && T_DATC.ATC_PatternApproachBeep == AtsSoundControlInstruction.Play)
                sound[265] = (int)AtsSoundControlInstruction.Stop;
            sound[265] = (int)T_DATC.ATC_PatternApproachBeep;
            sound[267] = (int)T_DATC.ATC_StationStopAnnounce;
            sound[266] = (int)Sound_Switchover;
            sound[268] = (int)T_DATC.ATC_EmergencyOperationAnnounce;
        }
    }
}
