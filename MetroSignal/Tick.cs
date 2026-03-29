using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using MetroAts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class MetroSignal : AssemblyPluginBase {
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
            var nextSection = sectionManager.Sections[pointer] as Section;
            var PreviousSection = sectionManager.Sections[pointer > 1 ? pointer - 2 : 0] as Section;

            if (SignalEnable) {
                if (StandAloneMode) {
                    if (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.WS_ATC) {
                        if (!WS_ATC.ATCEnable) WS_ATC.Init(state.Time);
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                    } else if (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.ATC) {
                        if (!CS_ATC.ATCEnable) CS_ATC.Init(state.Time);
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                    }
                    if (CS_ATC.ATCEnable) {
                        CS_ATC.Tick(state, currentSection, nextSection, handles, Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset, Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.InDepot);
                        if (CS_ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, CS_ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = CS_ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                    }
                    if (WS_ATC.ATCEnable) {
                        WS_ATC.Tick(state, currentSection, Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset);
                        if (WS_ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, WS_ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = WS_ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                    }
                    panel[274] = Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.InDepot ? 1 : 0;
                    panel[277] = Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset ? 1 : 0;
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                        if (!CS_ATC.ATCEnable && (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.InDepot
                            || Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset)) CS_ATC.Init(state.Time);
                        sound[256] = ((Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.InDepot && currentSection.CurrentSignalIndex >= 38 && currentSection.CurrentSignalIndex <= 48)
                        || Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.ATC) ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (currentSection.CurrentSignalIndex >= 50 && currentSection.CurrentSignalIndex <= 54) {
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                        if (!WS_ATC.ATCEnable &&
                            (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset)) WS_ATC.Init(state.Time);
                        sound[256] = Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.WS_ATC ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.InDepot || Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset) {
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                        sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }
                } else {
                    if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;
                    if (corePlugin.SignalSWPos == MetroAts.SignalSWList.WS_ATC) {
                        if (!WS_ATC.ATCEnable) WS_ATC.Init(state.Time);
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) {
                        if (!CS_ATC.ATCEnable) CS_ATC.Init(state.Time);
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();

                    }
                    if (CS_ATC.ATCEnable) {
                        CS_ATC.Tick(state, currentSection, nextSection, handles, corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset, corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot);
                        if (CS_ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, CS_ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = CS_ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                    }
                    if (WS_ATC.ATCEnable) {
                        WS_ATC.Tick(state, currentSection, corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset);
                        if (WS_ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, WS_ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = WS_ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                    }
                    if (!CS_ATC.ATCEnable) panel[274] = corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot ? 1 : 0;
                    panel[277] = (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR) ? 1 : 0;
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                        if (!CS_ATC.ATCEnable && (corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR))
                            CS_ATC.Init(state.Time);
                        sound[256] = ((corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot && currentSection.CurrentSignalIndex >= 38 && currentSection.CurrentSignalIndex <= 48)
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (currentSection.CurrentSignalIndex >= 50 && currentSection.CurrentSignalIndex <= 54) {
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                        if (!WS_ATC.ATCEnable &&
                            (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR)) WS_ATC.Init(state.Time);
                        sound[256] = corePlugin.SignalSWPos == MetroAts.SignalSWList.WS_ATC ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                        sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }
                }
                if (!StandAloneMode) {
                    if (!(corePlugin.KeyPos == MetroAts.KeyPosList.Metro || corePlugin.KeyPos == MetroAts.KeyPosList.ToyoKosoku) ||
                        (corePlugin.SignalSWPos != MetroAts.SignalSWList.InDepot
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.Noset
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.ATC
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.ATP
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.WS_ATC
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.JR)) {
                        BrakeTriggered = false;
                        SignalEnable = false;
                        WS_ATC.ResetAll();
                        CS_ATC.ResetAll();
                        if (sound[256] != (int)AtsSoundControlInstruction.Stop) sound[256] = (int)AtsSoundControlInstruction.Stop;
                        panel[274] = 0;
                        panel[277] = 0;
                    }
                }
                if (BrakeTriggered) {
                    AtsHandles.PowerNotch = 0;
                    if (handles.PowerNotch == 0) BrakeTriggered = false;
                }
                UpdatePanelAndSound(panel, sound, state.Time);
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
                    if (!SignalEnable && Keyin && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1)
                        SignalEnable = true;
                    AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                    AtsHandles.ReverserPosition = ReverserPosition.N;
                    if (sound[256] != (int)AtsSoundControlInstruction.Stop) sound[256] = (int)AtsSoundControlInstruction.Stop;
                    panel[275] = 0;
                    panel[278] = 0;
                } else {
                    Keyin = corePlugin.KeyPos == MetroAts.KeyPosList.Metro || corePlugin.KeyPos == MetroAts.KeyPosList.ToyoKosoku;
                    if (!SignalEnable && Keyin &&
                        (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.WS_ATC
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.ATP)
                        && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1)
                        SignalEnable = true;
                }

            }
            if (StandAloneMode) {
                var SignalSWText = "";
                switch (Config.SignalSWLists[NowSignalSW]) {
                    case SignalSWListStandAlone.Noset:
                        SignalSWText = "非設";
                        break;
                    case SignalSWListStandAlone.InDepot:
                        SignalSWText = "構内";
                        break;
                    case SignalSWListStandAlone.ATC:
                        SignalSWText = "ATC";
                        break;
                    case SignalSWListStandAlone.WS_ATC:
                        SignalSWText = "WS-ATC";
                        break;
                    case SignalSWListStandAlone.ATP:
                        SignalSWText = "ATP";
                        break;
                    default:
                        SignalSWText = "無効";
                        break;
                }
                var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
                leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
                leverText.Text = $"キー:{(Keyin ? "入" : "切")} 保安:{SignalSWText}\n{description}";
                if (isDoorOpen) AtsHandles.ReverserPosition = ReverserPosition.N;
                sound[270] = (int)Sound_Keyin;
                sound[271] = (int)Sound_Keyout;
                sound[272] = (int)Sound_SignalSW;

                panel[Config.Panel_keyoutput] = Convert.ToInt32(Keyin);
                if (!Config.SignalSW_legacyoutput) {
                    panel[Config.Panel_SignalSWoutput] = (int)Config.SignalSWLists[NowSignalSW];
                } else {
                    switch (Config.SignalSWLists[NowSignalSW]) {
                        case SignalSWListStandAlone.ATP:
                            panel[Config.Panel_SignalSWoutput] = 0;
                            break;
                        case SignalSWListStandAlone.WS_ATC:
                            panel[Config.Panel_SignalSWoutput] = 5;
                            break;
                        case SignalSWListStandAlone.Noset:
                            panel[Config.Panel_SignalSWoutput] = 3;
                            break;
                        case SignalSWListStandAlone.ATC:
                            panel[Config.Panel_SignalSWoutput] = 1;
                            break;
                        case SignalSWListStandAlone.InDepot:
                            panel[Config.Panel_SignalSWoutput] = 2;
                            break;
                    }
                }
            }

            //sound reset
            Sound_Keyin = Sound_Keyout = Sound_ResetSW = Sound_SignalSW = AtsSoundControlInstruction.Continue;
            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }

        private static void UpdatePanelAndSound(IList<int> panel, IList<int> sound, TimeSpan currentTime) {
            sound[273] = (int)Sound_ResetSW;

            bool needRefresh = true;
            if (Config.isLCD) {
                if (currentTime.TotalMilliseconds - lastPanelOutputRefreshTime.TotalMilliseconds > Config.LCDRefreshInterval) {
                    lastPanelOutputRefreshTime = currentTime;
                    needRefresh = true;
                } else {
                    needRefresh = false;
                }
            }

            int[] panelIndices = new int[] {
                287,288,289,290,291,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,
                285,286,313,312,284,314,311,310,263,274,277,270,266,280,340
            };

            int[] newPanelValues = new int[350];
            newPanelValues[287] = Convert.ToInt32(CS_ATC.ATC_01);
            newPanelValues[288] = Convert.ToInt32(CS_ATC.ATC_10);
            newPanelValues[289] = Convert.ToInt32(CS_ATC.ATC_15);
            newPanelValues[290] = Convert.ToInt32(CS_ATC.ATC_20);
            newPanelValues[291] = Convert.ToInt32(CS_ATC.ATC_25);
            newPanelValues[292] = Convert.ToInt32(CS_ATC.ATC_30);
            newPanelValues[293] = Convert.ToInt32(CS_ATC.ATC_35);
            newPanelValues[294] = Convert.ToInt32(CS_ATC.ATC_40);
            newPanelValues[295] = Convert.ToInt32(CS_ATC.ATC_45);
            newPanelValues[296] = Convert.ToInt32(CS_ATC.ATC_50);
            newPanelValues[297] = Convert.ToInt32(CS_ATC.ATC_55);
            newPanelValues[298] = Convert.ToInt32(CS_ATC.ATC_60);
            newPanelValues[299] = Convert.ToInt32(CS_ATC.ATC_65);
            newPanelValues[300] = Convert.ToInt32(CS_ATC.ATC_70);
            newPanelValues[301] = Convert.ToInt32(CS_ATC.ATC_75);
            newPanelValues[302] = Convert.ToInt32(CS_ATC.ATC_80);
            newPanelValues[303] = Convert.ToInt32(CS_ATC.ATC_85);
            newPanelValues[304] = Convert.ToInt32(CS_ATC.ATC_90);
            newPanelValues[305] = Convert.ToInt32(CS_ATC.ATC_95);
            newPanelValues[306] = Convert.ToInt32(CS_ATC.ATC_100);
            newPanelValues[307] = Convert.ToInt32(CS_ATC.ATC_105);
            newPanelValues[308] = Convert.ToInt32(CS_ATC.ATC_110);

            newPanelValues[285] = Convert.ToInt32(CS_ATC.ATC_Stop);
            newPanelValues[286] = Convert.ToInt32(CS_ATC.ATC_Proceed);

            newPanelValues[313] = Convert.ToInt32(CS_ATC.ATC_P);
            newPanelValues[312] = Convert.ToInt32(CS_ATC.ATC_SignalAnn);
            newPanelValues[284] = Convert.ToInt32(CS_ATC.ATC_X);

            newPanelValues[314] = CS_ATC.ORPNeedle;
            newPanelValues[311] = CS_ATC.ATCNeedle;
            newPanelValues[310] = Convert.ToInt32(CS_ATC.ATCNeedle_Disappear);

            newPanelValues[263] = Convert.ToInt32(CS_ATC.ATC_ATC);
            newPanelValues[274] = CS_ATC.ATCEnable ? Convert.ToInt32(CS_ATC.ATC_Depot) : 0;
            newPanelValues[277] = (CS_ATC.ATCEnable && CS_ATC.ATC_Noset) || (WS_ATC.ATCEnable && WS_ATC.ATC_Noset)
                ? Convert.ToInt32(CS_ATC.ATC_Noset || WS_ATC.ATC_Noset) : 0;
            newPanelValues[270] = Convert.ToInt32(CS_ATC.ATC_ServiceBrake || WS_ATC.ATC_ServiceBrake);
            newPanelValues[266] = Convert.ToInt32(CS_ATC.ATC_EmergencyBrake || WS_ATC.ATC_EmergencyBrake);
            newPanelValues[280] = Convert.ToInt32(CS_ATC.ATC_EmergencyOperation);

            newPanelValues[340] = Convert.ToInt32(WS_ATC.ATC_WSATC);

            foreach (var idx in panelIndices) {
                if (needRefresh) {
                    panel[idx] = newPanelValues[idx];
                    lastPanelOutput[idx] = newPanelValues[idx];
                } else {
                    panel[idx] = lastPanelOutput[idx];
                }
            }

            sound[258] = (int)CS_ATC.ATC_Ding;
            sound[259] = (int)CS_ATC.ATC_ORPBeep;
            if (CS_ATC.ATCEnable && CS_ATC.ATC_Noset) { sound[256] = (int)CS_ATC.ATC_WarningBell; }
            sound[261] = (int)CS_ATC.ATC_EmergencyOperationAnnounce;
        }
    }
}
