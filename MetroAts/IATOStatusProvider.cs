namespace MetroAts {
    /// <summary>
    /// 可选接口：向核心上报本设备当前的 ATO（自动列车运行）可用状态。
    /// 由具备 ATC 且支持 ATO 的设备实现（当前：MetroSignal、TokyuSignal）。
    /// 核心把当前激活设备的该状态聚合为 <see cref="MetroAts.IsATOAvailable"/>，
    /// 供 MetroAtsBridge 等外部插件读取（替代其读取 ATC 面板灯端子的旧方式）。
    /// </summary>
    public interface IATOStatusProvider {
        /// <summary>
        /// 本设备当前是否 ATO 可用。
        /// 判定标准（与既有 ATC 面板逻辑一致）：ATC 有效（ATCEnable）且
        /// 不处于非设（ATC_Noset）与构内（ATC_Depot）位置；档位归属由设备自身结合核心 SignalSWPos 判断。
        /// </summary>
        bool IsATOAvailable { get; }
    }
}
