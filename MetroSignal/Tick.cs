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

            // 信号切换分派：钥匙/档位归属由核心仲裁，激活状态据此翻转
            corePlugin.Arbitrate(this);

            if (SignalEnable) {
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
                // 启用/去启用已由上方 corePlugin.Arbitrate(this) 管理（Activate/Deactivate）
            }

            //sound reset
            Sound_ResetSW = AtsSoundControlInstruction.Continue;
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
