using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobuAts
{
    public class CSATC
    {
        public static int ATCmode = 0; //0:地下铁ATC 1:西武ATC 2:ATC-P 3:T-DATC 
        public static int nowsig = 0;
        private static int[] Limit = {0,0,0,0,0,0,0,0,0,0,0,10,10,15,20,25,30,35,40,45,50,55,60,65,70,75,80,85,90,95,100,105,110,120};
        public static bool noticeStatus,ORPstatus;
        public static SpeedLimit ORPpattern = new SpeedLimit(),staStoppattern = new SpeedLimit();
        public static void Refreshsig(int signal) {
            if (signal == 35) {
                switch (nowsig)
                {
                    case 15:
                        ORPpattern = new SpeedLimit(7, TobuAts.NowGamelocation + 48);
                        break;
                    case 17:
                        ORPpattern = new SpeedLimit(7, TobuAts.NowGamelocation + 79);
                        break;
                    case 22:
                        ORPpattern = new SpeedLimit(7, TobuAts.NowGamelocation + 220);
                        break;
                    default:
                        ORPpattern = new SpeedLimit(7,TobuAts.NowGamelocation + 79);
                        break;
                }
            }
            nowsig = signal;
        }
        public static void Refreshbeacon(TobuAts.AtsBeaconData data)
        {
            if (data.Type == 31)
            {
                noticeStatus = data.Signal < nowsig;
            }
            else if (data.Type == 7)
            {
                noticeStatus = data.Optional > 0;
            }
            else if (data.Type == 12)
            {
                ORPpattern = new SpeedLimit(7, TobuAts.NowGamelocation + data.Optional);
            }
            else if (data.Type == 32)
            {

            }
        }
        public static int BrakeControl()
        {

            return TobuAts.vehicleSpec.BrakeNotches;
        }
        public static void ORPspeed()
        {
            
        }
    }
}
