using System;
using System.Collections.Generic;
using System.Linq;

namespace MetroAts {
    public partial class MetroAts {
        /// <summary>
        /// 已注册的信号设备及其最近一次仲裁上下文。
        /// </summary>
        private sealed class DeviceEntry {
            public ISignalDevice Device { get; }
            public KeyPosList ActiveKey { get; set; } = KeyPosList.None;
            public SignalSWList ActiveSw { get; set; } = SignalSWList.Noset;

            public DeviceEntry(ISignalDevice device) {
                Device = device;
            }
        }

        private readonly List<DeviceEntry> signalDevices = new List<DeviceEntry>();

        /// <summary>
        /// 是否有任一已注册设备处于激活状态。
        /// 未来可取代 SubPluginEnabled（子插件不再自行置位，而是由核心经仲裁后据此接管）。
        /// </summary>
        public bool AnyDeviceActive {
            get { return signalDevices.Any(entry => entry.Device.IsActive); }
        }

        /// <summary>
        /// 当前 ATO 是否可用（供 MetroAtsBridge 等外部插件读取）。
        /// 判定：1) 当前钥匙位（线路）在 <see cref="Config.ATOKeyPosLists"/> 白名单内
        /// （缺省仅 Metro，可用 <c>[atotascsw]atokeys</c> 覆盖扩展，如 <c>Metro,Tokyu</c>）；
        /// 2) 存在处于激活状态且实现 <see cref="IATOStatusProvider"/> 的设备上报 ATO 可用。
        /// 由 ATC 设备（MetroSignal / TokyuSignal）在其内部按“ATC 有效且不在非设/构内位置”上报，
        /// 替代 bridge 此前读取 ATC 面板灯端子(panel 263/274 等)的耦合方式。
        /// </summary>
        public bool IsATOAvailable {
            get {
                if (!Config.ATOKeyPosLists.Contains(KeyPos)) return false;
                foreach (var entry in signalDevices) {
                    if (entry.Device.IsActive && entry.Device is IATOStatusProvider provider && provider.IsATOAvailable)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 注册一个信号设备。重复注册同一实例会被忽略。
        /// </summary>
        /// <param name="device">待注册的信号设备。</param>
        public void RegisterDevice(ISignalDevice device) {
            if (device is null) throw new ArgumentNullException(nameof(device));
            if (signalDevices.Any(entry => ReferenceEquals(entry.Device, device))) return;
            signalDevices.Add(new DeviceEntry(device));
        }

        /// <summary>
        /// 注销一个信号设备。
        /// </summary>
        /// <param name="device">待注销的信号设备。</param>
        public void UnregisterDevice(ISignalDevice device) {
            if (device is null) return;
            signalDevices.RemoveAll(entry => ReferenceEquals(entry.Device, device));
        }

        /// <summary>
        /// 设备级仲裁。每个信号设备应在其 Tick 首行调用本方法（传入自身），
        /// 由核心统一判定"当前钥匙/信号档位组合是否归本设备"。
        /// 仅当设备的激活状态需要翻转时才调用 Activate / Deactivate，
        /// 其余时刻为幂等空操作，因此与 BveEX 不保证插件 Tick 顺序的特性兼容。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 本方法只做"钥匙/档位 → 设备"的路由级许可，判定为：钥匙已插入(None 除外)
        /// 且 <see cref="ISignalDevice.Supports"/> 受理当前 (KeyPos, SignalSW) 组合。
        /// 与既有子插件激活逻辑保持一致，本方法 <b>不</b> 包含以下判定：
        /// </para>
        /// <list type="bullet">
        /// <item><description><b>不判定车门状态</b>：既有逻辑中开门不导致设备失活（门只影响核心对 Reverser 的强制）。</description></item>
        /// <item><description><b>不判定制动条件</b>：EB/brake 门控按设备而异（多数设备需 brake≠EB，JR 与 SeibuATS 档不要求），
        /// 必须由设备在 Activate/IsActive 内部自行处理。设备收到 Activate 时若自身运行条件未满足，应保持待命
        /// （IsActive 保持 false），Arbitrate 会在后续帧再次调用 Activate —— 因此 Activate 必须幂等。</description></item>
        /// <item><description><b>不做区段/信标类领域判断</b>：信号档位在同设备内变化时，设备可在其 Tick 内读取核心 SignalSWPos 自行切换内部模式。</description></item>
        /// </list>
        /// </remarks>
        /// <param name="device">调用方信号设备（自身）。</param>
        public void Arbitrate(ISignalDevice device) {
            if (device is null) return;

            DeviceEntry entry = signalDevices.FirstOrDefault(item => ReferenceEquals(item.Device, device));
            if (entry is null) {
                RegisterDevice(device);
                entry = signalDevices.First(item => ReferenceEquals(item.Device, device));
            }

            KeyPosList key = KeyPos;
            SignalSWList sw = SignalSWPos;
            bool wantOn = key != KeyPosList.None && device.Supports(key, sw);

            if (wantOn) {
                if (!device.IsActive) {
                    device.Activate(key, sw);
                } else if (entry.ActiveKey != key) {
                    // 设备保持激活期间钥匙位置发生变化（直通换线）
                    device.OnKeyPosChanged(key);
                }
                entry.ActiveKey = key;
                entry.ActiveSw = sw;
            } else if (device.IsActive) {
                device.Deactivate(key);
                entry.ActiveKey = KeyPosList.None;
                entry.ActiveSw = SignalSWList.Noset;
            }
        }
    }
}
