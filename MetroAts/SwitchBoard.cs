using System.Collections.Generic;

namespace MetroAts {
    // ============================================================
    // 钥匙位置 (KeyPosList) 与信号开关位置 (SignalSWList) 逻辑集中管理。
    // - 档位步进（I/J 钥匙、G/H 信号开关、Space+TASC）
    // - 安全位复位（场景开始 / 紧急制动）
    // - 显示文本与面板输出的映射（消除原分散在 Tick/Input 中的重复 switch）
    // ============================================================
    public partial class MetroAts {

        // ---------- 显示文本映射 ----------
        private static readonly IReadOnlyDictionary<KeyPosList, string> KeyDisplayTexts =
            new Dictionary<KeyPosList, string> {
                { KeyPosList.None,       "未挿入" },
                { KeyPosList.Tokyu,      "東　急" },
                { KeyPosList.Metro,      "メトロ" },
                { KeyPosList.Tobu,       "東　武" },
                { KeyPosList.Seibu,      "西　武" },
                { KeyPosList.ToyoKosoku, "東　葉" },
                { KeyPosList.JR,         "Ｊ　Ｒ" },
                { KeyPosList.Sotetsu,    "相　鉄" },
                { KeyPosList.Odakyu,     "小田急" },
            };

        private static readonly IReadOnlyDictionary<SignalSWList, string> SignalSWDisplayTexts =
            new Dictionary<SignalSWList, string> {
                { SignalSWList.Noset,     "非設" },
                { SignalSWList.InDepot,   "構内" },
                { SignalSWList.ATC,       "ATC" },
                { SignalSWList.Tobu,      "東武" },
                { SignalSWList.SeibuATS,  "西武" },
                { SignalSWList.Sotetsu,   "相鉄" },
                { SignalSWList.JR,        "ＪＲ" },
                { SignalSWList.TokyuATS,  "東急ATS" },
                { SignalSWList.WS_ATC,    "WS-ATC" },
                { SignalSWList.ATP,       "ATP" },
                { SignalSWList.Odakyu,    "小田急" },
            };

        // ---------- 钥匙位置 → 面板 keyoutput 数值 ----------
        private static readonly IReadOnlyDictionary<KeyPosList, int> KeyPanelOutputs =
            new Dictionary<KeyPosList, int> {
                { KeyPosList.None,       0 },
                { KeyPosList.Metro,      1 },
                { KeyPosList.Tobu,       2 },
                { KeyPosList.Tokyu,      3 },
                { KeyPosList.Seibu,      4 },
                { KeyPosList.Sotetsu,    5 },
                { KeyPosList.JR,         6 },
                { KeyPosList.Odakyu,     7 },
                { KeyPosList.ToyoKosoku, 8 },
            };

        private string KeyDisplayText() {
            string text;
            return KeyDisplayTexts.TryGetValue(KeyPos, out text) ? text : "無　効";
        }

        private string SignalSWDisplayText() {
            string text;
            return SignalSWDisplayTexts.TryGetValue(SignalSWPos, out text) ? text : "無効";
        }

        // ---------- 位置索引工具 ----------
        private int KeyIndex(KeyPosList key) {
            return Config.KeyPosLists.IndexOf(key);
        }

        /// <summary>实时求"拔出位(None)"在钥匙档位列表中的索引。</summary>
        private int NoneKeyIndex() {
            return Config.KeyPosLists.IndexOf(KeyPosList.None);
        }

        /// <summary>
        /// 场景启动 / 紧急制动时复位到安全位：钥匙归到拔出位(None)、
        /// 信号开关归到本车的单一安全档位（Config.SafeSignalSW，见 ResolveSafeSignalSW：
        /// 默认 Noset；对 JR 兼任非设的车自动/显式回退到 JR），并关闭 ATO/TASC。
        /// </summary>
        private void ResetPositionsToSafe() {
            isTASCenabled = false;
            int noneIndex = NoneKeyIndex();
            if (noneIndex >= 0) {
                NowKey = noneIndex;
            }
            int safeIndex = Config.SignalSWLists.IndexOf(Config.SafeSignalSW);
            if (safeIndex >= 0) {
                NowSignalSW = safeIndex;
            }
        }

