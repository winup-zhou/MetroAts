using MetroAts;

namespace TokyuSignal {
    /// <summary>
    /// TokyuSignal 对核心 IPluginStateProvider 的实现：
    /// 把本插件所有面板灯 / 声音输出的平行逻辑状态暴露给其它 BveEX 插件只读查询。
    /// </summary>
    public partial class TokyuSignal : IPluginStateProvider {
        public string PluginName => "TokyuSignal";
        public System.Collections.Generic.IReadOnlyDictionary<string, int> PanelStates => Config.PanelMap.State;
        public System.Collections.Generic.IReadOnlyDictionary<string, int> SoundStates => Config.SoundMap.State;
    }
}
