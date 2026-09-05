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

            // 信号切换分派：钥匙/档位归属由核心仲裁，激活状态据此翻转
            corePlugin.Arbitrate(this);

            if (SignalEnable) {
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
                if ((currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49)
                    || (currentSection.CurrentSignalIndex >= 50 && currentSection.CurrentSignalIndex <= 54)) {
                    Config.SoundMap.WriteSound(sound, "Warning", (int)SoundPlayMode.PlayLooping);
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
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_Power")] = Convert.ToInt32(ATS_P.P_Power || Config.PPowerAlwaysLight);
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_PatternApproach")] = Convert.ToInt32(ATS_P.P_PatternApproach);
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_BrakeActioned")] = Convert.ToInt32(ATS_P.P_BrakeActioned);
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_EBActioned")] = Convert.ToInt32(ATS_P.P_EBActioned);
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_BrakeOverride")] = Convert.ToInt32(ATS_P.P_BrakeOverride);
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_PEnable")] = Convert.ToInt32(ATS_P.P_PEnable);
            newPanelValues[Config.PanelMap.DefaultIndexOf("P_Fail")] = Convert.ToInt32(ATS_P.P_Fail);
            newPanelValues[Config.PanelMap.DefaultIndexOf("SN_Power")] = Convert.ToInt32(ATS_SN.SN_Power);
            newPanelValues[Config.PanelMap.DefaultIndexOf("SN_Action")] = Convert.ToInt32(ATS_SN.SN_Action);

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

            Config.SoundMap.WriteSound(sound, "Ding", (int)ATS_P.P_Ding);
            Config.SoundMap.WriteSound(sound, "Chime", (int)ATS_SN.SN_Chime);
            if (ATS_SN.ATSEnable) Config.SoundMap.WriteSound(sound, "Warning", (int)ATS_SN.SN_WarningBell);
        }
    }
}
