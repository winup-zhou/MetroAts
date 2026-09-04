using System.Collections.Generic;

namespace MetroAts {
    /// <summary>
    /// 车载保安装置（设备）对 MetroAts 核心暴露的执行契约。
    /// 设备 = 现实中可独立安装的一台保安装置（对应一个信号子插件 DLL）；
    /// 一个设备可挂载多个钥匙位置（直通线路，如 JR 与 Sotetsu），
    /// 设备内部可包含多个信号制式模式（一体化 / 开关切换 / 自动切换），这些模式对核心不可见。
    /// </summary>
    public interface ISignalDevice {
        /// <summary>
        /// 设备名，例如 "JR_SotetsuSignal"。
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// 该设备可挂载的钥匙位置（线路）列表，例如 { JR, Sotetsu } 或 { Metro, ToyoKosoku }。
        /// </summary>
        IReadOnlyList<KeyPosList> SupportedKeyPositions { get; }

        /// <summary>
        /// 判断给定的钥匙位置与信号档位组合是否归本设备受理。
        /// 本方法只回答"这个档位组合归不归你管"，不得包含任何制动(EB)、车门、
        /// 区段/信标类领域判断 —— 这些由 Activate 与设备自身 Tick 处理。
        /// </summary>
        /// <param name="key">当前钥匙位置（线路）。</param>
        /// <param name="sw">当前信号选择开关档位。</param>
        /// <returns>本设备是否受理该组合。</returns>
        bool Supports(KeyPosList key, SignalSWList sw);

        /// <summary>
        /// 核心在 (key, sw) 归本设备受理且钥匙已插入时调用（每帧可能重复调用，必须幂等）。
        /// 本设备应在此把 <see cref="IsActive"/> 翻转为 true 并完成启动；
        /// 但若本设备存在运行前置条件（例如按自身规则需要制动已脱离 EB —— 各设备要求不同，
        /// 与既有激活逻辑保持一致），条件未满足时应保持待命（IsActive 保持 false），
        /// 待后续帧条件满足时再真正生效。不要在本方法内做任何复位/清除操作。
        /// </summary>
        /// <param name="key">激活时的钥匙位置（线路）。</param>
        /// <param name="sw">激活时的信号档位。</param>
        void Activate(KeyPosList key, SignalSWList sw);

        /// <summary>
        /// 核心判定当前 (key, sw) 组合已不再归本设备（钥匙拔出或档位移出合法集）时调用
        /// （仅在从激活变为失活时调用一次）。设备应在此完成全部模块复位并停止施加制动，
        /// 与既有逻辑的去激活动作（ResetAll、清音、清面板灯）保持一致。
        /// </summary>
        /// <param name="key">失活时的钥匙位置（当前钥匙）。</param>
        void Deactivate(KeyPosList key);

        /// <summary>
        /// 本设备保持激活期间钥匙位置发生变化（直通换线，如 JR→Sotetsu、Metro→ToyoKosoku）时调用。
        /// 设备可据此切换线路相关的行为，无需重启整个设备。
        /// </summary>
        /// <param name="newKey">新的钥匙位置（线路）。</param>
        void OnKeyPosChanged(KeyPosList newKey);

        /// <summary>
        /// 本设备当前是否处于激活（工作）状态。
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 本设备当前是否正在实施制动。
        /// </summary>
        bool IsBraking { get; }
    }
}
