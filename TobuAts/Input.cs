using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace TobuAts
{
    public static partial class TobuAts
    {
        public static bool DoorClosed;
        [DllExport(CallingConvention.StdCall)]
        public static void KeyDown(int keyIndex)
        {
            MetroPlugin.KeyDown(keyIndex);
            if (AutopilotLoaded) AutopilotPlugin.KeyDown(keyIndex);
            if (CSC50TLoaded) CSC50TPlugin.KeyDown(keyIndex);
            if (NotchnumberLoaded) NotchnumberPlugin.KeyDown(keyIndex);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.KeyDown(keyIndex);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void KeyUp(int keyIndex)
        {
            MetroPlugin.KeyUp(keyIndex);
            if (AutopilotLoaded) AutopilotPlugin.KeyUp(keyIndex);
            if (CSC50TLoaded) CSC50TPlugin.KeyUp(keyIndex);
            if (NotchnumberLoaded) NotchnumberPlugin.KeyUp(keyIndex);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.KeyUp(keyIndex);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetBeaconData(AtsBeaconData data)
        {
            TobuSig.ReadBeacon(data);
            MetroPlugin.SetBeaconData(data);
            if (AutopilotLoaded) AutopilotPlugin.SetBeaconData(data);
            if (CSC50TLoaded) CSC50TPlugin.SetBeaconData(data);
            if (NotchnumberLoaded) NotchnumberPlugin.SetBeaconData(data);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.SetBeaconData(data);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetSignal(int signalIndex)
        {
            TobuSig.RefreshSignal(signalIndex);
            MetroPlugin.SetSignal(signalIndex);
            if (AutopilotLoaded) AutopilotPlugin.SetSignal(signalIndex);
            if (CSC50TLoaded) CSC50TPlugin.SetSignal(signalIndex);
            if (NotchnumberLoaded) NotchnumberPlugin.SetSignal(signalIndex);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.SetSignal(signalIndex);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Initialize(int initialHandlePosition)
        {
            TobuSig.init();
            DoorClosed = false;
            MetroPlugin.Initialize(initialHandlePosition);
            if (AutopilotLoaded) AutopilotPlugin.Initialize(initialHandlePosition);
            if (CSC50TLoaded) CSC50TPlugin.Initialize(initialHandlePosition);
            if (NotchnumberLoaded) NotchnumberPlugin.Initialize(initialHandlePosition);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.Initialize(initialHandlePosition);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void DoorOpen()
        {
            DoorClosed = false;
            MetroPlugin.DoorOpen();
            TobuSig.NextStop = false;
            TobuSig.InvisiablePattern = new SpeedLimit();
            if (AutopilotLoaded) AutopilotPlugin.DoorOpen();
            if (CSC50TLoaded) CSC50TPlugin.DoorOpen();
            if (NotchnumberLoaded) NotchnumberPlugin.DoorOpen();
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.DoorOpen();
        }

        [DllExport(CallingConvention.StdCall)]
        public static void DoorClose()
        {
            DoorClosed = true;
            MetroPlugin.DoorClose();
            if (AutopilotLoaded) AutopilotPlugin.DoorClose();
            if (CSC50TLoaded) CSC50TPlugin.DoorClose();
            if (NotchnumberLoaded) NotchnumberPlugin.DoorClose();
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.DoorClose();
        }

        [DllExport(CallingConvention.StdCall)]
        public static void HornBlow(int type)
        {
            MetroPlugin.HornBlow(type);
            if (AutopilotLoaded) AutopilotPlugin.HornBlow(type);
            if (CSC50TLoaded) CSC50TPlugin.HornBlow(type);
            if (NotchnumberLoaded) NotchnumberPlugin.HornBlow(type);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.HornBlow(type);
        }
    }
}
