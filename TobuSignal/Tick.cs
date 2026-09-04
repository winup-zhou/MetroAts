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
                if (currentSection.CurrentSignalIndex >= 109 && currentSection.CurrentSignalIndex != 134 && currentSection.CurrentSignalIndex < 149)
                    sound[256] = corePlugin.SignalSWPos == MetroAts.SignalSWList.Tobu ?
                        (int)AtsSoundControlInstruction.Stop : (int)AtsSoundControlInstruction.PlayLooping;
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
            Sound_ResetSW = Sound_Switchover = AtsSoundControlInstruction.Continue;

            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }

        private static void UpdatePanelAndSound(IList<int> panel,IList<int> sound, TimeSpan currentTime) {
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

            // 需要刷新的 panel 项索引
            int[] panelIndices = new int[] {
                287,288,289,290,291,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,
                285,286,316,317,313,284,314,311,310,323,324,318,319,321,320,326,325,322,327,330,332,333,331,328,329
            };

            // 计算 panel 新值
            int[] newPanelValues = new int[350];
            newPanelValues[287] = Convert.ToInt32(T_DATC.ATC_01);
            newPanelValues[288] = Convert.ToInt32(T_DATC.ATC_10);
            newPanelValues[289] = Convert.ToInt32(T_DATC.ATC_15);
            newPanelValues[290] = Convert.ToInt32(T_DATC.ATC_20);
            newPanelValues[291] = Convert.ToInt32(T_DATC.ATC_25);
            newPanelValues[292] = Convert.ToInt32(T_DATC.ATC_30);
            newPanelValues[293] = Convert.ToInt32(T_DATC.ATC_35);
            newPanelValues[294] = Convert.ToInt32(T_DATC.ATC_40);
            newPanelValues[295] = Convert.ToInt32(T_DATC.ATC_45);
            newPanelValues[296] = Convert.ToInt32(T_DATC.ATC_50);
            newPanelValues[297] = Convert.ToInt32(T_DATC.ATC_55);
            newPanelValues[298] = Convert.ToInt32(T_DATC.ATC_60);
            newPanelValues[299] = Convert.ToInt32(T_DATC.ATC_65);
            newPanelValues[300] = Convert.ToInt32(T_DATC.ATC_70);
            newPanelValues[301] = Convert.ToInt32(T_DATC.ATC_75);
            newPanelValues[302] = Convert.ToInt32(T_DATC.ATC_80);
            newPanelValues[303] = Convert.ToInt32(T_DATC.ATC_85);
            newPanelValues[304] = Convert.ToInt32(T_DATC.ATC_90);
            newPanelValues[305] = Convert.ToInt32(T_DATC.ATC_95);
            newPanelValues[306] = Convert.ToInt32(T_DATC.ATC_100);
            newPanelValues[307] = Convert.ToInt32(T_DATC.ATC_105);
            newPanelValues[308] = Convert.ToInt32(T_DATC.ATC_110);

            if (!Config.SeparateATCGRlamp) {
                newPanelValues[285] = Convert.ToInt32(T_DATC.ATC_Stop);
                newPanelValues[286] = Convert.ToInt32(T_DATC.ATC_Proceed);
            } else {
                newPanelValues[316] = Convert.ToInt32(T_DATC.ATC_Stop);
                newPanelValues[317] = Convert.ToInt32(T_DATC.ATC_Proceed);
            }

            newPanelValues[313] = Convert.ToInt32(T_DATC.ATC_P);
            newPanelValues[284] = Convert.ToInt32(T_DATC.ATC_X);

            newPanelValues[314] = T_DATC.ORPNeedle;
            newPanelValues[311] = T_DATC.ATCNeedle;
            newPanelValues[310] = Convert.ToInt32(T_DATC.ATCNeedle_Disappear);
            newPanelValues[323] = T_DATC.ATC_EndPointDistance;
            newPanelValues[324] = T_DATC.ATC_SwitcherPosition;

            newPanelValues[318] = Convert.ToInt32(T_DATC.ATC_TobuATC);
            newPanelValues[319] = Convert.ToInt32(T_DATC.ATC_Depot);
            newPanelValues[321] = Convert.ToInt32(T_DATC.ATC_ServiceBrake);
            newPanelValues[320] = Convert.ToInt32(T_DATC.ATC_EmergencyBrake);
            newPanelValues[326] = Convert.ToInt32(T_DATC.ATC_EmergencyOperation);
            newPanelValues[325] = Convert.ToInt32(T_DATC.ATC_StationStop);
            newPanelValues[322] = Convert.ToInt32(T_DATC.ATC_PatternApproach);

            newPanelValues[327] = Convert.ToInt32(TSP_ATS.ATS_TobuAts);
            newPanelValues[330] = Convert.ToInt32(TSP_ATS.ATS_ATSEmergencyBrake);
            newPanelValues[332] = Convert.ToInt32(TSP_ATS.ATS_StopAnnounce);
            newPanelValues[333] = Convert.ToInt32(TSP_ATS.ATS_EmergencyOperation);
            newPanelValues[331] = Convert.ToInt32(TSP_ATS.ATS_Confirm);
            newPanelValues[328] = Convert.ToInt32(TSP_ATS.ATS_60);
            newPanelValues[329] = Convert.ToInt32(TSP_ATS.ATS_15);

            // 刷新逻辑
            foreach (var idx in panelIndices) {
                if (needRefresh) {
                    panel[idx] = newPanelValues[idx];
                    lastPanelOutput[idx] = newPanelValues[idx];
                } else {
                    panel[idx] = lastPanelOutput[idx];
                }
            }

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
