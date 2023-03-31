using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

namespace TobuAts
{
    public static partial class TobuAts
    {
        private static AtsHandles lastHandle1, lastHandle2, lastHandle3;
        public static int SigType;
        private static int CgSType;
        private static bool ResetStop;
        //public static int MetroPluginHandleUpdate, AutopilotHandleUpdate, NotchnumberPluginHandleUpdate;
        [DllExport(CallingConvention.StdCall)]
        public static AtsHandles Elapse(AtsVehicleState state, IntPtr hPanel, IntPtr hSound)
        {
            var panel = new AtsIoArray(hPanel);
            var sound = new AtsIoArray(hSound);
            var handles = new AtsHandles
            {
                Power = pPower,
                Brake = pBrake,
                Reverser = pReverser,
                ConstantSpeed = AtsCscInstruction.Continue
            };

            if (AutopilotLoaded)
            {
                if(!handles.Equals(lastHandle1))
                {
                    lastHandle1 = handles;
                    AutopilotPlugin.SetBrake(handles.Brake);
                    AutopilotPlugin.SetPower(handles.Power);
                    AutopilotPlugin.SetReverser(handles.Reverser);
                }
                handles = AutopilotPlugin.Elapse(state, hPanel, hSound);
            }

            var CSC50THandle = CSC50TLoaded ? CSC50TPlugin.Elapse(state, hPanel, hSound) : new AtsHandles
            {
                Power = pPower,
                Brake = pBrake,
                Reverser = pReverser,
                ConstantSpeed = AtsCscInstruction.Continue
            };

            NowGamelocation = state.Location;
            NowGameTime = state.Time;
            NowVehicleSpeed = state.Speed;
            if (ResetStop)
            {
                TobuSig.NextStop = false;
                ResetStop = false;
            }

            if (panel[92] == 2 && panel[72] == 0)//東武キー、CgS ATS位置の時のみ割込み
            {
                var lastCgSType = CgSType;
                CgSType = 1;
                var lastSigType = SigType;//信号タイプを代入
                if (lastCgSType == 0) TobuSig.Ding = true;//スイッチ切換時、Dingをオン
                if (!DoorClosed) handles.Reverser = 0;
                if (TobuSig.NowSig >= 9 && TobuSig.NowSig <= 49)//ATC信号の時
                {
                    SigType = 1;
                    if (!handles.Equals(lastHandle2))//ハンドル位置が変更されたとき
                    {
                        lastHandle2 = handles;
                        MetroPlugin.SetBrake(handles.Brake);
                        MetroPlugin.SetPower(handles.Power);
                        MetroPlugin.SetReverser(handles.Reverser);
                    }
                    MetroPlugin.Elapse(state, hPanel, hSound);
                    var MonitorSpeed = TobuSig.MonitorSpeed(state.Location, state.Speed);
                    var ATCPatternLimit = TobuSig.ATCPattern.Limit;
                    int i41;
                    for (i41 = 1; i41 <= 44; i41 = i41 + 1)
                        panel[i41] = 0;

                    panel[74] = 1;
                    panel[134] = TobuSig.Plamp ? 1 : 0;
                    sound[115] = TobuSig.Ding ? 1 : 2;
                    if (TobuSig.Ding)
                        TobuSig.Ding = false;
                    sound[30] = DoorClosed ? 1 : 2;//東武戸閉のやつ　必要？
                    sound[27] = (handles.Brake == vehicleSpec.BrakeNotches + 1 && state.Speed > 5) ? 1 : 2;//EB放送
                    sound[117] = TobuSig.NextStop ? 1 : 2;
                    panel[252] = TobuSig.NextStop ? 1 : 0;
                    panel[101] = TobuSig.ATCLimit[TobuSig.NowSig] == -2 ? 1 : 0;
                    panel[103] = panel[101] == 1 ? 1 : 0;
                    panel[127] = (int)ATCPatternLimit;
                    panel[130] = ATCPatternLimit == 0 ? 0 : (state.Time % 500 < 250 ? TobuSig.TrackPos : 0);
                    if (TobuSig.CurrentDis <= TobuSig.MaxDis&&!TobuSig.inDepot)
                    {
                        if (TobuSig.CurrentDis < 200) panel[129] = 0;
                        else if (TobuSig.CurrentDis >= 200 && TobuSig.CurrentDis < 400) panel[129] = 1;
                        else if (TobuSig.CurrentDis >= 400 && TobuSig.CurrentDis < 600) panel[129] = 2;
                        else if (TobuSig.CurrentDis >= 600 && TobuSig.CurrentDis < 800) panel[129] = 3;
                        else if (TobuSig.CurrentDis >= 800 && TobuSig.CurrentDis < 1000) panel[129] = 4;
                        else if (TobuSig.CurrentDis >= 1000 && TobuSig.CurrentDis < 1200) panel[129] = 5;
                        else if (TobuSig.CurrentDis >= 1200 && TobuSig.CurrentDis < 1400) panel[129] = 6;
                        else if (TobuSig.CurrentDis > 1400 && TobuSig.CurrentDis < 1600) panel[129] = 7;
                        else if (TobuSig.CurrentDis > 1600) panel[129] = 0;
                    } else panel[129] = 0;
                    panel[132] = ATCPatternLimit > 0 ? 1 : 0;
                    panel[131] = ATCPatternLimit > 0 ? 0 : 1;
                    panel[128] = (MonitorSpeed - state.Speed < 5 && MonitorSpeed > 0) ? 1 : 0;
                    sound[116] = (MonitorSpeed - state.Speed < 5 && MonitorSpeed > 0) ? 1 : 2;
                    panel[135] = Convert.ToInt32(MonitorSpeed * 10);
                    panel[75] = TobuSig.inDepot ? 1 : 0;
                    if(state.Speed > MonitorSpeed)
                    {
                        panel[76] = 0;
                        panel[77] = 1;
                        if (state.Speed - MonitorSpeed < 1)
                        {
                            pPower = handles.Power = 0;
                            handles.Brake = Math.Max(4, pBrake);
                        }
                        else
                        {
                            pPower = handles.Power = 0;
                            handles.Brake = Math.Max(vehicleSpec.BrakeNotches, pBrake);
                        }
                    }
                    else if(TobuSig.ATCLimit[TobuSig.NowSig] == -2 || state.Speed > TobuSig.InvisiablePattern.AtLocation(state.Location,-Config.EBDec))
                    {
                        pPower = handles.Power = 0;
                        handles.Brake = Math.Max(vehicleSpec.BrakeNotches + 1, pBrake);
                        panel[77] = panel[76] = 1;
                    }
                    else if (MonitorSpeed == 0)
                    {
                        pPower = handles.Power = 0;
                        handles.Brake = Math.Max(vehicleSpec.BrakeNotches, pBrake);
                        panel[76] = 0;
                        panel[77] = 1;
                    }
                    else
                    {
                        panel[76] = panel[77] = 0;
                    }
                    sound[118] = SigType != lastSigType ? 1 : 2;
                }
                else//ATS信号の時
                {
                    SigType = 2;
                    panel[74] = 0;
                    panel[127] = 0;
                    if (!handles.Equals(lastHandle2))
                    {
                        lastHandle2 = handles;
                        MetroPlugin.SetBrake(handles.Brake);
                        MetroPlugin.SetPower(handles.Power);
                        MetroPlugin.SetReverser(handles.Reverser);
                    }
                    handles = MetroPlugin.Elapse(state, hPanel, hSound);
                    panel[103] = 1;
                    sound[118] = SigType != lastSigType ? 1 : 2;
                }
                
            }
            else//ハンドル位置条件外
            {
                CgSType = 0;
                panel[127] = 0;
                if (!handles.Equals(lastHandle2))
                {
                    lastHandle2 = handles;
                    MetroPlugin.SetBrake(handles.Brake);
                    MetroPlugin.SetPower(handles.Power);
                    MetroPlugin.SetReverser(handles.Reverser);
                }
                handles = MetroPlugin.Elapse(state, hPanel, hSound);
                
                panel[103] = 1;
            }

            //ホームドア表示灯
            if(DoorClosed == true)//閉扉時
            {
                panel[183] = panel[181] != 0 ? 0 : 1;
                panel[184] = panel[182] != 0 ? 0 : 1;
            }
            else//開扉時
            {
                panel[183] = panel[192] == 2 ? 0 : 1;
                panel[184] = panel[192] == 2 ? 0 : 1;
            }

            if (OtherpluginLoaded)
            {
                if (!handles.Equals(lastHandle3))
                {
                    lastHandle3 = handles;
                    Otherplugin.SetBrake(handles.Brake);
                    if(handles.Brake == 0)Otherplugin.SetPower(handles.Power);
                    else Otherplugin.SetPower(0);
                    Otherplugin.SetReverser(handles.Reverser);
                }
                handles = Otherplugin.Elapse(state, hPanel, hSound);
            }
            handles.ConstantSpeed = CSC50THandle.ConstantSpeed;

            return handles;
        }
    }
}
