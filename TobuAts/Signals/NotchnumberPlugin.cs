using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TobuAts
{
    class NotchnumberPlugin
    {
        private const string PIPath = "NotchNumber.dll";
        private const CallingConvention CalCnv = CallingConvention.StdCall;
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void Load();
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void Dispose();
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void SetVehicleSpec(TobuAts.AtsVehicleSpec s);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void Initialize(int s);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern unsafe TobuAts.AtsHandles Elapse(TobuAts.AtsVehicleState s, IntPtr Pa, IntPtr So);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void SetPower(int p);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void SetBrake(int b);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void SetReverser(int r);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void KeyDown(int k);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void KeyUp(int k);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void HornBlow(int k);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void DoorOpen();
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void DoorClose();
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void SetSignal(int s);
        [DllImport(PIPath, CallingConvention = CalCnv)]
        public static extern void SetBeaconData(TobuAts.AtsBeaconData b);
    }
}
