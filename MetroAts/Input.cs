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
            if (e.DefaultBrakePosition == BrakePosition.Emergency) {
                for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                    if (Config.KeyPosLists[i] == KeyPosList.None) {
                        NowKey = i;
                        break;
                    }
                }
                for (int i = 0; i < Config.SignalSWLists.Count; ++i) {
                    if (Config.SignalSWLists[i] == SignalSWList.Noset) {
                        NowSignalSW = i;
                        break;
                    }
                }
            }
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
            if (state.Speed == 0 && handles.ReverserPosition == ReverserPosition.N && handles.BrakeNotch == vehicleSpec.BrakeNotches + 1) {
                if (e.KeyName == AtsKeyName.I) {
                    if (Config.KeyPosLists[NowKey] == KeyPosList.None && NowKey > 0) {
                        NowKey--;
                        Sound_Keyin = AtsSoundControlInstruction.Play;
                    } else if(NowKey > 0) {
                        for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                            if (Config.KeyPosLists[i] == KeyPosList.None) {
                                NowKey = i;
                                break;
                            }
                        }
                        Sound_Keyout = AtsSoundControlInstruction.Play;
                    }
                } else if (e.KeyName == AtsKeyName.J) {
                    if (Config.KeyPosLists[NowKey] == KeyPosList.None && NowKey < Config.KeyPosLists.Count - 1) {
                        NowKey++;
                        Sound_Keyin = AtsSoundControlInstruction.Play;
                    } else if(NowKey < Config.KeyPosLists.Count - 1) {
                        for (int i = 0; i < Config.KeyPosLists.Count; ++i) {
                            if (Config.KeyPosLists[i] == KeyPosList.None) {
                                NowKey = i;
                                break;
                            }
                        }
                        Sound_Keyout = AtsSoundControlInstruction.Play;
                    }
                } else if (e.KeyName == AtsKeyName.G) {
                    if (Config.SignalSW_loop) {
                        NowSignalSW--;
                        NowSignalSW %= Config.SignalSWLists.Count;
                        Sound_SignalSW = AtsSoundControlInstruction.Play;
                    } else if (NowSignalSW > 0) {
                        NowSignalSW--;
                        Sound_SignalSW = AtsSoundControlInstruction.Play;
                    }
                } else if (e.KeyName == AtsKeyName.H) {
                    if (Config.SignalSW_loop) {
                        NowSignalSW++;
                        NowSignalSW %= Config.SignalSWLists.Count;
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
