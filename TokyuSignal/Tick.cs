using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using MetroAts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokyuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class TokyuSignal : AssemblyPluginBase {
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

            if (SignalEnable) {
                if (StandAloneMode) {
                    if (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.ATC) {
                        if (!ATC.ATCEnable) ATC.Init(state.Time);
                        if (TokyuATS.ATSEnable) TokyuATS.ResetAll();
                    } else if (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.TokyuATS) {
                        if (!TokyuATS.ATSEnable) TokyuATS.Init(state.Time);
                        if (ATC.ATCEnable) ATC.ResetAll();
                    }
                    if (ATC.ATCEnable) {
                        ATC.Tick(state, currentSection, nextSection, handles, Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset);
                        if (ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (TokyuATS.ATSEnable) TokyuATS.ResetAll();
                    }
                    if (TokyuATS.ATSEnable) {
                        TokyuATS.Tick(state);
                        if (TokyuATS.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, TokyuATS.BrakeCommand);
                            else AtsHandles.BrakeNotch = TokyuATS.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (ATC.ATCEnable) ATC.ResetAll();
                    }
                    panel[279] = Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset ? 1 : 0;
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (!ATC.ATCEnable && (Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.Noset))
                            ATC.Init(state.Time);
                        if (TokyuATS.ATSEnable) {
                            TokyuATS.ResetAll();
                            AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                            AtsHandles.ReverserPosition = ReverserPosition.N;
                        }
                        if (!ATC.ATCEnable)
                            sound[256] = Config.SignalSWLists[NowSignalSW] == SignalSWListStandAlone.ATC ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                        if (ATC.ATCEnable) ATC.ResetAll();
                        sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }
                } else {
                    if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;
                    if (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) {
                        if (!ATC.ATCEnable) ATC.Init(state.Time);
                        if (TokyuATS.ATSEnable) TokyuATS.ResetAll();
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.TokyuATS) {
                        if (!TokyuATS.ATSEnable) TokyuATS.Init(state.Time);
                        if (ATC.ATCEnable) ATC.ResetAll();
                    }
                    if (ATC.ATCEnable) {
                        ATC.Tick(state, currentSection, nextSection, handles, corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset);
                        if (ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (TokyuATS.ATSEnable) TokyuATS.ResetAll();
                    }
                    if (TokyuATS.ATSEnable) {
                        TokyuATS.Tick(state);
                        if (TokyuATS.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2) 
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, TokyuATS.BrakeCommand);
                            else AtsHandles.BrakeNotch = TokyuATS.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (ATC.ATCEnable) ATC.ResetAll();
                    }
                    panel[279] = corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset ? 1 : 0;
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (!ATC.ATCEnable) {
                            if (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                                ATC.Init(state.Time);
                            } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.TokyuATS) {
                                ATC.InitNow();
                            }
                        }
                        //if (TokyuATS.ATSEnable) { 
                        //    TokyuATS.ResetAll();
                        //    AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                        //    AtsHandles.ReverserPosition = ReverserPosition.N;
                        //}
                        if (!ATC.ATCEnable) sound[256] = (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC)
                            ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                        if (ATC.ATCEnable) ATC.ResetAll();
                        sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }
                }
                if (!StandAloneMode) {
                    if (!(corePlugin.KeyPos == MetroAts.KeyPosList.Tokyu) ||
                        (corePlugin.SignalSWPos != MetroAts.SignalSWList.Noset
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.ATC
                        && corePlugin.SignalSWPos != MetroAts.SignalSWList.TokyuATS)) {
                        BrakeTriggered = false;
                        SignalEnable = false;
                        ATC.ResetAll();
                        TokyuATS.ResetAll();
                        if (sound[256] != (int)AtsSoundControlInstruction.Stop) sound[256] = (int)AtsSoundControlInstruction.Stop;
                        panel[276] = 0;
                        panel[279] = 0;
                    }
                }
                if (BrakeTriggered) {
                    AtsHandles.PowerNotch = 0;
                    if (handles.PowerNotch == 0) BrakeTriggered = false;
                }
                UpdatePanelAndSound(panel, sound);
                if (state.Time.TotalMilliseconds - lastHandleOutputRefreshTime.TotalMilliseconds > Config.Panel_HandleOutputRefreshInterval) {
                    lastHandleOutputRefreshTime = state.Time;
                    panel[Config.Panel_poweroutput] = AtsHandles.PowerNotch;
                    panel[Config.Panel_brakeoutput] = AtsHandles.BrakeNotch;
                }
            } else {
                if (StandAloneMode) {
                    if (!SignalEnable && Keyin && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1)
                        SignalEnable = true;
                    AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                    AtsHandles.ReverserPosition = ReverserPosition.N;
                    if (sound[256] != (int)AtsSoundControlInstruction.Stop) sound[256] = (int)AtsSoundControlInstruction.Stop;
                    panel[276] = 0;
                    panel[279] = 0;
                } else {
                    Keyin = corePlugin.KeyPos == MetroAts.KeyPosList.Tokyu;
                    if (!SignalEnable && Keyin &&
                        (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.TokyuATS)
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
                    case SignalSWListStandAlone.ATC:
                        SignalSWText = "ATC";
                        break;
                    case SignalSWListStandAlone.TokyuATS:
                        SignalSWText = "東急ATS";
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
                        case SignalSWListStandAlone.TokyuATS:
                            panel[Config.Panel_SignalSWoutput] = 0;
                            break;
                        case SignalSWListStandAlone.Noset:
                            panel[Config.Panel_SignalSWoutput] = 4;
                            break;
                        case SignalSWListStandAlone.ATC:
                            panel[Config.Panel_SignalSWoutput] = 1;
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

        private static void UpdatePanelAndSound(IList<int> panel, IList<int> sound) {
            sound[273] = (int)Sound_ResetSW;

            //panel
            panel[287] = Convert.ToInt32(ATC.ATC_01);
            panel[288] = Convert.ToInt32(ATC.ATC_10);
            panel[289] = Convert.ToInt32(ATC.ATC_15);
            panel[290] = Convert.ToInt32(ATC.ATC_20);
            panel[291] = Convert.ToInt32(ATC.ATC_25);
            panel[292] = Convert.ToInt32(ATC.ATC_30);
            panel[293] = Convert.ToInt32(ATC.ATC_35);
            panel[294] = Convert.ToInt32(ATC.ATC_40);
            panel[295] = Convert.ToInt32(ATC.ATC_45);
            panel[296] = Convert.ToInt32(ATC.ATC_50);
            panel[297] = Convert.ToInt32(ATC.ATC_55);
            panel[298] = Convert.ToInt32(ATC.ATC_60);
            panel[299] = Convert.ToInt32(ATC.ATC_65);
            panel[300] = Convert.ToInt32(ATC.ATC_70);
            panel[301] = Convert.ToInt32(ATC.ATC_75);
            panel[302] = Convert.ToInt32(ATC.ATC_80);
            panel[303] = Convert.ToInt32(ATC.ATC_85);
            panel[304] = Convert.ToInt32(ATC.ATC_90);
            panel[305] = Convert.ToInt32(ATC.ATC_95);
            panel[306] = Convert.ToInt32(ATC.ATC_100);
            panel[307] = Convert.ToInt32(ATC.ATC_105);
            panel[308] = Convert.ToInt32(ATC.ATC_110);

            panel[285] = Convert.ToInt32(ATC.ATC_Stop);
            panel[286] = Convert.ToInt32(ATC.ATC_Proceed);

            panel[313] = Convert.ToInt32(ATC.ATC_P);
            panel[312] = Convert.ToInt32(ATC.ATC_SignalAnn);
            panel[284] = Convert.ToInt32(ATC.ATC_X);

            panel[311] = ATC.ATCNeedle;
            panel[310] = Convert.ToInt32(ATC.ATCNeedle_Disappear);

            panel[265] = Convert.ToInt32(ATC.ATC_ATC);
            if (ATC.ATCEnable) panel[276] = Convert.ToInt32(ATC.ATC_Depot);
            if (ATC.ATCEnable && ATC.ATC_Noset) panel[279] = Convert.ToInt32(ATC.ATC_Noset);
            panel[272] = Convert.ToInt32(ATC.ATC_ServiceBrake);
            panel[268] = Convert.ToInt32(ATC.ATC_EmergencyBrake);
            panel[282] = Convert.ToInt32(ATC.ATC_EmergencyOperation);
            panel[342] = Convert.ToInt32(ATC.ATC_StationStop);

            panel[343] = Convert.ToInt32(TokyuATS.ATS_TokyuATS);
            panel[344] = Convert.ToInt32(TokyuATS.ATS_EB);
            panel[345] = Convert.ToInt32(TokyuATS.ATS_WarnNormal);
            panel[346] = Convert.ToInt32(TokyuATS.ATS_WarnTriggered);

            sound[258] = (int)ATC.ATC_Ding;
            sound[259] = (int)ATC.ATC_ORPBeep;
            if (ATC.ATC_SignalAnnBeep == AtsSoundControlInstruction.Play)
                sound[260] = (int)AtsSoundControlInstruction.Stop;
            sound[260] = (int)ATC.ATC_SignalAnnBeep;
            sound[269] = (int)TokyuATS.ATS_WarnBell;
            if (TokyuATS.ATSEnable) {
                sound[256] = (int)TokyuATS.ATS_EBBell;
            }
            if (ATC.ATCEnable) {
                sound[256] = (int)ATC.ATC_WarningBell;
            }
            sound[261] = (int)ATC.ATC_EmergencyOperationAnnounce;
        }
    }
}
