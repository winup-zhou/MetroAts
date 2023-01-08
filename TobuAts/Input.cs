using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TobuAts
{
    public static partial class TobuAts
    {
        [DllExport(CallingConvention.StdCall)]
        public static void KeyDown(AtsKey key)
        {

        }

        [DllExport(CallingConvention.StdCall)]
        public static void KeyUp(AtsKey key)
        {

        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetBeaconData(AtsBeaconData data)
        {
            switch (data.Type)
            {

            }
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Initialize(int initialHandlePosition)
        {

        }

        [DllExport(CallingConvention.StdCall)]
        public static void DoorOpen()
        {

        }

        [DllExport(CallingConvention.StdCall)]
        public static void DoorClose()
        {

        }

        [DllExport(CallingConvention.StdCall)]
        public static void HornBlow(int type)
        {

        }

    }
}
