using BveEx.Extensions.Native;
using System.Collections.Generic;
using MetroAts;

namespace OdakyuSignal {
    /// <summary>
    /// OdakyuSignal 对 MetroAts 核心的设备契约实现。
    /// 设备形态：OM-ATS / D-ATS-P（由车载手动·自动切换开关 ATS_SW 决定），挂载 Odakyu 单一钥匙位置。
    /// 启用/去启用由核心仲裁；OM/D 的内部选择与照查逻辑保留在本设备内。
    /// </summary>
    public partial class OdakyuSignal : ISignalDevice {
        public string DeviceName => "OdakyuSignal";

        public IReadOnlyList<KeyPosList> SupportedKeyPositions { get; } =
            new[] { KeyPosList.Odakyu };

        public bool Supports(KeyPosList key, SignalSWList sw) {
            return key == KeyPosList.Odakyu && sw == SignalSWList.Odakyu;
        }

        public bool IsActive => SignalEnable;
        public bool IsBraking => BrakeTriggered;

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 只做启用翻转；OM/D 模块的 Init 惰性初始化保留在 Tick 内（按 ATS_SW 调度）。
            if (!SignalEnable) SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 整体复位：清信号逻辑 + 清接口端子（347..356）
            BrakeTriggered = false;
            SignalEnable = false;
            OM_ATS.ResetAll();
            D_ATS_P.ResetAll();
            if (Native.AtsPanelArray != null) {
                for (int i = 347; i <= 356; i++)
                    if (i < Native.AtsPanelArray.Count) Native.AtsPanelArray[i] = 0;
            }
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // 单钥匙设备不会触发（预留钩子）
        }
    }
}
