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
        public static AtsVehicleSpec vehicleSpec;
        public static int SignalType = 0;
        public static int CompanyType = 0;
        [DllExport(CallingConvention.StdCall)]
        public static void Load()
        {

        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetVehicleSpec(AtsVehicleSpec spec)
        {
            vehicleSpec = spec;
        }
    }
}
