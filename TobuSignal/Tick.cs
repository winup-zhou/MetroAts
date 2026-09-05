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

            // 信号切换分派：钥匙/档位归属由核心仲裁，激活状态据此翻转
            corePlugin.Arbitrate(this);

            if (SignalEnable) {
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
                            Sound_Switchover = SoundPlayMode.Play;
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
                            Sound_Switchover = SoundPlayMode.Play;
                        } else {
                            TSP_ATS.Init(state.Time);
                        }
                    }
                }
                if (currentSection.CurrentSignalIndex >= 109 && currentSection.CurrentSignalIndex != 134 && currentSection.CurrentSignalIndex < 149)
                    Config.SoundMap.WriteSound(sound, "Warning", corePlugin.SignalSWPos == MetroAts.SignalSWList.Tobu ?
                        (int)SoundPlayMode.Stop : (int)SoundPlayMode.PlayLooping);
                if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;
                // 去启用（key/sw 移出合法集）已由上方 corePlugin.Arbitrate(this) 的 Deactivate 管理
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
            Sound_ResetSW = Sound_Switchover = SoundPlayMode.Continue;

            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }

        private static void UpdatePanelAndSound(IList<int> panel,IList<int> sound, TimeSpan currentTime) {
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
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_01")] = Convert.ToInt32(T_DATC.ATC_01);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_10")] = Convert.ToInt32(T_DATC.ATC_10);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_15")] = Convert.ToInt32(T_DATC.ATC_15);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_20")] = Convert.ToInt32(T_DATC.ATC_20);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_25")] = Convert.ToInt32(T_DATC.ATC_25);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_30")] = Convert.ToInt32(T_DATC.ATC_30);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_35")] = Convert.ToInt32(T_DATC.ATC_35);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_40")] = Convert.ToInt32(T_DATC.ATC_40);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_45")] = Convert.ToInt32(T_DATC.ATC_45);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_50")] = Convert.ToInt32(T_DATC.ATC_50);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_55")] = Convert.ToInt32(T_DATC.ATC_55);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_60")] = Convert.ToInt32(T_DATC.ATC_60);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_65")] = Convert.ToInt32(T_DATC.ATC_65);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_70")] = Convert.ToInt32(T_DATC.ATC_70);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_75")] = Convert.ToInt32(T_DATC.ATC_75);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_80")] = Convert.ToInt32(T_DATC.ATC_80);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_85")] = Convert.ToInt32(T_DATC.ATC_85);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_90")] = Convert.ToInt32(T_DATC.ATC_90);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_95")] = Convert.ToInt32(T_DATC.ATC_95);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_100")] = Convert.ToInt32(T_DATC.ATC_100);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_105")] = Convert.ToInt32(T_DATC.ATC_105);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_110")] = Convert.ToInt32(T_DATC.ATC_110);

            if (!Config.SeparateATCGRlamp) {
                newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Stop")] = Convert.ToInt32(T_DATC.ATC_Stop);
                newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Proceed")] = Convert.ToInt32(T_DATC.ATC_Proceed);
            } else {
                newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_StopSeparate")] = Convert.ToInt32(T_DATC.ATC_Stop);
                newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_ProceedSeparate")] = Convert.ToInt32(T_DATC.ATC_Proceed);
            }

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_P")] = Convert.ToInt32(T_DATC.ATC_P);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_X")] = Convert.ToInt32(T_DATC.ATC_X);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ORPNeedle")] = T_DATC.ORPNeedle;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATCNeedle")] = T_DATC.ATCNeedle;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATCNeedle_Disappear")] = Convert.ToInt32(T_DATC.ATCNeedle_Disappear);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EndPointDistance")] = T_DATC.ATC_EndPointDistance;
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_SwitcherPosition")] = T_DATC.ATC_SwitcherPosition;

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_TobuATC")] = Convert.ToInt32(T_DATC.ATC_TobuATC);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_Depot")] = Convert.ToInt32(T_DATC.ATC_Depot);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_ServiceBrake")] = Convert.ToInt32(T_DATC.ATC_ServiceBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EmergencyBrake")] = Convert.ToInt32(T_DATC.ATC_EmergencyBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_EmergencyOperation")] = Convert.ToInt32(T_DATC.ATC_EmergencyOperation);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_StationStop")] = Convert.ToInt32(T_DATC.ATC_StationStop);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATC_PatternApproach")] = Convert.ToInt32(T_DATC.ATC_PatternApproach);

            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_TobuAts")] = Convert.ToInt32(TSP_ATS.ATS_TobuAts);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_ATSEmergencyBrake")] = Convert.ToInt32(TSP_ATS.ATS_ATSEmergencyBrake);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_StopAnnounce")] = Convert.ToInt32(TSP_ATS.ATS_StopAnnounce);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_EmergencyOperation")] = Convert.ToInt32(TSP_ATS.ATS_EmergencyOperation);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_Confirm")] = Convert.ToInt32(TSP_ATS.ATS_Confirm);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_60")] = Convert.ToInt32(TSP_ATS.ATS_60);
            newPanelValues[Config.PanelMap.DefaultIndexOf("ATS_15")] = Convert.ToInt32(TSP_ATS.ATS_15);

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

            //sound
            Config.SoundMap.WriteSound(sound, "Ding", (int)T_DATC.ATC_Ding);
            // パターン接近：若该音此前仍处于持续状态(上一帧写的是 Continue)而本帧要发一次提示音，先停再发（与既有语义一致）
            int patternApproachIdx = Config.SoundMap.IndexOf("PatternApproachBeep");
            var soundPlayMode = SoundPlayCommands.GetMode(patternApproachIdx >= 0 && sound != null && patternApproachIdx < sound.Count ? sound[patternApproachIdx] : 0);
            if (soundPlayMode == SoundPlayMode.Continue && T_DATC.ATC_PatternApproachBeep == SoundPlayMode.Play)
                Config.SoundMap.WriteSound(sound, "PatternApproachBeep", (int)SoundPlayMode.Stop);
            Config.SoundMap.WriteSound(sound, "PatternApproachBeep", (int)T_DATC.ATC_PatternApproachBeep);
            Config.SoundMap.WriteSound(sound, "StationStopAnnounce", (int)T_DATC.ATC_StationStopAnnounce);
            Config.SoundMap.WriteSound(sound, "Switchover", (int)Sound_Switchover);
            Config.SoundMap.WriteSound(sound, "EmergencyOperationAnnounce", (int)T_DATC.ATC_EmergencyOperationAnnounce);
        }
    }
}