        /// <summary>
        /// 步进钥匙档位。
        /// dir = -1 对应 I 侧（向拔出位左侧/逆序方向），+1 对应 J 侧（向拔出位右侧/顺序方向）。
        /// 语义（与 BVE 钥匙手感一致）：不在拔出位时只能退回到拔出位(None)，
        /// 在拔出位时才可向两侧插入；EnforceKeyPos 且已知 LineDef 时直接跳转到线路档。
        /// </summary>
        private void MoveKey(int dir) {
            int count = Config.KeyPosLists.Count;
            if (Config.KeyPosLists[NowKey] == KeyPosList.None) {
                // 当前位于拔出位：向目标方向插入
                if (dir < 0) {
                    if (NowKey <= 0) return; // 已在最左，无可降
                    if (LineDef != KeyPosList.None && Config.EnforceKeyPos) {
                        int target = KeyIndex(LineDef);
                        if (target >= 0 && NowKey > target) {
                            NowKey = target;
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    } else {
                        NowKey--;
                        Sound_Keyin = AtsSoundControlInstruction.Play;
                    }
                } else {
                    if (NowKey >= count - 1) return; // 已在最右，无可升
                    if (LineDef != KeyPosList.None && Config.EnforceKeyPos) {
                        int target = KeyIndex(LineDef);
                        if (target >= 0 && NowKey < target) {
                            NowKey = target;
                            Sound_Keyin = AtsSoundControlInstruction.Play;
                        }
                    } else {
                        NowKey++;
                        Sound_Keyin = AtsSoundControlInstruction.Play;
                    }
                }
                return;
            }

            // 当前已插入：只能退回到拔出位，或在拔出位同侧继续步进
            int noneIndex = NoneKeyIndex();
            if (dir < 0) {
                if (NowKey > noneIndex) {
                    NowKey = noneIndex; // 退回到拔出位
                    Sound_Keyout = AtsSoundControlInstruction.Play;
                } else if (NowKey > 0 && !Config.EnforceKeyPos) {
                    NowKey--;
                    Sound_Keyin = AtsSoundControlInstruction.Play;
                }
            } else {
                if (NowKey < noneIndex) {
                    NowKey = noneIndex; // 退回到拔出位
                    Sound_Keyout = AtsSoundControlInstruction.Play;
                } else if (NowKey < count - 1 && !Config.EnforceKeyPos) {
                    NowKey++;
                    Sound_Keyin = AtsSoundControlInstruction.Play;
                }
            }
        }

        /// <summary>
        /// 步进信号开关档位。
        /// dir = -1 对应 G 侧（逆序），+1 对应 H 侧（顺序）。
        /// loop 模式循环移动；非 loop 模式在边界处保持不动。
        /// </summary>
        private void MoveSignalSW(int dir) {
            int count = Config.SignalSWLists.Count;
            if (count <= 0) return;

            if (Config.SignalSW_loop) {
                NowSignalSW = (NowSignalSW + dir) % count;
                if (NowSignalSW < 0) NowSignalSW += count;
                Sound_SignalSW = AtsSoundControlInstruction.Play;
            } else {
                int target = NowSignalSW + dir;
                if (target >= 0 && target < count) {
                    NowSignalSW = target;
                    Sound_SignalSW = AtsSoundControlInstruction.Play;
                }
            }
        }

        /// <summary>切换 ATO/TASC 使能（仅在状态实际变化时播放档位切换音）。</summary>
        private void ToggleTASC(bool enable) {
            if (isTASCenabled == enable) return;
            isTASCenabled = enable;
            Sound_SignalSW = AtsSoundControlInstruction.Play;
        }

        // ---------- 面板输出（供 Initialize / Tick 共用，保证两处永远一致）----------
        private void WriteKeyPosToPanel(IList<int> panel) {
            int value;
            panel[Config.Panel_keyoutput] = KeyPanelOutputs.TryGetValue(KeyPos, out value) ? value : 0;
        }

        private void WriteSignalSWToPanel(IList<int> panel) {
            if (!Config.SignalSW_legacyoutput) {
                panel[Config.Panel_SignalSWoutput] = (int)Config.SignalSWLists[NowSignalSW];
            } else {
                panel[Config.Panel_SignalSWoutput] = SignalSWLegacyOutput(KeyPos, SignalSWPos);
            }
        }

        /// <summary>信号开关的旧式(兼容)面板数值。所有档位均为全函数映射。</summary>
        private static int SignalSWLegacyOutput(KeyPosList key, SignalSWList sw) {
            switch (sw) {
                case SignalSWList.WS_ATC:
                    return 5;
                case SignalSWList.Noset:
                    return key == KeyPosList.Tokyu ? 4 : 3;
                case SignalSWList.ATC:
                    return 1;
                case SignalSWList.InDepot:
                    return 2;
                default: // TokyuATS / Odakyu / Sotetsu / SeibuATS / Tobu / JR / ATP
                    return 0;
            }
        }
    }
}
