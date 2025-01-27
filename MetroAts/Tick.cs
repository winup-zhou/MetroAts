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
                default:
                    SignalSWText = "無効";
                    break;
            }

            var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
            leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
            leverText.Text = $"キー:{KeyText} 保安:{SignalSWText}\n{description}";
            if (Config.KeyPosLists[NowKey] == KeyPosList.None) {
                AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                AtsHandles.ReverserPosition = ReverserPosition.N;
            }
            if(isDoorOpen) AtsHandles.ReverserPosition = ReverserPosition.N;

            sound[270] = (int)Sound_Keyin;
            sound[271] = (int)Sound_Keyout;
            sound[272] = (int)Sound_SignalSW;

            Sound_Keyin = Sound_Keyout = Sound_SignalSW = AtsSoundControlInstruction.Continue;
        }
    }
}
