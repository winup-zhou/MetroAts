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
                    if (!ATC.ATCEnable) panel[275] = corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot ? 1 : 0;
                    panel[278] = corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset ? 1 : 0;
                    if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49) {
                        if (!ATC.ATCEnable) {
                            if (corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                                ATC.Init(state.Time);
                            }else if(corePlugin.SignalSWPos == MetroAts.SignalSWList.SeibuATS) {
                                ATC.InitNow();
                            }
                        }
                        sound[256] = ((corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot && currentSection.CurrentSignalIndex >= 38 && currentSection.CurrentSignalIndex <= 48)
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC) ? (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
                    } else if (corePlugin.SignalSWPos != MetroAts.SignalSWList.ATC) {
                        if (ATC.ATCEnable) ATC.ResetAll();
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
                287, 291, 294, 297, 301, 304,
                285, 286, 284, 311, 310, 264, 275, 278, 271, 267, 281,
                334, 335, 336, 337, 338
            };

            int[] newPanelValues = new int[350];
            newPanelValues[287] = Convert.ToInt32(ATC.ATC_01);
            newPanelValues[291] = Convert.ToInt32(ATC.ATC_25);
            newPanelValues[294] = Convert.ToInt32(ATC.ATC_40);
            newPanelValues[297] = Convert.ToInt32(ATC.ATC_55);
            newPanelValues[301] = Convert.ToInt32(ATC.ATC_75);
            newPanelValues[304] = Convert.ToInt32(ATC.ATC_90);

            newPanelValues[285] = Convert.ToInt32(ATC.ATC_Stop);
            newPanelValues[286] = Convert.ToInt32(ATC.ATC_Proceed);

            newPanelValues[284] = Convert.ToInt32(ATC.ATC_X);

            newPanelValues[311] = ATC.ATCNeedle;
            newPanelValues[310] = Convert.ToInt32(ATC.ATCNeedle_Disappear);

            newPanelValues[264] = Convert.ToInt32(ATC.ATC_ATC);
            newPanelValues[275] = ATC.ATCEnable ? Convert.ToInt32(ATC.ATC_Depot) : 0;
            newPanelValues[278] = (ATC.ATCEnable && ATC.ATC_Noset) ? Convert.ToInt32(ATC.ATC_Noset) : 0;
            newPanelValues[271] = Convert.ToInt32(ATC.ATC_ServiceBrake);
            newPanelValues[267] = Convert.ToInt32(ATC.ATC_EmergencyBrake);
            newPanelValues[281] = Convert.ToInt32(ATC.ATC_EmergencyOperation);

            newPanelValues[334] = Convert.ToInt32(SeibuATS.ATS_Power);
            newPanelValues[335] = Convert.ToInt32(SeibuATS.ATS_EB);
            newPanelValues[336] = Convert.ToInt32(SeibuATS.ATS_Stop);
            newPanelValues[337] = Convert.ToInt32(SeibuATS.ATS_Confirm);
            newPanelValues[338] = Convert.ToInt32(SeibuATS.ATS_Limit);

            foreach (var idx in panelIndices) {
                if (needRefresh) {
                    panel[idx] = newPanelValues[idx];
                    lastPanelOutput[idx] = newPanelValues[idx];
                } else {
                    panel[idx] = lastPanelOutput[idx];
                }
            }

            sound[258] = (int)ATC.ATC_Ding;
            if (ATC.ATCEnable && ATC.ATC_Noset) { sound[256] = (int)ATC.ATC_WarningBell; }
            sound[261] = (int)ATC.ATC_EmergencyOperationAnnounce;
            sound[262] = (int)SeibuATS.ATS_StopAnnounce;
            sound[263] = (int)SeibuATS.ATS_EBAnnounce;
        }
    }
}
