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

            return handles;
        }
    }
}
