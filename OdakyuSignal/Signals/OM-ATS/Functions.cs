using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    /// <summary>
    /// OM-ATS（小田急旧式・点式 ATS）。
    /// 地上子（40 最外方 / 41 中間 / 42 直下）通过对信号机現示逐次设定限制速度，
    /// 该限制保持到通过下一个地上子为止（現示上升不会立即解除）。
    /// 现示编码（与小田急 D-ATS-P 场景一致）：0=停止 1=警戒(25) 2=注意(45) 3=减速(75) 4=进行(全开)
    /// 参考：https://souko.weebly.com/om-ats.html
    /// </summary>
    internal partial class OM_ATS {
        public static void Init(TimeSpan time) {
            InitStartTime = time;
            ATSEnable = true;
        }

        public static void SwitchOver() {
            ATSEnable = true;
        }

        public static void ResetAll() {
            InitStartTime = TimeSpan.Zero;
            LimitSpeed = Config.LessInf;
            SpeedCaution = false;
            EBTriggerSpeed = Config.LessInf;
            EBApplied = false;
            EBNeedConfirm = false;
            BrakeCommand = 0;
            ATSEnable = false;
            ATS_Power = false;
            ATS_Triggered = false;
            ATS_SpeedCaution = false;
            ATS_EmergencyOperation = false;
        }

        public static void DoorOpened() {
            // 开门时解除停车后的 EB 确认（停稳 + 门开视为已处置）
            if (EBNeedConfirm) {
                EBNeedConfirm = false;
                EBApplied = false;
                BrakeCommand = 0;
            }
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            if (!ATSEnable) return;

            switch (e.Type) {
                case 100: // 保安装置启动用地上子（通用）
                    if (!ATSEnable) ATSEnable = true;
                    break;

                case 40: // 最外方：对信号机为注意(現示2)时限制 75km/h，超速→常用最大制动
                    if (e.SignalIndex == 2) {
                        SetLimit(75, false);
                    }
                    break;

                case 41: // 中间：停止(0)→18(EB) / 注意(2)→45 / 减速(3)→75，超速→常用最大制动
                    if (e.SignalIndex == 0) {
                        SetLimit(18, true);
                    } else if (e.SignalIndex == 2) {
                        SetLimit(45, false);
                    } else if (e.SignalIndex == 3) {
                        SetLimit(75, false);
                    }
                    break;

                case 42: // 直下：停止(0)→0(EB停车) / 警戒(1)→25 / 注意(2)→45，超速→EB
                    if (e.SignalIndex == 0) {
                        SetLimit(0, true);
                    } else if (e.SignalIndex == 1) {
                        SetLimit(25, true);
                    } else if (e.SignalIndex == 2) {
                        SetLimit(45, true);
                    }
                    break;

                case 43: // 速度注意点灯：对信号机为停止现示时点亮
                    SpeedCaution = e.SignalIndex == 0;
                    break;

                case 60: // 终端照查：超速时 EB（速度注意点亮中）否则常用最大制动
                    if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
                    if (Math.Abs(state.Speed) > LimitSpeed) {
                        if (SpeedCaution) {
                            EBApplied = true;
                            EBNeedConfirm = true;
                            EBTriggerSpeed = LimitSpeed;
                            BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches + 1;
                            ATS_Triggered = true;
                        } else {
                            BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches;
                            ATS_Triggered = true;
                        }
                    }
                    break;
            }
        }

        /// <summary>设定限制速度。EB 型超速需要停车+确认(B1)后解除，非 EB 型降速到限制以下即解除。</summary>
        private static void SetLimit(double speed, bool useEB) {
            LimitSpeed = speed;
            EBTriggerSpeed = useEB ? speed : Config.LessInf;
            // 新的限制通常伴随速度注意状态刷新（现示非停止时熄灭提示）
            SpeedCaution = false;
            // 重新设限后若上一 EB 尚未确认，先复位以按新限制照查
            EBApplied = false;
            EBNeedConfirm = false;
        }

        public static void ConfirmEB(VehicleState state, HandleSet handles) {
            if (EBNeedConfirm) {
                EBNeedConfirm = false;
                EBApplied = false;
                BrakeCommand = 0;
                ATS_Triggered = false;
            }
        }

        public static void Disable() {
            ResetAll();
        }
    }
}
