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

            var KeyText = "";
            var SignalSWText = "";

            switch (Config.KeyPosLists[NowKey]) {
                case KeyPosList.None:
                    KeyText = "未挿入";
                    break;
                case KeyPosList.Tokyu:
                    KeyText = "東　急";
                    break;
                case KeyPosList.Metro:
                    KeyText = "メトロ";
                    break;
                case KeyPosList.Tobu:
                    KeyText = "東　武";
                    break;
                case KeyPosList.Seibu:
                    KeyText = "西　武";
                    break;
                case KeyPosList.ToyoKosoku:
                    KeyText = "東　葉";
                    break;
                case KeyPosList.JR:
                    KeyText = "Ｊ　Ｒ";
                    break;
                case KeyPosList.Sotetsu:
                    KeyText = "相　鉄";
                    break;
                case KeyPosList.Odakyu:
                    KeyText = "小田急";
                    break;
                default:
                    KeyText = "無　効";
                    break;
            }

            switch (Config.SignalSWLists[NowSignalSW]) {
                case SignalSWList.Noset:
                    SignalSWText = "非設";
                    break;
                case SignalSWList.InDepot:
                    SignalSWText = "構内";
                    break;
                case SignalSWList.ATC:
                    SignalSWText = "ATC";
                    break;
                case SignalSWList.Tobu:
                    SignalSWText = "東武";
                    break;
                case SignalSWList.SeibuATS:
                    SignalSWText = "西武";
                    break;
                case SignalSWList.Sotetsu:
                    SignalSWText = "相鉄";
                    break;
                case SignalSWList.JR:
                    SignalSWText = "ＪＲ";
                    break;
                case SignalSWList.TokyuATS:
                    SignalSWText = "東急ATS";
                    break;
                case SignalSWList.WS_ATC:
                    SignalSWText = "WS-ATC";
                    break;
                case SignalSWList.ATP:
                    SignalSWText = "ATP";
                    break;
                case SignalSWList.Odakyu:
                    KeyText = "小田急";
                    break;
                default:
                    SignalSWText = "無効";
                    break;
            }

            var TASCstate = isTASCenabled ? "ATO/TASC" : "手動";

            var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
            leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
            leverText.Text = $"キー:{KeyText} 保安:{SignalSWText} 運転:{TASCstate}\n{description}";
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
            
            switch (Config.KeyPosLists[NowKey]) {
                case KeyPosList.None: panel[Config.Panel_keyoutput] = 0; break;
                case KeyPosList.Metro: panel[Config.Panel_keyoutput] = 1; break;
                case KeyPosList.Tobu: panel[Config.Panel_keyoutput] = 2; break;
                case KeyPosList.Tokyu: panel[Config.Panel_keyoutput] = 3; break;
                case KeyPosList.Seibu: panel[Config.Panel_keyoutput] = 4; break;
                case KeyPosList.Sotetsu: panel[Config.Panel_keyoutput] = 5; break;
                case KeyPosList.JR: panel[Config.Panel_keyoutput] = 6; break;
                case KeyPosList.Odakyu: panel[Config.Panel_keyoutput] = 7; break;
                case KeyPosList.ToyoKosoku: panel[Config.Panel_keyoutput] = 8; break;
            }

            panel[Config.Panel_ATOTASCSWoutput] = Convert.ToInt32(isTASCenabled);
            if (!Config.SignalSW_legacyoutput) {
                panel[Config.Panel_SignalSWoutput] = (int)Config.SignalSWLists[NowSignalSW];
            } else {
                switch (Config.SignalSWLists[NowSignalSW]) {
                    case SignalSWList.TokyuATS:
                    case SignalSWList.Odakyu:
                    case SignalSWList.Sotetsu:
                    case SignalSWList.SeibuATS:
                    case SignalSWList.Tobu:
                    case SignalSWList.JR:
                    case SignalSWList.ATP:
                        panel[Config.Panel_SignalSWoutput] = 0; 
                        break;
                    case SignalSWList.WS_ATC:
                        panel[Config.Panel_SignalSWoutput] = 5;
                        break;
                    case SignalSWList.Noset:
                        panel[Config.Panel_SignalSWoutput] = Config.KeyPosLists[NowKey] == KeyPosList.Tokyu ? 4 : 3;
                        break;
                    case SignalSWList.ATC:
                        panel[Config.Panel_SignalSWoutput] = 1;
                        break;
                    case SignalSWList.InDepot:
                        panel[Config.Panel_SignalSWoutput] = 2;
                        break;
                }
            }
                

            sound[270] = (int)Sound_Keyin;
            sound[271] = (int)Sound_Keyout;
            sound[272] = (int)Sound_SignalSW;

            Sound_Keyin = Sound_Keyout = Sound_SignalSW = AtsSoundControlInstruction.Continue;
        }
    }
}
