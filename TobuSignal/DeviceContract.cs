using BveEx.Extensions.Native;
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
            // 只做启用翻转；T-DATC/TSP-ATS 的 Init 惰性初始化保留在 Tick 内（区段自动切换逻辑不变）。
            if (!SignalEnable) SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 与既有去激活动作一致：整体复位 + 清音
            BrakeTriggered = false;
            SignalEnable = false;
            T_DATC.ResetAll();
            TSP_ATS.ResetAll();
            if (Native.AtsSoundArray != null && Native.AtsSoundArray.Count > 256)
                Native.AtsSoundArray[256] = (int)AtsSoundControlInstruction.Stop;
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // 单钥匙设备不会触发（预留钩子）
        }
    }
}
