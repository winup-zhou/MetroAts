using MetroAts;

namespace TobuSignal {
    /// <summary>
    /// TobuSignal 对核心 IPluginStateProvider 的实现：
    /// 把本插件所有面板灯 / 声音输出的平行逻辑状态暴露给其它 BveEX 插件只读查询。
    /// 状态字典（Config.PanelMap.State / SoundMap.State）在每帧写输出时同步更新；
    /// 逻辑键与各插件 ini [output] 段可自定义的端子键一致。
    /// </summary>
    public partial class TobuSignal : IPluginStateProvider {
        public string PluginName => "TobuSignal";
        public System.Collections.Generic.IReadOnlyDictionary<string, int> PanelStates => Config.PanelMap.State;
        public System.Collections.Generic.IReadOnlyDictionary<string, int> SoundStates => Config.SoundMap.State;
    }
}
