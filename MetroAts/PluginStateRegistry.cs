using System;
using System.Collections.Generic;

namespace MetroAts {
    public partial class MetroAts {
        /// <summary>
        /// 已注册插件状态提供者的轻量注册表（插件名 → 状态对象）。
        /// 只保存各插件注册的引用，不做任何聚合/转发逻辑；
        /// 查询方持核心引用即可经 <see cref="TryGetPluginState"/> 发现任意已注册插件的只读状态。
        /// </summary>
        private readonly Dictionary<string, IPluginStateProvider> pluginStates = new Dictionary<string, IPluginStateProvider>();

        /// <summary>注册一个插件状态提供者（同名重复注册会覆盖）。</summary>
        public void RegisterPluginStateProvider(IPluginStateProvider provider) {
            if (provider is null) throw new ArgumentNullException(nameof(provider));
            pluginStates[provider.PluginName] = provider;
        }

        /// <summary>注销插件状态提供者。</summary>
        public void UnregisterPluginStateProvider(IPluginStateProvider provider) {
            if (provider is null) return;
            if (pluginStates.TryGetValue(provider.PluginName, out var cur) && ReferenceEquals(cur, provider))
                pluginStates.Remove(provider.PluginName);
        }

        /// <summary>尝试按插件名取只读状态对象（例如 "MetroSignal"、"TobuSignal"、"MetroAts"）。</summary>
        public bool TryGetPluginState(string pluginName, out IPluginStateProvider provider) {
            return pluginStates.TryGetValue(pluginName, out provider);
        }

        /// <summary>已注册的插件名集合。</summary>
        public IEnumerable<string> RegisteredPluginStateNames {
            get { return pluginStates.Keys; }
        }

        private void ClearPluginStates() {
            pluginStates.Clear();
        }
    }
}
