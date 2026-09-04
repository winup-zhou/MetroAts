using BveEx.Extensions.Native;
using System.Collections.Generic;
using MetroAts;

namespace SeibuSignal {
    /// <summary>
    /// SeibuSignal 对 MetroAts 核心的设备契约实现。
    /// 设备形态：CS-ATC 与 SeibuATS（档位开关切换），挂载 Seibu 单一钥匙位置。
    /// 档位切换与区段内的模块互斥逻辑保留在 Tick 内，此处只做启用门控翻转与整体复位。
    /// </summary>
    public partial class SeibuSignal : ISignalDevice {
        public string DeviceName => "SeibuSignal";

        public IReadOnlyList<KeyPosList> SupportedKeyPositions { get; } =
            new[] { KeyPosList.Seibu };

        public bool Supports(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：SeibuATS / ATC / InDepot / Noset
            return key == KeyPosList.Seibu
                && (sw == SignalSWList.SeibuATS
                 || sw == SignalSWList.ATC
                 || sw == SignalSWList.InDepot
                 || sw == SignalSWList.Noset);
        }

        public bool IsActive => SignalEnable;
        public bool IsBraking => BrakeTriggered;

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 只做启用翻转；SeibuATS/ATC 的 Init 惰性初始化保留在 Tick 内（档位切换逻辑不变）。
            if (!SignalEnable) SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 与既有去激活动作一致：整体复位 + 清音 + 清面板灯(275/278)
            BrakeTriggered = false;
            SignalEnable = false;
            SeibuATS.ResetAll();
            ATC.ResetAll();
            if (Native.AtsSoundArray != null && Native.AtsSoundArray.Count > 256
                && Native.AtsSoundArray[256] != (int)AtsSoundControlInstruction.Stop)
                Native.AtsSoundArray[256] = (int)AtsSoundControlInstruction.Stop;
            if (Native.AtsPanelArray != null) {
                Native.AtsPanelArray[275] = 0;
                Native.AtsPanelArray[278] = 0;
            }
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // 单钥匙设备不会触发（预留钩子）
        }
    }
}
