using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System.Collections.Generic;
using MetroAts;

namespace TokyuSignal {
    /// <summary>
    /// TokyuSignal 对 MetroAts 核心的设备契约实现。
    /// 设备形态：ATC-P 与 TokyuATS（档位开关切换），挂载 Tokyu 单一钥匙位置。
    /// 档位切换与区段内的模块互斥逻辑保留在 Tick 内，此处只做启用门控翻转与整体复位。
    /// </summary>
    public partial class TokyuSignal : ISignalDevice, IATOStatusProvider {
        public string DeviceName => "TokyuSignal";

        public IReadOnlyList<KeyPosList> SupportedKeyPositions { get; } =
            new[] { KeyPosList.Tokyu };

        public bool Supports(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：key==Tokyu，sw∈{ATC,Noset,TokyuATS}
            return key == KeyPosList.Tokyu
                && (sw == SignalSWList.ATC
                 || sw == SignalSWList.Noset
                 || sw == SignalSWList.TokyuATS);
        }

        public bool IsActive => SignalEnable;
        public bool IsBraking => BrakeTriggered;

        /// <summary>
        /// ATO 可用判定（向核心上报）：ATC-P 有效照查中（ATC_ATC）且不处于非设(ATC_Noset)与构内(ATC_Depot)位置。
        /// 档位归属由核心白名单(atokeys)决定，本属性只表达本设备内部 ATC 是否处于可 ATO 状态。
        /// </summary>
        public bool IsATOAvailable {
            get { return ATC.ATCEnable && ATC.ATC_ATC && !ATC.ATC_Noset && !ATC.ATC_Depot; }
        }

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：本设备（ATC-P 类）需制动脱离 EB 位(EB=BrakeNotches+1) 才投入。
            // 条件未满足时保持待命（IsActive=false），Arbitrate 会在后续帧再次调用本方法。
            if (SignalEnable) return;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) return;
            SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 与既有去激活动作一致：整体复位 + 整体清音 + 清全部面板灯（避免钥匙拔出后遗留灯/音）
            BrakeTriggered = false;
            SignalEnable = false;
            ATC.ResetAll();
            TokyuATS.ResetAll();
            Config.SoundMap.ClearAllSound(Native.AtsSoundArray);
            Config.PanelMap.ClearAllPanel(Native.AtsPanelArray);
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // 单钥匙设备不会触发（预留钩子）
        }
    }
}
