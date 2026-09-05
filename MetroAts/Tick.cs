using BveEx.Extensions.PreTrainPatch;
using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MetroAts {
    public partial class MetroAts : AssemblyPluginBase {
        public override void Tick(TimeSpan elapsed) {
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;

            string KeyText = KeyDisplayText();
            string SignalSWText = SignalSWDisplayText();

            // 運転表示：手動 / ATO/TASC；模式以独立字段 モード 显示（平常/回復/遅速）
            string TASCstate = isTASCenabled ? "ATO/TASC" : "手動";

            var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
            leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
            if(Config.atotascsw_enable)
                leverText.Text = $"キー:{KeyText} 保安:{SignalSWText} 運転:{TASCstate} モード:{AtoModeText()}\n{description}";
            else
                leverText.Text = $"キー:{KeyText} 保安:{SignalSWText}\n{description}";

            if (Config.KeyPosLists[NowKey] == KeyPosList.None || !SubPluginEnabled) {
                AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                AtsHandles.ReverserPosition = ReverserPosition.N;
            }
            if(isDoorOpen) AtsHandles.ReverserPosition = ReverserPosition.N;
            SubPluginEnabled = false;

            if (!SubPluginEnabled) {
                if (state.Time.TotalMilliseconds - lastHandleOutputRefreshTime.TotalMilliseconds > Config.Panel_HandleOutputRefreshInterval) {
                    lastHandleOutputRefreshTime = state.Time;
                    lastBrakeNotch = AtsHandles.BrakeNotch;
                    lastPowerNotch = AtsHandles.PowerNotch;
                    if (Config.PanelWriteEnabled) {
                        panel[Config.Panel_poweroutput] = AtsHandles.PowerNotch;
                        panel[Config.Panel_brakeoutput] = AtsHandles.BrakeNotch;
                    }
                } else {
                    if (Config.PanelWriteEnabled) {
                        panel[Config.Panel_poweroutput] = lastPowerNotch;
                        panel[Config.Panel_brakeoutput] = lastBrakeNotch;
                    }
                }
            }
            
            WriteKeyPosToPanel(panel);

            if (Config.PanelWriteEnabled)
                panel[Config.Panel_ATOTASCSWoutput] = Convert.ToInt32(isTASCenabled);
            WriteSignalSWToPanel(panel);

            Config.SoundMap.WriteSound(sound, "keyin", (int)Sound_Keyin);
            Config.SoundMap.WriteSound(sound, "keyout", (int)Sound_Keyout);
            Config.SoundMap.WriteSound(sound, "signalsw_sound", (int)Sound_SignalSW);

            // 平行状态暴露（供其它 BveEX 插件经核心注册表只读查询）
            int keyState = Config.KeyPosLists[NowKey] == KeyPosList.None ? 0 :
                (KeyPanelOutputs.TryGetValue(KeyPos, out int kv) ? kv : 0);
            UpdateCorePanelStates(
                keyState,
                Config.SignalSW_legacyoutput ? SignalSWLegacyOutput(KeyPos, SignalSWPos) : (int)SignalSWPos,
                Convert.ToInt32(isTASCenabled),
                lastPowerNotch, lastBrakeNotch);
            CorePanelStates["ato_mode"] = ATORunningModeValue;
            UpdateCoreSoundStates((int)Sound_Keyin, (int)Sound_Keyout, (int)Sound_SignalSW);

            Sound_Keyin = Sound_Keyout = Sound_SignalSW = SoundPlayMode.Continue;
        }
    }
}
