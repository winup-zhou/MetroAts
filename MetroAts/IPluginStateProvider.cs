using System.Collections.Generic;

namespace MetroAts {
    /// <summary>
    /// 插件对外暴露的只读状态查询接口。
    /// 每个插件（信号子插件 / MetroPIAddon / 核心自身）实现本接口，
    /// 把本插件逐灯/逐音的逻辑状态（与面板/声音输出端子一一平行）暴露给其它 BveEX 插件读取。
    /// 状态对象由插件自行更新，本接口只读，查询方不得修改。
    /// </summary>
    /// <remarks>
    /// 运行时可发现性：各插件将自身实例注册到核心的轻量注册表
    /// （<see cref="MetroAts.RegisterPluginStateProvider"/>），其它插件持核心引用即可
    /// 经 <see cref="MetroAts.TryGetPluginState"/> 取到指定插件的状态对象，无需引用其 DLL。
    /// 核心注册表只保存引用，不做任何聚合逻辑。
    /// </remarks>
    public interface IPluginStateProvider {
        /// <summary>插件名（注册键），例如 "MetroSignal"、"TobuSignal"、"MetroPIAddon"、"MetroAts"。</summary>
        string PluginName { get; }

        /// <summary>
        /// 面板灯逻辑状态（逻辑键 → 灯值 0/1）。键与 [output] 段中可自定义的面板键一致，
        /// 每帧由插件在写面板的同时更新。
        /// </summary>
        IReadOnlyDictionary<string, int> PanelStates { get; }

        /// <summary>
        /// 声音指令逻辑状态（逻辑键 → SoundPlayMode 数值）。键与 [output] 段中可自定义的声音键一致。
        /// </summary>
        IReadOnlyDictionary<string, int> SoundStates { get; }
    }
}
