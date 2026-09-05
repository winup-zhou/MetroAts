using BveEx.Extensions.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MetroAts {
    /// <summary>
    /// 输出端子映射与逻辑状态记录器（供各插件统一使用）。
    ///
    /// 作用一（可自定义键值）：把插件的每盏面板灯 / 每条声音输出用一个"逻辑键"表示，
    /// 逻辑键默认映射到当前硬编码端子号，可通过 INI 的 [output] 段覆盖（键=逻辑名、值=端子号），
    /// 例如：<c>[output]&#10;ATC_Stop = 285</c>。
    ///
    /// 作用二（平行状态暴露）：插件每帧写面板/声音时，同时记录该逻辑键的当前值；
    /// 通过 <see cref="IPluginStateProvider.PanelStates"/> / <see cref="IPluginStateProvider.SoundStates"/>
    /// 供其它 BveEX 插件只读查询，无需解析物理面板数组。
    /// </summary>
    public sealed class OutputIndexMap {
        private readonly Dictionary<string, int> _index = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _default = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _state = new Dictionary<string, int>();

        /// <summary>逻辑键 → 当前（默认或已覆盖）端子号。</summary>
        public IReadOnlyDictionary<string, int> Index => _index;

        /// <summary>逻辑键 → 内置默认端子号（ini 覆盖前）。</summary>
        public IReadOnlyDictionary<string, int> Default => _default;

        /// <summary>逻辑键 → 最近一次写入的灯值/声音指令。</summary>
        public IReadOnlyDictionary<string, int> State => _state;

        /// <summary>
        /// 是否仍把本表面板键写入 BVE 物理面板数组（默认 true）。
        /// 关闭后仅记录逻辑状态、不写物理数组——暴露模式仍可用（其它插件可经核心读取状态），
        /// 但本插件不再点亮/清空 BVE 面板端子。由各插件 INI 的 [output]writepanel 配置。
        /// </summary>
        public bool PanelWriteEnabled { get; set; } = true;

        /// <summary>
        /// 是否仍把本表声音键写入 BVE 物理声音数组（默认 true）。
        /// 由各插件 INI 的 [output]writesound 配置。
        /// </summary>
        public bool SoundWriteEnabled { get; set; } = true;

        /// <summary>
        /// 注册一批逻辑键及其默认端子号。重复注册同键将覆盖默认值（用于多段合并）。
        /// </summary>
        public void RegisterDefault(string key, int defaultIndex) {
            if (!_index.ContainsKey(key)) _index[key] = defaultIndex;
            _default[key] = defaultIndex;
        }

        /// <summary>按逻辑键取当前端子号（未注册返回 -1）。</summary>
        public int IndexOf(string key) {
            return _index.TryGetValue(key, out int i) ? i : -1;
        }

        /// <summary>按逻辑键取内置默认端子号（未注册返回 -1）。</summary>
        public int DefaultIndexOf(string key) {
            return _default.TryGetValue(key, out int i) ? i : -1;
        }

        /// <summary>按逻辑键取最近写入值（未写过返回 0）。</summary>
        public int ValueOf(string key) {
            return _state.TryGetValue(key, out int v) ? v : 0;
        }

        /// <summary>尝试读某键当前端子号。</summary>
        public bool TryGetIndex(string key, out int index) {
            return _index.TryGetValue(key, out index);
        }

        /// <summary>
        /// 覆盖若干键的端子号（来自 INI）。只覆盖已注册的键；未注册键忽略。
        /// </summary>
        public void Override(IEnumerable<KeyValuePair<string, int>> overrides) {
            foreach (var kv in overrides) {
                if (_index.ContainsKey(kv.Key)) _index[kv.Key] = kv.Value;
            }
        }

        /// <summary>
        /// 写面板灯：把 value 写到 <paramref name="panel"/> 的当前映射端子，并记录逻辑状态。
        /// 若该键未注册或端子号越界则只记录状态、不写面板（安全容错）。
        /// </summary>
        public void WritePanel(IList<int> panel, string key, int value) {
            Record(key, value);
            if (!PanelWriteEnabled) return;
            if (_index.TryGetValue(key, out int idx) && idx >= 0 && panel != null && idx < panel.Count)
                panel[idx] = value;
        }

        /// <summary>写声音指令（同 <see cref="WritePanel"/>）。</summary>
        public void WriteSound(IList<int> sound, string key, int value) {
            Record(key, value);
            if (!SoundWriteEnabled) return;
            if (_index.TryGetValue(key, out int idx) && idx >= 0 && sound != null && idx < sound.Count)
                sound[idx] = value;
        }

        /// <summary>
        /// 只记录逻辑状态、不写物理数组。供"集中式数组刷新/LCD 缓存"型输出在帧末统一落地前记录状态。
        /// </summary>
        public void Record(string key, int value) {
            _state[key] = value;
        }

        /// <summary>
        /// 把本表全部已注册面板键写为 0（熄灯），并同步记录状态。
        /// 供设备去激活（钥匙拔出）时整体复位使用；端子号经本表映射，重映射后仍正确。
        /// </summary>
        public void ClearAllPanel(IList<int> panel) {
            foreach (var key in _index.Keys)
                WritePanel(panel, key, 0);
        }

        /// <summary>
        /// 把本表全部已注册声音键写为 Stop（停音），并同步记录状态。
        /// 供设备去激活（钥匙拔出）时整体复位使用。
        /// </summary>
        public void ClearAllSound(IList<int> sound) {
            const int stop = (int)SoundPlayMode.Stop;
            foreach (var key in _index.Keys)
                WriteSound(sound, key, stop);
        }

        /// <summary>清空所有记录（Dispose 用）。</summary>
        public void Clear() {
            _index.Clear();
            _state.Clear();
        }

        #region INI 读取

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault,
            StringBuilder lpReturnedString, int nSize, string lpFileName);

        private const int buffer_size = 4096;

        /// <summary>
        /// 从 <paramref name="iniPath"/> 的 <paramref name="section"/> 段读取全部已注册键的覆盖值。
        /// 段内每行形如 <c>逻辑键 = 端子号</c>。返回成功解析的 (键, 新值)。
        /// </summary>
        public static List<KeyValuePair<string, int>> ReadSection(string iniPath, string section, IEnumerable<string> knownKeys) {
            var result = new List<KeyValuePair<string, int>>();
            if (string.IsNullOrEmpty(iniPath) || !System.IO.File.Exists(iniPath)) return result;

            foreach (var key in knownKeys) {
                var ret = new StringBuilder(buffer_size);
                int read = GetPrivateProfileString(section, key, "", ret, buffer_size, iniPath);
                if (read > 0 && read < buffer_size - 1) {
                    if (int.TryParse(ret.ToString().Trim(), out int val)) {
                        result.Add(new KeyValuePair<string, int>(key, val));
                    }
                }
            }
            return result;
        }

        #endregion
    }
}
