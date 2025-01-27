﻿using BveEx.Extensions.Native;
using BveEx.PluginHost.Plugins;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeibuSignal {
    [Plugin(PluginType.VehiclePlugin)]
    public partial class SeibuSignal : AssemblyPluginBase {
        public override void Tick(TimeSpan elapsed) {
            var AtsHandles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.AtsHandles;
            var handles = BveHacker.Scenario.Vehicle.Instruments.AtsPlugin.Handles;
            var state = Native.VehicleState;
            var panel = Native.AtsPanelArray;
            var sound = Native.AtsSoundArray;

            int pointer = 0;
            while (sectionManager.Sections[pointer].Location < state.Location)
                pointer++;
            if (pointer >= sectionManager.Sections.Count)
                pointer = sectionManager.Sections.Count - 1;

            var currentSection = sectionManager.Sections[pointer == 0 ? 0 : pointer - 1] as Section;

            if (SignalEnable) {
                if (StandAloneMode) {
                    if (SeibuATS.ATSEnable) {
                        SeibuATS.Tick(state, sectionManager);
                        AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, SeibuATS.BrakeCommand);
                        if (SeibuATS.BrakeCommand > 0) BrakeTriggered = true;
                    } else {
                        SeibuATS.Init(state.Time);
                    }
                } else {
                    if (corePlugin.SignalSWPos == MetroAts.SignalSWList.SeibuATS) {
                        if (!SeibuATS.ATSEnable) SeibuATS.Init(state.Time);
                        if (ATC.ATCEnable)
                            ATC.ResetAll();
                    } else if (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset) {
                        if (!ATC.ATCEnable) ATC.Init(state.Time);
                        if (SeibuATS.ATSEnable)
                            SeibuATS.ResetAll();
                    }

                    if (ATC.ATCEnable) {
                        ATC.Tick(state, handles, currentSection, corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset, corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot);
                        AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, ATC.BrakeCommand);
                        if (ATC.BrakeCommand > 0) BrakeTriggered = true;
                    } else {
                        SeibuATS.Tick(state, sectionManager);
                        AtsHandles.BrakeNotch = Math.Max(AtsHandles.BrakeNotch, SeibuATS.BrakeCommand);
                        if (SeibuATS.BrakeCommand > 0) BrakeTriggered = true;
                        if (currentSection.CurrentSignalIndex >= 9 && currentSection.CurrentSignalIndex != 34 && currentSection.CurrentSignalIndex < 49
                            && (corePlugin.SignalSWPos != MetroAts.SignalSWList.ATC || corePlugin.SignalSWPos != MetroAts.SignalSWList.InDepot))
                            sound[256] = (int)AtsSoundControlInstruction.PlayLooping;
                        else sound[256] = (int)AtsSoundControlInstruction.Stop;
                    }
                }
                if (!StandAloneMode) {
                    if (corePlugin.KeyPos != MetroAts.KeyPosList.Seibu) {
                        BrakeTriggered = false;
                        SignalEnable = false;
                        SeibuATS.ResetAll();
                        ATC.ResetAll();
                    }
                }
                if (BrakeTriggered) {
                    AtsHandles.PowerNotch = 0;
                    if (handles.PowerNotch == 0) BrakeTriggered = false;
                }
            } else {
                for (var i = 329; i <= 334; ++i) panel[i] = 0;
                panel[287] = 0;
                panel[291] = 0;
                panel[294] = 0;
                panel[297] = 0;
                panel[301] = 0;
                panel[304] = 0;

                panel[285] = 0;
                panel[286] = 0;

                panel[284] = 0;

                panel[310] = 0;
                panel[309] = 0;

                panel[264] = 0;
                panel[275] = 0;
                panel[278] = 0;
                panel[271] = 0;
                panel[267] = 0;
                panel[281] = 0;
                if (StandAloneMode) {
                    if (!SignalEnable && Keyin)
                        SignalEnable = true;
                    AtsHandles.BrakeNotch = vehicleSpec.BrakeNotches + 1;
                    AtsHandles.ReverserPosition = ReverserPosition.N;
                } else {
                    Keyin = corePlugin.KeyPos == MetroAts.KeyPosList.Seibu;
                    if (!SignalEnable && Keyin && corePlugin.SignalSWPos == MetroAts.SignalSWList.SeibuATS)
                        SignalEnable = true;
                    else if (!SignalEnable && Keyin && (corePlugin.SignalSWPos == MetroAts.SignalSWList.ATC
                        || corePlugin.SignalSWPos == MetroAts.SignalSWList.InDepot || corePlugin.SignalSWPos == MetroAts.SignalSWList.Noset)
                        && handles.ReverserPosition != ReverserPosition.N && handles.BrakeNotch != vehicleSpec.BrakeNotches + 1)
                        SignalEnable = true;
                }

            }
            if (StandAloneMode) {
                var description = BveHacker.Scenario.Vehicle.Instruments.Cab.GetDescriptionText();
                leverText = (LeverText)BveHacker.MainForm.Assistants.Items.First(item => item is LeverText);
                leverText.Text = $"キー:{(Keyin ? "入" : "切")} \n{description}";
                if (isDoorOpen) AtsHandles.ReverserPosition = ReverserPosition.N;
                sound[270] = (int)Sound_Keyin;
                sound[271] = (int)Sound_Keyout;
            }
            sound[273] = (int)Sound_ResetSW;

            //panel
            panel[287] = Convert.ToInt32(ATC.ATC_01);
            panel[291] = Convert.ToInt32(ATC.ATC_25);
            panel[294] = Convert.ToInt32(ATC.ATC_40);
            panel[297] = Convert.ToInt32(ATC.ATC_55);
            panel[301] = Convert.ToInt32(ATC.ATC_75);
            panel[304] = Convert.ToInt32(ATC.ATC_90);

            panel[285] = Convert.ToInt32(ATC.ATC_Stop);
            panel[286] = Convert.ToInt32(ATC.ATC_Proceed);

            panel[284] = Convert.ToInt32(ATC.ATC_X);

            panel[310] = ATC.ATCNeedle;
            panel[309] = Convert.ToInt32(ATC.ATCNeedle_Disappear);

            panel[264] = Convert.ToInt32(ATC.ATC_ATC);
            panel[275] = Convert.ToInt32(ATC.ATC_Depot);
            panel[278] = Convert.ToInt32(ATC.ATC_Noset);
            panel[271] = Convert.ToInt32(ATC.ATC_ServiceBrake);
            panel[267] = Convert.ToInt32(ATC.ATC_EmergencyBrake);
            panel[281] = Convert.ToInt32(ATC.ATC_EmergencyOperation);

            panel[329] = Convert.ToInt32(SeibuATS.ATS_Power);
            panel[330] = Convert.ToInt32(SeibuATS.ATS_EB);
            panel[331] = Convert.ToInt32(SeibuATS.ATS_Stop);
            panel[332] = Convert.ToInt32(SeibuATS.ATS_Confirm);
            panel[333] = Convert.ToInt32(SeibuATS.ATS_Limit);
            //panel[334] = Convert.ToInt32(SeibuATS.);

            sound[258] = (int)ATC.ATC_Ding;
            if (ATC.ATCEnable) { sound[256] = (int)ATC.ATC_WarningBell; }
            sound[261] = (int)ATC.ATC_EmergencyOperationAnnounce;
            sound[262] = (int)SeibuATS.ATS_StopAnnounce;
            sound[263] = (int)SeibuATS.ATS_EBAnnounce;

            //sound reset
            Sound_Keyin = Sound_Keyout = Sound_ResetSW = AtsSoundControlInstruction.Continue;
            //handles.PowerNotch = 0;
            //handles.BrakeNotch = 0;
            //handles.ConstantSpeedMode = ConstantSpeedMode.Continue;
            //handles.ReverserPosition = ReverserPosition.N;
        }
    }
}
