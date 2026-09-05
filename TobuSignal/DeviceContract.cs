using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System.Collections.Generic;
using MetroAts;

namespace TobuSignal {
    /// <summary>
    /// TobuSignal 对 MetroAts 核心的设备契约实现。
    /// 设备形态：T-DATC / TSP-ATS（依区段自动切换，一体装置），挂载 Tobu 单一钥匙位置。
    /// 内部"按区段在 T-DATC/TSP-ATS 间自动切换"的逻辑保留在 Tick 内，此处只做启用门控翻转。
    /// </summary>
    public partial class TobuSignal : ISignalDevice {
        public string DeviceName => "TobuSignal";

        public IReadOnlyList<KeyPosList> SupportedKeyPositions { get; } =
            new[] { KeyPosList.Tobu };

        public bool Supports(KeyPosList key, SignalSWList sw) {
            return key == KeyPosList.Tobu && sw == SignalSWList.Tobu;
        }

        public bool IsActive => SignalEnable;
        public bool IsBraking => BrakeTriggered;

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：本设备（T-DATC/TSP-ATS 一体）需制动脱离 EB 位(EB=BrakeNotches+1) 才投入。
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
            T_DATC.ResetAll();
            TSP_ATS.ResetAll();
            Config.SoundMap.ClearAllSound(Native.AtsSoundArray);
            Config.PanelMap.ClearAllPanel(Native.AtsPanelArray);
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // 单钥匙设备不会触发（预留钩子）
        }
    }
}
