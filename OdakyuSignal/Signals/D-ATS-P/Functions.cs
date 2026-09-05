using BveEx.Extensions.Native.Input;
using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    /// <summary>
    /// D-ATS-P（小田急・连续照查式 ATS-P 形）。
    /// 限制由地上子 4(制限パターン発生) / 5(最高速度) / 22(信号机灯数) 与前方信号現示共同构成；
    /// Tick 每帧对前方闭塞现示计算速度照查模式并施加制动。
    /// 参考：https://souko.weebly.com/d-ats-p.html
    /// </summary>
    internal partial class D_ATS_P {
        public static void Init(TimeSpan time) {
            InitStartTime = time;
            ATSEnable = true;
        }

        public static void SwitchOver() {
            ATSEnable = true;
        }

        public static void ResetAll() {
            SignalPattern = SpeedPattern.inf;
            LimitPattern = SpeedPattern.inf;
            InitStartTime = TimeSpan.Zero;
            LastEBResetTime = TimeSpan.Zero;
            NeedConfirm = false;
            Confirmed = false;
            Noset = false;
            lastPatternApproach = false;
            MaxSpeed = 100;
            ValidDataFromBeacon = 2;
            lastcurrentSectionLocation = 0;
            lastBrakeNotch = lastPowerNotch = 0;
            BrakeCommand = 0;
            ATSEnable = false;

            ATS_Power = false;
            ATS_PatternApproach = false;
            ATS_SpeedCaution = false;
            ATS_Triggered = false;
            ATS_Noset = false;
            ATS_NoSignal = false;
            ATS_EmergencyOperation = false;
            EB_NeedConfirm = false;
            ATS_Pbeacon = false;

            WarnBell = SoundPlayMode.Stop;
            PatternApproach = SoundPlayMode.Stop;
            EB_buzzer = SoundPlayMode.Stop;
            SpeedCaution_buzzer = SoundPlayMode.Stop;
        }

        public static void BeaconPassed(VehicleState state, BeaconPassedEventArgs e) {
            switch (e.Type) {
                case 100: // 保安装置启动用地上子（通用）
                    if (!ATSEnable) ATSEnable = true;
                    break;
                case 22: // 信号机灯数取得地上子：设置在闭塞(信号机)开始 20m 手前，用于读取到下一信号机的距离
                    // 现实模型：距离由地上子提供（此处将照查模式终点置于信号机位置），信号現示则由轨道电路连续传输
                    ValidDataFromBeacon = 2;
                    if (state != null)
                        SignalPattern.Location = state.Location + e.Distance + 20; // 信标位置 + 到信号机的 20m
                    break;
                case 5: // 最高速度设定（无此地上子时 100km/h）
                    MaxSpeed = e.Optional;
                    break;
                case 4: // 制限速度パターン発生
                    // 网页编码：sendData = 位置[m]*100 + 制限速度/5（速度段固定2位，00=制限解除）
                    // 例：sendData=1272516 → 位置12725m / 速度16×5=80km/h；sendData=852500 → 8525m 处解除
                    if (ATSEnable) {
                        if (e.Optional == -1 || e.Optional % 100 == 0) {
                            LimitPattern = SpeedPattern.inf; // 制限解除
                        } else {
                            double limitSpeed = (e.Optional % 100) * 5; // 制限速度 [km/h]
                            double limitLocation = e.Optional / 100;    // 制限(模式)起点位置 [m]
                            LimitPattern = new SpeedPattern(limitSpeed, limitLocation, MaxSpeed);
                        }
                    }
                    break;
                case 60: // 終端照査：通过时超速→速度注意灯＋EB 需确认
                    if (ATSEnable) {
                        if (state is null) state = new VehicleState(0, 0, TimeSpan.Zero, 0, 0, 0, 0, 0, 0);
                        var monitorSpeed = Math.Min(SignalPattern.AtLocation(state.Location, -3.3), LimitPattern.AtLocation(state.Location, -3.3));
                        if (Math.Abs(state.Speed) > monitorSpeed) {
                            ATS_SpeedCaution = true;
                            SpeedCaution_buzzer = SoundPlayMode.PlayLooping;
                            NeedConfirm = true; // 进入 EB 需确认流程
                            LastEBResetTime = state.Time;
                        }
                    }
                    break;
            }
        }

        public static void ConfirmEB(VehicleState state, HandleSet handles) {
            // EB 装置/EB 需确认的复位：重置计时并解除 EB 照查
            LastEBResetTime = state?.Time ?? TimeSpan.Zero;
            if (NeedConfirm) {
                NeedConfirm = false;
                WarnBell = SoundPlayMode.Stop;
            }
            if (EB_NeedConfirm) {
                EB_NeedConfirm = false;
                EB_buzzer = SoundPlayMode.Stop;
            }
            BrakeCommand = 0;
            ATS_Triggered = false;
        }

        public static void Disable() {
            ResetAll();
        }
    }
}
