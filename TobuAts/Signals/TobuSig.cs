using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;

namespace TobuAts
{
    public class TobuSig
    {
        public static int[] ATCLimit = { -2, -2, -2, -2, -2, -2, -2, -2, -2, 0, 0, 10, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110, 120, -1, -2, -2, -1, 45, 40, 35, 30, 25, 20, 15, 10, 10, 0, -2 };
        public static SpeedLimit ATCPattern = new SpeedLimit(),InvisiablePattern = new SpeedLimit();
        public static int NowSig, LastSig ,TrackPos,MaxDis;
        public static double CurrentDis = 0;
        public static bool Ding = false,Plamp = false,NextStop = false;
        public static int[] SectionLimits = { -5, -5, -5, -5, -5, -5, -5, -5, -5 };
        public static double[] SectionDistance = { -5, -5, -5, -5, -5, -5, -5, -5, -5 };
        const double ORPdec = -2.25;//*10
        const double InvDec = -4.0;

        public static void init()
        {
            MaxDis = 0;
            CurrentDis = 0;
            for (int i = 0; i < 8; ++i)
            {
                SectionDistance[i] = SectionLimits[i] = -5;
            }
        }
        public static void RefreshSignal(int sig)
        {
            LastSig = NowSig;
            NowSig = sig;
        }

        public static void ReadBeacon(TobuAts.AtsBeaconData data)
        {
            switch (data.Type) {
                case 31:
                    if (data.Optional <= 7)
                    {
                        SectionDistance[data.Optional+1] = data.Distance;
                        SectionLimits[data.Optional+1] = ATCLimit[data.Signal]<0?0: ATCLimit[data.Signal];
                    }
                    break;
                case 42:
                    TrackPos = data.Optional;
                    break;
                case 43:
                    NextStop = true;
                    InvisiablePattern = new SpeedLimit { Limit = 0, Location = TobuAts.NowGamelocation + 510 };
                    break;
                case 44:
                    MaxDis = data.Optional;
                    break;
            }
        }

        public static double MonitorSpeed(double Location,double Speed)
        {
            var LastPattern = ATCPattern;
            if (SectionLimits[2] == -5)
            {
                ATCPattern = new SpeedLimit { Limit = ATCLimit[NowSig] < 0 ? 0 : ATCLimit[NowSig], Location = Location };
            }
            else
            {
                if (ATCPattern.Limit > SectionLimits[2] && SectionLimits[2] != -5) {
                    Plamp = true;
                    if (SectionLimits[2] == 0) { ATCPattern = new SpeedLimit { Limit = SectionLimits[2], Location = SectionDistance[2] + Location - 15 }; Ding = true; }
                    else ATCPattern = new SpeedLimit { Limit = SectionLimits[2], Location = SectionDistance[2] + Location };
                }
                if (SectionLimits[1] < ATCLimit[NowSig])
                {
                    ATCPattern = new SpeedLimit { Limit = ATCLimit[NowSig] < 0 ? 0 : ATCLimit[NowSig], Location = Location };
                    Plamp = false;
                }
                for (int i = 1; i < SectionLimits.Length; ++i)
                {
                    if(SectionLimits[i] == 0)
                    {
                        CurrentDis = SectionDistance[i] - 15;
                        break;
                    }
                    if (i == SectionLimits.Length - 1) CurrentDis = 0;
                }
            }
            if (CurrentDis == 0&&(Speed > Math.Min(Math.Min(ATCPattern.AtLocation(Location, ORPdec), ATCLimit[NowSig] < 0 ? 0 : ATCLimit[NowSig]), 100) || Speed <= ATCPattern.Limit))
            {
                if(Plamp = true)Ding = true; Plamp = false;
            }
            return Math.Min(Math.Min(ATCPattern.AtLocation(Location, ORPdec),ATCLimit[NowSig] < 0 ? 0 : ATCLimit[NowSig]), 100);
        }

    }
}
