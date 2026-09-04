using BveEx.Extensions.Native;
using System.Collections.Generic;
using MetroAts;

namespace TokyuSignal {
    /// <summary>
    /// TokyuSignal 对 MetroAts 核心的设备契约实现。
    /// 设备形态：ATC-P 与 TokyuATS（档位开关切换），挂载 Tokyu 单一钥匙位置。
    /// 档位切换与区段内的模块互斥逻辑保留在 Tick 内，此处只做启用门控翻转与整体复位。
    /// </summary>
    public partial class TokyuSignal : ISignalDevice {
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

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 只做启用翻转；ATC/TokyuATS 的 Init 惰性初始化保留在 Tick 内（档位切换逻辑不变）。
            if (!SignalEnable) SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 与既有去激活动作一致：整体复位 + 清音 + 清面板灯(276/279)
            BrakeTriggered = false;
            SignalEnable = false;
            ATC.ResetAll();
            TokyuATS.ResetAll();
            if (Native.AtsSoundArray != null && Native.AtsSoundArray.Count > 256
                && Native.AtsSoundArray[256] != (int)AtsSoundControlInstruction.Stop)
                Native.AtsSoundArray[256] = (int)AtsSoundControlInstruction.Stop;
            if (Native.AtsPanelArray != null) {
                Native.AtsPanelArray[276] = 0;
                Native.AtsPanelArray[279] = 0;
            }
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // 单钥匙设备不会触发（预留钩子）
        }
    }
}
