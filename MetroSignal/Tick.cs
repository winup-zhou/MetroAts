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
                    if (!CS_ATC.ATCEnable) Config.PanelMap.WritePanel(panel, "ATC_Depot", corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot ? 1 : 0);
                    Config.PanelMap.WritePanel(panel, "ATC_Noset", (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR) ? 1 : 0);
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                        if (!CS_ATC.ATCEnable && (corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR))
                            CS_ATC.Init(state.Time);
                        Config.SoundMap.WriteSound(sound, "Warning",
                            ((corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot && currentSection.CurrentSignalIndex >= 38 && currentSection.CurrentSignalIndex <= 48)
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) ? (int)SoundPlayMode.Stop : (int)SoundPlayMode.PlayLooping);
                    } else if (currentSection.CurrentSignalIndex >= 50 && currentSection.CurrentSignalIndex <= 54) {
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                        if (!WS_ATC.ATCEnable &&
                            (corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset
                            || corePlugin.SignalSWPos == MetroAts.SignalSWList.JR)) WS_ATC.Init(state.Time);
                        Config.SoundMap.WriteSound(sound, "Warning",
                            corePlugin.SignalSWPos == MetroAts.SignalSWList.WS_ATC ? (int)SoundPlayMode.Stop : (int)SoundPlayMode.PlayLooping);
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                        if (CS_ATC.ATCEnable) CS_ATC.ResetAll();
                        if (WS_ATC.ATCEnable) WS_ATC.ResetAll();
                        Config.SoundMap.WriteSound(sound, "Warning", (int)SoundPlayMode.Stop);
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
            Sound_ResetSW = SoundPlayMode.Continue;
            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }

        private static void UpdatePanelAndSound(IList<int> panel, IList<int> sound, TimeSpan currentTime) {
            Config.SoundMap.WriteSound(sound, "ResetSW", (int)Sound_ResetSW);

            bool needRefresh = true;
            if (Config.isLCD) {
                if (currentTime.TotalMilliseconds - lastPanelOutputRefreshTime.TotalMilliseconds > Config.LCDRefreshInterval) {
                    lastPanelOutputRefreshTime = currentTime;
                    needRefresh = true;
                } else {
                    needRefresh = false;
                }
            }

            // 面板值仍按"内置默认端子号"作为槽位计算（与既有语义一致），
            // 末端写面板时经 PanelMap 映射到"实际（可被 [output] 覆盖的）端子号"，并记录平行状态。
            int[] newPanelValues = new int[350];
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_01")] = Convert.ToInt32(CS_ATC.ATC_01);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_10")] = Convert.ToInt32(CS_ATC.ATC_10);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_15")] = Convert.ToInt32(CS_ATC.ATC_15);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_20")] = Convert.ToInt32(CS_ATC.ATC_20);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_25")] = Convert.ToInt32(CS_ATC.ATC_25);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_30")] = Convert.ToInt32(CS_ATC.ATC_30);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_35")] = Convert.ToInt32(CS_ATC.ATC_35);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_40")] = Convert.ToInt32(CS_ATC.ATC_40);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_45")] = Convert.ToInt32(CS_ATC.ATC_45);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_50")] = Convert.ToInt32(CS_ATC.ATC_50);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_55")] = Convert.ToInt32(CS_ATC.ATC_55);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_60")] = Convert.ToInt32(CS_ATC.ATC_60);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_65")] = Convert.ToInt32(CS_ATC.ATC_65);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_70")] = Convert.ToInt32(CS_ATC.ATC_70);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_75")] = Convert.ToInt32(CS_ATC.ATC_75);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_80")] = Convert.ToInt32(CS_ATC.ATC_80);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_85")] = Convert.ToInt32(CS_ATC.ATC_85);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_90")] = Convert.ToInt32(CS_ATC.ATC_90);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_95")] = Convert.ToInt32(CS_ATC.ATC_95);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_100")] = Convert.ToInt32(CS_ATC.ATC_100);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_105")] = Convert.ToInt32(CS_ATC.ATC_105);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_110")] = Convert.ToInt32(CS_ATC.ATC_110);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Stop")] = Convert.ToInt32(CS_ATC.ATC_Stop);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Proceed")] = Convert.ToInt32(CS_ATC.ATC_Proceed);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_P")] = Convert.ToInt32(CS_ATC.ATC_P);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_SignalAnn")] = Convert.ToInt32(CS_ATC.ATC_SignalAnn);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_X")] = Convert.ToInt32(CS_ATC.ATC_X);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ORPNeedle")] = CS_ATC.ORPNeedle;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATCNeedle")] = CS_ATC.ATCNeedle;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATCNeedle_Disappear")] = Convert.ToInt32(CS_ATC.ATCNeedle_Disappear);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_ATC")] = Convert.ToInt32(CS_ATC.ATC_ATC);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Depot")] = CS_ATC.ATCEnable ? Convert.ToInt32(CS_ATC.ATC_Depot) : 0;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Noset")] = (CS_ATC.ATCEnable && CS_ATC.ATC_Noset) || (WS_ATC.ATCEnable && WS_ATC.ATC_Noset)
                ? Convert.ToInt32(CS_ATC.ATC_Noset || WS_ATC.ATC_Noset) : 0;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_ServiceBrake")] = Convert.ToInt32(CS_ATC.ATC_ServiceBrake || WS_ATC.ATC_ServiceBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EmergencyBrake")] = Convert.ToInt32(CS_ATC.ATC_EmergencyBrake || WS_ATC.ATC_EmergencyBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EmergencyOperation")] = Convert.ToInt32(CS_ATC.ATC_EmergencyOperation);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_WSATC")] = Convert.ToInt32(WS_ATC.ATC_WSATC);

            foreach (var key in Config.PanelMap.Index.Keys) {
                int defIdx = Config.PanelMap.DefaultIndexOf(key);
                int actualIdx = Config.PanelMap.IndexOf(key);
                int value = needRefresh ? newPanelValues[defIdx] : lastPanelOutput[defIdx];
                Config.PanelMap.Record(key, value);
                if (actualIdx >= 0 && actualIdx < panel.Count) {
                    if (needRefresh) {
                        panel[actualIdx] = newPanelValues[defIdx];
                        lastPanelOutput[defIdx] = newPanelValues[defIdx];
                    } else {
                        panel[actualIdx] = lastPanelOutput[defIdx];
                    }
                }
            }

            Config.SoundMap.WriteSound(sound, "Ding", (int)CS_ATC.ATC_Ding);
            Config.SoundMap.WriteSound(sound, "ORPBeep", (int)CS_ATC.ATC_ORPBeep);
            if (CS_ATC.ATCEnable && CS_ATC.ATC_Noset) Config.SoundMap.WriteSound(sound, "Warning", (int)CS_ATC.ATC_WarningBell);
            Config.SoundMap.WriteSound(sound, "EmergencyOperationAnnounce", (int)CS_ATC.ATC_EmergencyOperationAnnounce);
        }
    }
}
