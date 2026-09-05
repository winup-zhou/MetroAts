using System.Collections.Generic;

namespace MetroAts {
    public partial class MetroAts : IPluginStateProvider {
        // ---------- IPluginStateProvider：核心自身的平行只读状态 ----------
        // 逻辑键与 [output] 段一致（key / signalsw / atotascsw / power / brake），
        // 外加核心声音输出键 keyin / keyout / signalsw_sound。
        public string PluginName => "MetroAts";

        public IReadOnlyDictionary<string, int> PanelStates => CorePanelStates;
        public IReadOnlyDictionary<string, int> SoundStates => CoreSoundStates;

        private readonly Dictionary<string, int> CorePanelStates = new Dictionary<string, int>();
        private readonly Dictionary<string, int> CoreSoundStates = new Dictionary<string, int>();

        private void UpdateCorePanelStates(int keyValue, int swValue, int atotascswValue, int powerValue, int brakeValue) {
            CorePanelStates["key"] = keyValue;
            CorePanelStates["signalsw"] = swValue;
            CorePanelStates["atotascsw"] = atotascswValue;
            CorePanelStates["power"] = powerValue;
            CorePanelStates["brake"] = brakeValue;
        }

        private void UpdateCoreSoundStates(int keyin, int keyout, int signalswSound) {
            CoreSoundStates["keyin"] = keyin;
            CoreSoundStates["keyout"] = keyout;
            CoreSoundStates["signalsw_sound"] = signalswSound;
        }

        private void ClearCoreStates() {
            CorePanelStates.Clear();
            CoreSoundStates.Clear();
        }
    }
}
