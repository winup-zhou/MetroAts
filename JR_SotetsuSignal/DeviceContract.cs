using BveEx.Extensions.Native;
using System.Collections.Generic;
using MetroAts;

namespace JR_SotetsuSignal {
    /// <summary>
    /// JR_SotetsuSignal 对 MetroAts 核心的设备契约实现（阶段 2 试点）。
    /// 设备形态：ATS-P / ATS-SN 一体化装置，可挂载 JR 与 Sotetsu 两个钥匙位置；
    /// 激活判定与既有逻辑一致（JR 不要求制动脱离 EB）。
    /// </summary>
    public partial class JR_SotetsuSignal : ISignalDevice {
        public string DeviceName => "JR_SotetsuSignal";

        public IReadOnlyList<KeyPosList> SupportedKeyPositions { get; } =
            new[] { KeyPosList.JR, KeyPosList.Sotetsu };

        public bool Supports(KeyPosList key, SignalSWList sw) {
            // 与既有逻辑一致：key∈{JR,Sotetsu} 且 sw∈{JR,Sotetsu}
            return (key == KeyPosList.JR || key == KeyPosList.Sotetsu)
                && (sw == SignalSWList.JR || sw == SignalSWList.Sotetsu);
        }

        public bool IsActive => SignalEnable;
        public bool IsBraking => BrakeTriggered;

        public void Activate(KeyPosList key, SignalSWList sw) {
            // 与既有激活逻辑一致：JR 无 EB 门控、无车门门控，直接投入。
            // ATS_P / ATS_SN 的 Init 惰性初始化保留在 Tick 的 SignalEnable 分支内执行，
            // 因此此处只置位（Activate 幂等，可被 Arbitrate 每帧重入）。
            if (!SignalEnable) SignalEnable = true;
        }

        public void Deactivate(KeyPosList key) {
            // 与既有去激活动作一致：整体复位 + 整体清音 + 清全部面板灯（避免钥匙拔出后遗留灯/音）
            BrakeTriggered = false;
            SignalEnable = false;
            ATS_P.ResetAll();
            ATS_SN.ResetAll();
            Config.SoundMap.ClearAllSound(Native.AtsSoundArray);
            Config.PanelMap.ClearAllPanel(Native.AtsPanelArray);
        }

        public void OnKeyPosChanged(KeyPosList newKey) {
            // JR ↔ Sotetsu 直通换线：本设备行为一致，无需重启（预留钩子供未来线路差异化行为）。
        }
    }
}
