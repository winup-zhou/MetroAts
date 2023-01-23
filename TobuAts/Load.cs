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
        public static double NowGamelocation = 0;
        public static int NowGameTime = 0;
        public static double NowVehicleSpeed = 0;
        public static bool MetropluginLoaded = false, AutopilotLoaded = false, CSC50TLoaded = false, NotchnumberLoaded = false, RealAnalogGaugeLoaded = false;
        public static bool ModeAvailable;

        static TobuAts()
        {
            Config.Load(Path.Combine(Config.PluginDir, "TobuAtsConfig.txt"));
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Load()
        {
            while (true)
            {
                DialogResult dr;
                try
                {
                    MetroPlugin.Load();
                    MetropluginLoaded = true;
                    break;
                }
                catch (DllNotFoundException e)
                {
                    dr = MessageBox.Show("メトロ統合プラグインが見つかりません。\n" + e.Message, "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                }
                catch (Exception e)
                {
                    dr = MessageBox.Show("エラーが発生しました。\n" + e.ToString(), "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                }
                if (dr != DialogResult.Retry) break;
            }
            if (Config.Load_bve_autopilot)
            {
                while (true)
                {
                    DialogResult dr;
                    try
                    {
                        AutopilotPlugin.Load();
                        AutopilotLoaded = true;
                        break;
                    }
                    catch (DllNotFoundException e)
                    {
                        dr = MessageBox.Show("bve-autopilotプラグインが見つかりません。\n" + e.Message, "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    catch (Exception e)
                    {
                        dr = MessageBox.Show("エラーが発生しました。\n" + e.ToString(), "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    if (dr != DialogResult.Retry) break;
                }
            }
            if (Config.Load_csc_plugin)
            {
                while (true)
                {
                    DialogResult dr;
                    try
                    {
                        CSC50TPlugin.Load();
                        CSC50TLoaded = true;
                        break;
                    }
                    catch (DllNotFoundException e)
                    {
                        dr = MessageBox.Show("CSC50Tプラグインが見つかりません。\n" + e.Message, "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    catch (Exception e)
                    {
                        dr = MessageBox.Show("エラーが発生しました。\n" + e.ToString(), "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    if (dr != DialogResult.Retry) break;
                }
            }
            if (Config.Load_Notchnumber_plugin)
            {
                while (true)
                {
                    DialogResult dr;
                    try
                    {
                        NotchnumberPlugin.Load();
                        NotchnumberLoaded = true;
                        break;
                    }
                    catch (DllNotFoundException e)
                    {
                        dr = MessageBox.Show("Notchnumberプラグインが見つかりません。\n" + e.Message, "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    catch (Exception e)
                    {
                        dr = MessageBox.Show("エラーが発生しました。\n" + e.ToString(), "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    if (dr != DialogResult.Retry) break;
                }
            }
            if (Config.Load_RealAnalogGauge_plugin)
            {
                while (true)
                {
                    DialogResult dr;
                    try
                    {
                        RealAnalogGaugePlugin.Load();
                        RealAnalogGaugeLoaded = true;
                        break;
                    }
                    catch (DllNotFoundException e)
                    {
                        dr = MessageBox.Show("RealAnalogGaugeプラグインが見つかりません。\n" + e.Message, "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    catch (Exception e)
                    {
                        dr = MessageBox.Show("エラーが発生しました。\n" + e.ToString(), "TobuAts", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                    if (dr != DialogResult.Retry) break;
                }
            }
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetVehicleSpec(AtsVehicleSpec spec)
        {
            MetroPlugin.SetVehicleSpec(spec);
            if (AutopilotLoaded) AutopilotPlugin.SetVehicleSpec(spec);
            if (CSC50TLoaded) CSC50TPlugin.SetVehicleSpec(spec);
            if (NotchnumberLoaded) NotchnumberPlugin.SetVehicleSpec(spec);
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.SetVehicleSpec(spec);
            vehicleSpec = spec;
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Dispose()
        {
            MetroPlugin.Dispose();
            if (AutopilotLoaded) AutopilotPlugin.Dispose();
            if (CSC50TLoaded) CSC50TPlugin.Dispose();
            if (NotchnumberLoaded) NotchnumberPlugin.Dispose();
            if (RealAnalogGaugeLoaded) RealAnalogGaugePlugin.Dispose();
        }
    }
}
