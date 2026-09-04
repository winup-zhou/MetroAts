using BveEx.Extensions.Native;
using BveTypes.ClassWrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdakyuSignal {
    internal partial class OM_ATS {
        private static TimeSpan InitStartTime = TimeSpan.Zero;
        private static bool SpeedCaution = false;
        private static double EBTriggerSpeed = Config.LessInf;
        private static double LimitSpeed = Config.LessInf;
        private static bool EBApplied = false;
        private static bool EBNeedConfirm = false;

        public static bool ATS_Power, ATS_Triggered, ATS_SpeedCaution, ATS_EmergencyOperation;
        public static int BrakeCommand = 0;
        public static bool ATSEnable = false;

        public static void Tick(VehicleState state) {
            if (!ATSEnable) return;

            ATS_Power = true;
            ATS_SpeedCaution = SpeedCaution;
            double sp = Math.Abs(state.Speed);

            // EB 动作中：保持非常制动直到列车停止，再由 B1 确认解除
            if (EBNeedConfirm) {
                BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches + 1;
                ATS_Triggered = true;
                ATS_EmergencyOperation = sp > 0.5; // 停车后非常运转显示熄灭（等待确认）
                return;
            }

            bool over = LimitSpeed < Config.LessInf && sp > LimitSpeed;
            if (over) {
                ATS_Triggered = true;
                if (EBTriggerSpeed < Config.LessInf) {
                    // EB 型限制（41-停止 / 42 系地上子）：超速→非常制动，须停车后 B1 确认
                    EBApplied = true;
                    EBNeedConfirm = true;
                    BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches + 1;
                    ATS_EmergencyOperation = true;
                } else {
                    // 常用最大制动（40 / 41-注意・减速）：降速到限制以下即缓解
                    BrakeCommand = OdakyuSignal.vehicleSpec.BrakeNotches;
                }
            } else {
                if (EBApplied && sp <= LimitSpeed) EBApplied = false;
                BrakeCommand = 0;
                ATS_Triggered = false;
                ATS_EmergencyOperation = false;
            }
        }
    }
}
