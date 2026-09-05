using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System.Collections.Generic;
using MetroAts;

namespace MetroSignal {
    /// <summary>
    /// MetroSignal 对 MetroAts 核心的设备契约实现。
    /// 设备形态：CS-ATC / WS-ATC（档位开关切换），可挂载 Metro 与 ToyoKosoku（东叶高速直通）两个钥匙位置。
    /// 档位切换与区段内的模块互斥逻辑保留在 Tick 内，此处只做启用门控翻转与整体复位。
    /// 注意：SignalSWList.JR 对本设备是"非设"档（直通 JR 区间时），因此也纳入 Supports。
    /// </summary>
    public partial class MetroSignal : ISignalDevice, IATOStatusProvider {
        public string DeviceName => "MetroSignal";

        public IReadOnlyList<KeyPosList> SupportedKeyPositions { get; } =
            new[] { KeyPosList.Metro, KeyPosList.ToyoKosoku };

        public bool Supports(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：key∈{Metro,ToyoKosoku}，sw∈{ATC,InDepot,Noset,JR,WS_ATC,ATP}
            if (key != KeyPosList.Metro && key != KeyPosList.ToyoKosoku) return false;
            switch (sw) {
                case SignalSWList.ATC:
                case SignalSWList.InDepot:
                case SignalSWList.Noset:
                case SignalSWList.JR:
                case SignalSWList.WS_ATC:
                case SignalSWList.ATP:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsActive => SignalEnable;
        public bool IsBraking => BrakeTriggered;

        /// <summary>
        /// ATO 可用判定（向核心上报）：CS-ATC 有效照查中（ATC_ATC）且不处于非设(ATC_Noset)与构内(ATC_Depot)位置。
        /// 档位归属由核心白名单(atokeys)决定，本属性只表达本设备内部 ATC 是否处于可 ATO 状态。
        /// </summary>
        public bool IsATOAvailable {
            get { return CS_ATC.ATCEnable && CS_ATC.ATC_ATC && !CS_ATC.ATC_Noset && !CS_ATC.ATC_Depot; }
        }

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：本设备（ATC 类）需制动脱离 EB 位(EB=BrakeNotches+1) 才投入。
            // 条件未满足时保持待命（IsActive=false），Arbitrate 会在后续帧再次调用本方法。
            if (SignalEnable) return;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) return;
            SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 与既有去激活动作一致：整体复位 + 整体清音 + 清全部面板灯。
            // 拔出钥匙后本设备不再刷新面板，因此必须把本设备注册的全部面板/声音端子归零，
            // 避免上一个设备遗留的灯/音残留（端子经 PanelMap/SoundMap 映射，重映射后仍正确）。
            BrakeTriggered = false;
            SignalEnable = false;
            WS_ATC.ResetAll();
            CS_ATC.ResetAll();
            Config.SoundMap.ClearAllSound(Native.AtsSoundArray);
            Config.PanelMap.ClearAllPanel(Native.AtsPanelArray);
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // Metro ↔ ToyoKosoku 直通换线：本设备行为一致，无需重启（预留钩子）
        }
    }
}
