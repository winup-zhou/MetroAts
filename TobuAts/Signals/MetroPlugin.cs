using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TobuAts
{
    class MetroPlugin
    {
        private const CallingConvention CalCnv = CallingConvention.StdCall;
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void Load();
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void Dispose();
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void SetVehicleSpec(TobuAts.AtsVehicleSpec s);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void Initialize(int s);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern unsafe TobuAts.AtsHandles Elapse(TobuAts.AtsVehicleState s, IntPtr Pa, IntPtr So);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void SetPower(int p);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void SetBrake(int b);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void SetReverser(int r);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void KeyDown(int k);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void KeyUp(int k);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void HornBlow(int k);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void DoorOpen();
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void DoorClose();
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void SetSignal(int s);
        [DllImport("Ats.dll", CallingConvention = CalCnv)]
        public static extern void SetBeaconData(TobuAts.AtsBeaconData b);
    }
}
