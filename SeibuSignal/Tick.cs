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
            while (sectionManager.Sections[pointer].Location < state.Location) {
                pointer++;
                if (pointer >= sectionManager.Sections.Count) {
                    pointer = sectionManager.Sections.Count - 1;
                    break;
                }
            }

            var currentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;

            // 信号切换分派：钥匙/档位归属由核心仲裁，激活状态据此翻转
            corePlugin.Arbitrate(this);

            if (SignalEnable) {
                    if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;
                    if (corePlugin.SignalSWPos == MetroAts.SignalSWList.SeibuATS) {
                        if (!SeibuATS.ATSEnable) SeibuATS.Init(state.Time);
                        if (ATC.ATCEnable)
                            ATC.ResetAll();
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) {
                        if (!ATC.ATCEnable) ATC.Init(state.Time);
                        if (SeibuATS.ATSEnable)
                            SeibuATS.ResetAll();
                    }

                    if (SeibuATS.ATSEnable) {
                        SeibuATS.Tick(state, sectionManager);
                        if (SeibuATS.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, SeibuATS.BrakeCommand);
                            else AtsHandles.BrakeNotch = SeibuATS.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (ATC.ATCEnable && !(currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49))
                            ATC.ResetAll();
                    }
                    if (ATC.ATCEnable) {
                        ATC.Tick(state, handles, currentSection, corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset, corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot);
                        if (ATC.BrakeCommand > 0) {
                            if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                                AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATC.BrakeCommand);
                            else AtsHandles.BrakeNotch = ATC.BrakeCommand;
                            BrakeTriggered = true;
                        }
                        if (SeibuATS.ATSEnable)
                            SeibuATS.ResetAll();
                    }
                    Config.PanelMap.WritePanel(panel, "ATC_Depot", corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot ? 1 : 0);
                    Config.PanelMap.WritePanel(panel, "ATC_Noset", corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset ? 1 : 0);
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (!ATC.ATCEnable) {
                            if (corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                                ATC.Init(state.Time);
                            }else if(corePlugin.SignalSWPos == MetroAts.SignalSWList.SeibuATS) {
                                ATC.InitNow();
                            }
                        }
                        Config.SoundMap.WriteSound(sound, "Warning", ((corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot && currentSection.CurrentSignalIndex >= 38 && currentSection.CurrentSignalIndex <= 48)
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) ? (int)SoundPlayMode.Stop : (int)SoundPlayMode.PlayLooping);
                    } else if (corePlugin.SignalSWPos != MetroAts.SignalSWList.ATC) {
                        if (ATC.ATCEnable) ATC.ResetAll();
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

            // 面板值按内置默认端子号作为槽位计算，末端经 PanelMap 映射实际端子号并记录状态
            int[] newPanelValues = new int[350];
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_01")] = Convert.ToInt32(ATC.ATC_01);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_25")] = Convert.ToInt32(ATC.ATC_25);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_40")] = Convert.ToInt32(ATC.ATC_40);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_55")] = Convert.ToInt32(ATC.ATC_55);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_75")] = Convert.ToInt32(ATC.ATC_75);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_90")] = Convert.ToInt32(ATC.ATC_90);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Stop")] = Convert.ToInt32(ATC.ATC_Stop);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Proceed")] = Convert.ToInt32(ATC.ATC_Proceed);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_X")] = Convert.ToInt32(ATC.ATC_X);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATCNeedle")] = ATC.ATCNeedle;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATCNeedle_Disappear")] = Convert.ToInt32(ATC.ATCNeedle_Disappear);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_ATC")] = Convert.ToInt32(ATC.ATC_ATC);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Depot")] = ATC.ATCEnable ? Convert.ToInt32(ATC.ATC_Depot) : 0;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Noset")] = (ATC.ATCEnable && ATC.ATC_Noset) ? Convert.ToInt32(ATC.ATC_Noset) : 0;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_ServiceBrake")] = Convert.ToInt32(ATC.ATC_ServiceBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EmergencyBrake")] = Convert.ToInt32(ATC.ATC_EmergencyBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EmergencyOperation")] = Convert.ToInt32(ATC.ATC_EmergencyOperation);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_Power")] = Convert.ToInt32(SeibuATS.ATS_Power);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_EB")] = Convert.ToInt32(SeibuATS.ATS_EB);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_Stop")] = Convert.ToInt32(SeibuATS.ATS_Stop);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_Confirm")] = Convert.ToInt32(SeibuATS.ATS_Confirm);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_Limit")] = Convert.ToInt32(SeibuATS.ATS_Limit);

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

            Config.SoundMap.WriteSound(sound, "Ding", (int)ATC.ATC_Ding);
            if (ATC.ATCEnable && ATC.ATC_Noset) Config.SoundMap.WriteSound(sound, "Warning", (int)ATC.ATC_WarningBell);
            Config.SoundMap.WriteSound(sound, "EmergencyOperationAnnounce", (int)ATC.ATC_EmergencyOperationAnnounce);
            Config.SoundMap.WriteSound(sound, "StopAnnounce", (int)SeibuATS.ATS_StopAnnounce);
            Config.SoundMap.WriteSound(sound, "EBAnnounce", (int)SeibuATS.ATS_EBAnnounce);
        }
    }
}
