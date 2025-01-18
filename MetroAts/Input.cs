using BveEx.PluginHost;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using SlimDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroAts {
    public partial class MetroAts : AssemblyPluginBase {
        private void OnA1Pressed(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void OnA2Pressed(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        private void OnB1Pressed(object sender, EventArgs e) {
            TSP_ATS.OnB1Pressed(sender, e);
            ATS_P_SN.OnB1Pressed(sender, e);
        }

        private void OnB2Pressed(object sender, EventArgs e) {
            ATS_P_SN.OnB2Pressed(sender, e);
        }

        private void OnGPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (SignalMode != 4) {
                    ATCCgS.Play();
                    SignalMode += 1;
                }
            }
        }

        private void OnHPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (SignalMode != 0) {
                    ATCCgS.Play();
                    SignalMode -= 1;
                }
            }
        }

        //
        private void OnIPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (KeyPosition > 0) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition = 0;
                    KeyOff.Play();
                } else if (KeyPosition == 0) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition = -1;
                    KeyOn.Play();
                }
            }
        }

        private void OnJPressed(object sender, EventArgs e) {
            if (CanSwitch) {
                if (KeyPosition == -1) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition = 0;
                    KeyOff.Play();
                } else if (KeyPosition != 4) {
                    if (SignalEnable) SignalEnable = false;
                    KeyPosition += 1;
                    KeyOn.Play();
                }
            }
        }

        private void Initialize(BveEx.PluginHost.Native.StartedEventArgs e) {
            if (e.DefaultBrakePosition == BrakePosition.Removed) SignalEnable = false;
            T_DATC.Initialize(e);
            TSP_ATS.Initialize(e);
            ATC.Initialize(e);
            ATS_P_SN.Initialize(e);
        }

        private void DoorOpened(BveEx.PluginHost.Native.DoorEventArgs e) {
            T_DATC.DoorOpened(e);
            TSP_ATS.DoorOpened(e);
            ATC.DoorOpened(e);
        }

        private void BeaconPassed(BveEx.PluginHost.Native.BeaconPassedEventArgs e) {
            T_DATC.BeaconPassed(e);
            TSP_ATS.BeaconPassed(e);
            ATC.BeaconPassed(e);
            ATS_P_SN.BeaconPassed(e);
        }

        private void OnScenarioCreated(ScenarioCreatedEventArgs e) {
            sectionManager = e.Scenario.SectionManager;
        }
    }
}
