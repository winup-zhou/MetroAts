using BveEx.Extensions.Native.Input;
using BveEx.Extensions.Native;
using BveEx.PluginHost;
using BveEx.PluginHost.Input;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroAts {
    public partial class MetroAts : AssemblyPluginBase {

        private void Initialize(object sender, StartedEventArgs e) {
            var panel = Native.AtsPanelArray;
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                    if (Config.KeyPosLists[i] == KeyPosList.None) {
                        NowKey = i;
                        break;
                    }
                }
                for (int i = 0; i < Config.SignalSWLists.Count; ++i) {
                    if (Config.SignalSWLists[i] == SignalSWList.Noset || Config.SignalSWLists[i] == SignalSWList.JR) {
                        NowSignalSW = i;
                        break;
                    }
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
            panel[Config.Panel_SignalSWoutput] = (int)Config.SignalSWLists[NowSignalSW];
        }
        private void DoorOpened(object sender, EventArgs e) {
            isDoorOpen = true;
        }

        private void DoorClosed(object sender, EventArgs e) {
            isDoorOpen = false;
        }

        private void KeyUp(object sender, AtsKeyEventArgs e) {
            //throw new NotImplementedException();
        }

        private void KeyDown(object sender, AtsKeyEventArgs e) {
            var state = Native.VehicleState;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            if (Math.Abs(state.Speed) == 0 && handles.ReverserPosition == ReverserPosition.N && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) {
                if (e.KeyName == AtsKeyName.I) {
                    if (Config.KeyPosLists[NowKey] == KeyPosList.None && NowKey > 0) {
                        NowKey--;
                        Sound_Keyin = AtsSoundControlInstruction.Play;
                    } else if (Config.KeyPosLists[NowKey] != KeyPosList.None) {
                        for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                            if (Config.KeyPosLists[i] == KeyPosList.None) {
                                if (NowKey > i) { 
                                    NowKey = i;
                                    Sound_Keyout = AtsSoundControlInstruction.Play;
                                }
                                break;
                            }
                        }
                        if (Config.KeyPosLists[NowKey] != KeyPosList.None && NowKey > 0) { 
                            NowKey--; 
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    }
                } else if (e.KeyName == AtsKeyName.J) {
                    if (Config.KeyPosLists[NowKey] == KeyPosList.None && NowKey < Config.KeyPosLists.Count - 1) {
                        NowKey++;
                        Sound_Keyin = AtsSoundControlInstruction.Play;
                    } else {
                        for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                            if (Config.KeyPosLists[i] == KeyPosList.None) {
                                if (NowKey < i) { 
                                    NowKey = i;
                                    Sound_Keyout = AtsSoundControlInstruction.Play;
                                }
                                break;
                            }
                        }
                        if (Config.KeyPosLists[NowKey] != KeyPosList.None && NowKey < Config.KeyPosLists.Count - 1) { 
                            NowKey++;
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    }
                } else if (e.KeyName == AtsKeyName.G) {
                    if (Config.SignalSW_loop) {
                        NowSignalSW = (NowSignalSW - 1) % Config.SignalSWLists.Count;
                        if (NowSignalSW < 0) NowSignalSW += Config.SignalSWLists.Count;
                        Sound_SignalSW = AtsSoundControlInstruction.Play;
                    } else if (NowSignalSW > 0) {
                        NowSignalSW--;
                        Sound_SignalSW = AtsSoundControlInstruction.Play;
                    }
                } else if (e.KeyName == AtsKeyName.H) {
                    if (Config.SignalSW_loop) {
                        NowSignalSW = (NowSignalSW + 1) % Config.SignalSWLists.Count;
                        Sound_SignalSW = AtsSoundControlInstruction.Play;
                    } else if (NowSignalSW < Config.SignalSWLists.Count - 1) {
                        NowSignalSW++;
                        Sound_SignalSW = AtsSoundControlInstruction.Play;
                    }
                }
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

    }
}
