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

            var TASCstate = isTASCenabled ? "ATO/TASC" : "手動";

            var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
            leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
            if(Config.atotascsw_enable)
                leverText.Text = $"キー:{KeyText} 保安:{SignalSWText} 運転:{TASCstate}\n{description}";
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
                    panel[Config.Panel_poweroutput] = AtsHandles.PowerNotch;
                    panel[Config.Panel_brakeoutput] = AtsHandles.BrakeNotch;
                } else {
                    panel[Config.Panel_poweroutput] = lastPowerNotch;
                    panel[Config.Panel_brakeoutput] = lastBrakeNotch;
                }
            }
            
            WriteKeyPosToPanel(panel);

            panel[Config.Panel_ATOTASCSWoutput] = Convert.ToInt32(isTASCenabled);
            WriteSignalSWToPanel(panel);

            sound[270] = (int)Sound_Keyin;
            sound[271] = (int)Sound_Keyout;
            sound[272] = (int)Sound_SignalSW;

            Sound_Keyin = Sound_Keyout = Sound_SignalSW = AtsSoundControlInstruction.Continue;
        }
    }
}
