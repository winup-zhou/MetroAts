using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JR_SotetsuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class JR_SotetsuSignal : AssemblyPluginBase {
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
                if (StandAloneMode) {
                    if (!ATS_P.ATSEnable) ATS_P.Init(state.Time);
                    if (!ATS_SN.ATSEnable && Config.SNEnable) ATS_SN.Init(state.Time);
                    if (ATS_P.P_PEnable) ATS_SN.ResetAll();
                    if (ATS_P.ATSEnable) {
                        ATS_P.Tick(state);
                        if (ATS_P.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATS_P.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATS_P.BrakeCommand;
                            BrakeTriggered = true;
                        }
                    }
                    if (ATS_SN.ATSEnable) {
                        ATS_SN.Tick(state);
                        if (ATS_SN.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATS_SN.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATS_SN.BrakeCommand;
                            BrakeTriggered = true;
                        }
                    }
                } else {
                    if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;
                    if (!ATS_P.ATSEnable) ATS_P.Init(state.Time);
                    if (!ATS_SN.ATSEnable && Config.SNEnable) ATS_SN.Init(state.Time);
                    if (ATS_P.P_PEnable) ATS_SN.ResetAll();
                    if (ATS_P.ATSEnable) {
                        ATS_P.Tick(state);
                        if (ATS_P.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATS_P.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATS_P.BrakeCommand;
                            BrakeTriggered = true;
                        }
                    }
                    if (ATS_SN.ATSEnable) {
                        ATS_SN.Tick(state);
                        if (ATS_SN.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATS_SN.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATS_SN.BrakeCommand;
                            BrakeTriggered = true;
                        }
                    }
                }
                if ((currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49)
                    || (currentSection.CurrentSignalIndex >= 50 && currentSection.CurrentSignalIndex <= 54)) {
                    sound[256] = (int)AtsSoundControlInstruction.PlayLooping;
                }
                if (!StandAloneMode) {
                    if (!(corePlugin.KeyPos == MetroAts.KeyPosList.JR || corePlugin.KeyPos == MetroAts.KeyPosList.Sotetsu)
                        || (corePlugin.SignalSWPos != MetroAts.SignalSWList.JR && corePlugin.SignalSWPos != MetroAts.SignalSWList.Sotetsu)) {
                        BrakeTriggered = false;
                        SignalEnable = false;
                        ATS_P.ResetAll();
                        ATS_SN.ResetAll();
                        sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }
                }
                if (BrakeTriggered) {
                    AtsHandles.PowerNotch = 0;
                    if (handles.PowerNotch == 0) BrakeTriggered = false;
                }
                UpdatePanelAndSound(panel, sound);
                if (state.Time.TotalMilliseconds - lastHandleOutputRefreshTime.TotalMilliseconds > Config.Panel_HandleOutputRefreshInterval) {
                    lastHandleOutputRefreshTime = state.Time;
                    lastBrakeNotch = AtsHandles.BrakeNotch; 
                    lastPowerNotch = AtsHandles.PowerNotch;
                    panel[Config.Panel_poweroutput] = AtsHandles.PowerNotch;
                    panel[Config.Panel_brakeoutput] = AtsHandles.BrakeNotch;
                } else {
                    panel[Config.Panel_poweroutput] = lastPowerNotch;
                    panel[Config.Panel_brakeoutput] = lastBrakeNotch;
                }
            } else {
                if (StandAloneMode) {
                    if (!SignalEnable && Keyin)
                        SignalEnable = true;
                    AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                    AtsHandles.ReverserPosition = ReverserPosition.N;
                } else {
                    Keyin = corePlugin.KeyPos == MetroAts.KeyPosList.JR || corePlugin.KeyPos == MetroAts.KeyPosList.Sotetsu;
                    if (!SignalEnable && Keyin && (corePlugin.SignalSWPos == MetroAts.SignalSWList.JR || corePlugin.SignalSWPos == MetroAts.SignalSWList.Sotetsu))
                        SignalEnable = true;
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
            Sound_Keyin = Sound_Keyout = Sound_ResetSW = AtsSoundControlInstruction.Continue;
        }

        private static void UpdatePanelAndSound(IList<int> panel, IList<int> sound) {
            sound[273] = (int)Sound_ResetSW;

            //panel
            panel[256] = Convert.ToInt32(ATS_P.P_Power || Config.PPowerAlwaysLight);
            panel[257] = Convert.ToInt32(ATS_P.P_PatternApproach);
            panel[258] = Convert.ToInt32(ATS_P.P_BrakeActioned);
            panel[259] = Convert.ToInt32(ATS_P.P_EBActioned);
            panel[260] = Convert.ToInt32(ATS_P.P_BrakeOverride);
            panel[261] = Convert.ToInt32(ATS_P.P_PEnable);
            panel[262] = Convert.ToInt32(ATS_P.P_Fail);
            panel[341] = Convert.ToInt32(ATS_SN.SN_Power);
            panel[342] = Convert.ToInt32(ATS_SN.SN_Action);

            sound[258] = (int)ATS_P.P_Ding;
            sound[257] = (int)ATS_SN.SN_Chime;
            if(ATS_SN.ATSEnable)sound[256] = (int)ATS_SN.SN_WarningBell;
        }
    }
}
