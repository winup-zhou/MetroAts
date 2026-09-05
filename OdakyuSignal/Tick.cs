using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class OdakyuSignal : AssemblyPluginBase {
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

            // 信号切换分派：钥匙/档位归属由核心仲裁，激活状态据此翻转
            corePlugin.Arbitrate(this);

            if (SignalEnable) {
                if (!corePlugin.SubPluginEnabled) corePlugin.SubPluginEnabled = true;

                // 车载 OM-ATS / D-ATS-P 手动·自动切换开关（ATS_SW）：
                //   Auto    = 自动（OM/D 均启用，各自按地上子工作）
                //   OM_ATS  = 手动固定 OM-ATS
                //   D_ATS_P = 手动固定 D-ATS-P
                bool omEnabled = ATS_Switch != ATS_SW.D_ATS_P;
                bool dEnabled = ATS_Switch != ATS_SW.OM_ATS;
                if (omEnabled && !OM_ATS.ATSEnable) OM_ATS.Init(state.Time);
                else if (!omEnabled && OM_ATS.ATSEnable) OM_ATS.Disable();
                if (dEnabled && !D_ATS_P.ATSEnable) D_ATS_P.Init(state.Time);
                else if (!dEnabled && D_ATS_P.ATSEnable) D_ATS_P.Disable();

                if (OM_ATS.ATSEnable) OM_ATS.Tick(state);
                if (D_ATS_P.ATSEnable) D_ATS_P.Tick(state, currentSection, nextSection, handles);

                int brake = 0;
                if (OM_ATS.ATSEnable) brake = Math.Max(brake, OM_ATS.BrakeCommand);
                if (D_ATS_P.ATSEnable) brake = Math.Max(brake, D_ATS_P.BrakeCommand);
                if (brake > 0) {
                    if (AtsHandles.BrakeNotch < vehicleSpec.BrakeNotches + 2)
                        AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, brake);
                    else AtsHandles.BrakeNotch = brake;
                    BrakeTriggered = true;
                }
                if (BrakeTriggered) {
                    AtsHandles.PowerNotch = 0;
                    if (handles.PowerNotch == 0) BrakeTriggered = false;
                }
                UpdatePanelAndSound(panel, sound);
                panel[Config.Panel_poweroutput] = AtsHandles.PowerNotch;
                panel[Config.Panel_brakeoutput] = AtsHandles.BrakeNotch;
            }
            // 非激活：启用/去启用已由 corePlugin.Arbitrate(this) 管理（Activate/Deactivate）
        }

        private static void UpdatePanelAndSound(IList<int> panel, IList<int> sound) {
            // 面板值经 PanelMap 映射到"实际（可被 [output] 段覆盖的）端子号"，并记录平行状态。
            // 接口定义（Google Sheets: MetroAts 预留接口）默认端子：
            //   347=OM-ATS  348=D-ATS-P  349=パターン接近  350=動作  351=速度注意
            //   352=無信号  353=P非設     354=非常運転      355=EB    356=P地上子
            Config.PanelMap.WritePanel(panel, "OM_ATS", OM_ATS.ATSEnable ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "D_ATS_P", D_ATS_P.ATSEnable ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_PatternApproach", D_ATS_P.ATS_PatternApproach ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_Triggered", (OM_ATS.ATS_Triggered || D_ATS_P.ATS_Triggered) ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_SpeedCaution", (OM_ATS.ATS_SpeedCaution || D_ATS_P.ATS_SpeedCaution) ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_NoSignal", D_ATS_P.ATS_NoSignal ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_Noset", D_ATS_P.ATS_Noset ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_EmergencyOperation", (OM_ATS.ATS_EmergencyOperation || D_ATS_P.ATS_EmergencyOperation) ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "EB", (OM_ATS.ATS_EmergencyOperation || D_ATS_P.EB_NeedConfirm || D_ATS_P.ATS_EmergencyOperation) ? 1 : 0);
            Config.PanelMap.WritePanel(panel, "ATS_Pbeacon", D_ATS_P.ATS_Pbeacon ? 1 : 0);
            // 小田急声道端子暂未在接口定义中提供，避免与既有声道冲突故不写 sound
            _ = sound;
        }
    }
}
