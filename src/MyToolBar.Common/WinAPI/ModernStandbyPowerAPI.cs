using System.Runtime.InteropServices;

namespace MyToolBar.Common.WinAPI;

/// <summary>
/// 注册Modern Standby电源通知
/// </summary>
public class ModernStandbyPowerAPI
{
    /// <summary>
    /// OS callback delegate definition
    /// </summary>
    /// <param name="context">The context for the callback</param>
    /// <param name="type">The type of the callback...for power notifcation it's a PBT_ message</param>
    /// <param name="setting">A structure related to the notification, depends on type parameter</param>
    /// <returns></returns>
    public delegate int DeviceNotifyCallbackRoutine(IntPtr context, int type, IntPtr setting);
    
    public const int WM_POWERBROADCAST = 536; // (0x218)
    public const int PBT_APMPOWERSTATUSCHANGE = 10; // (0xA) - Power status has changed.
    public const int PBT_APMRESUMEAUTOMATIC = 18; // (0x12) - Operation is resuming automatically from a low-power state.This message is sent every time the system resumes.
    public const int PBT_APMRESUMESUSPEND = 7; // (0x7) - Operation is resuming from a low-power state.This message is sent after PBT_APMRESUMEAUTOMATIC if the resume is triggered by user input, such as pressing a key.
    public const int PBT_APMSUSPEND = 4; // (0x4) - System is suspending operation.
    public const int PBT_POWERSETTINGCHANGE = 32787; // (0x8013) - A power setting change event has been received.
    private const int DEVICE_NOTIFY_CALLBACK = 2;
    
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS
    {
        public DeviceNotifyCallbackRoutine Callback;
        public IntPtr Context;
    }
    static Dictionary<IntPtr, GCHandle> _registeredHandle = [];
 
    [DllImport("Powrprof.dll", SetLastError = true)]
    static extern uint PowerRegisterSuspendResumeNotification(uint flags, ref DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS receipient, ref IntPtr registrationHandle);

    [DllImport("Powrprof.dll", SetLastError = true)]
    static extern uint PowerUnregisterSuspendResumeNotification(IntPtr registrationHandle);
    
    public static bool RegisterNotification(DeviceNotifyCallbackRoutine callback, ref IntPtr handle)
    {
        DEVICE_NOTIFY_SUBSCRIBE_PARAMETERS parameters = new()
        {
            Callback = callback,
            Context = IntPtr.Zero
        };
        var gcHandle = GCHandle.Alloc(parameters);
        uint result = PowerRegisterSuspendResumeNotification(DEVICE_NOTIFY_CALLBACK, ref parameters, ref handle);
        _registeredHandle[handle] = gcHandle;
        return result == 0;
    }
    public static bool UnregisterNotification(IntPtr handle)
    {
        uint result = PowerUnregisterSuspendResumeNotification(handle);
        bool free = false;
        if(_registeredHandle.TryGetValue(handle, out var gcHandle))
        {
            gcHandle.Free();
            _registeredHandle.Remove(handle);
            free = true;
        }
        return result == 0 && free;
    }
}