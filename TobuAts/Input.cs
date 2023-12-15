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
            if (OtherpluginLoaded) Otherplugin.KeyDown(keyIndex);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void KeyUp(int keyIndex)
        {
            MetroPlugin.KeyUp(keyIndex);
            if (AutopilotLoaded) AutopilotPlugin.KeyUp(keyIndex);
            if (CSC50TLoaded) CSC50TPlugin.KeyUp(keyIndex);
            if (OtherpluginLoaded) Otherplugin.KeyUp(keyIndex);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetBeaconData(AtsBeaconData data)
        {
            TobuSig.ReadBeacon(data);
            MetroPlugin.SetBeaconData(data);
            if (AutopilotLoaded) AutopilotPlugin.SetBeaconData(data);
            if (CSC50TLoaded) CSC50TPlugin.SetBeaconData(data);
            if (OtherpluginLoaded) Otherplugin.SetBeaconData(data);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetSignal(int signalIndex)
        {
            TobuSig.RefreshSignal(signalIndex);
            MetroPlugin.SetSignal(signalIndex);
            if (AutopilotLoaded) AutopilotPlugin.SetSignal(signalIndex);
            if (CSC50TLoaded) CSC50TPlugin.SetSignal(signalIndex);
            if (OtherpluginLoaded) Otherplugin.SetSignal(signalIndex);
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Initialize(int initialHandlePosition)
        {
            TobuSig.Init();
            DoorClosed = false;
            ResetStop = true;
            MetroPlugin.Initialize(initialHandlePosition);
            if (AutopilotLoaded) AutopilotPlugin.Initialize(initialHandlePosition);
            if (CSC50TLoaded) CSC50TPlugin.Initialize(initialHandlePosition);
            if (OtherpluginLoaded) Otherplugin.Initialize(initialHandlePosition);
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
            if (OtherpluginLoaded) Otherplugin.DoorOpen();
        }

        [DllExport(CallingConvention.StdCall)]
        public static void DoorClose()
        {
            DoorClosed = true;
            MetroPlugin.DoorClose();
            if (AutopilotLoaded) AutopilotPlugin.DoorClose();
            if (CSC50TLoaded) CSC50TPlugin.DoorClose();
            if (OtherpluginLoaded) Otherplugin.DoorClose();
        }

        [DllExport(CallingConvention.StdCall)]
        public static void HornBlow(int type)
        {
            MetroPlugin.HornBlow(type);
            if (AutopilotLoaded) AutopilotPlugin.HornBlow(type);
            if (CSC50TLoaded) CSC50TPlugin.HornBlow(type);
            if (OtherpluginLoaded) Otherplugin.HornBlow(type);
        }
    }
}
