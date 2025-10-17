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
                        NoneKeyPos = NowKey = i;
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
                        if (LineDef != KeyPosList.None && Config.EnforceKeyPos) {
                            for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                                if (Config.KeyPosLists[i] == LineDef) {
                                    if (NowKey > i) {
                                        NowKey = i;
                                        Sound_Keyin = AtsSoundControlInstruction.Play;
                                    }
                                    break;
                                }
                            }
                        } else {
                            NowKey--;
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    } else {
                        if (NowKey > NoneKeyPos) {
                            for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                                if (Config.KeyPosLists[i] == KeyPosList.None) {
                                    if (NowKey > i) {
                                        NowKey = i;
                                        Sound_Keyout = AtsSoundControlInstruction.Play;
                                    }
                                    break;
                                }
                            }
                        } else if(NowKey > 0) {
                            NowKey--;
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    }
                    
                } else if (e.KeyName == AtsKeyName.J) {
                    if (Config.KeyPosLists[NowKey] == KeyPosList.None && NowKey < Config.KeyPosLists.Count - 1) {
                        if (LineDef != KeyPosList.None && Config.EnforceKeyPos) {
                            for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                                if (Config.KeyPosLists[i] == LineDef) {
                                    if (NowKey < i) {
                                        NowKey = i;
                                        Sound_Keyin = AtsSoundControlInstruction.Play;
                                    }
                                    break;
                                }
                            }
                        } else {
                            NowKey++;
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    } else {
                        if (NowKey < NoneKeyPos) {
                            for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                                if (Config.KeyPosLists[i] == KeyPosList.None) {
                                    if (NowKey < i) {
                                        NowKey = i;
                                        Sound_Keyout = AtsSoundControlInstruction.Play;
                                    }
                                    break;
                                }
                            }
                        } else if (NowKey < Config.KeyPosLists.Count - 1) {
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

        private void SetBeaconData(object sender, BeaconPassedEventArgs e) {
            var state = Native.VehicleState;
            if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
            switch (e.Type) {
                case 42:
                    switch (e.Optional / 10) {
                        default: LineDef = KeyPosList.None; break;
                        case 1: LineDef = KeyPosList.Metro; break;
                        case 2: LineDef = KeyPosList.Tobu; break;
                        case 3: LineDef = KeyPosList.Tokyu; break;
                        case 4: LineDef = KeyPosList.Seibu; break;
                        case 5: LineDef = KeyPosList.Sotetsu; break;
                        case 6: LineDef = KeyPosList.JR; break;
                        case 7: LineDef = KeyPosList.Odakyu; break;
                        case 8: LineDef = KeyPosList.ToyoKosoku; break;
                    }
                    Direction = e.Optional % 10;
                    break;
            }
        }

        private void SetVehicleSpec(object sender, EventArgs e) {
            vehicleSpec = Native.VehicleSpec;
        }

    }
}
