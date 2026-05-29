using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace iOSActivityKit.Platforms.iOS;

/// <summary>
/// iOS implementation of <see cref="ILiveActivityService"/>. Calls into the
/// native <c>LiveActivityBridge</c> (shipped as the iOSActivityKitBridge
/// .xcframework) over <c>objc_msgSend</c>, so no Objective-C binding project
/// is required.
/// </summary>
public sealed class LiveActivityServiceiOS : ILiveActivityService
{
    private const string LibObjC = "/usr/lib/libobjc.dylib";

    private NSObject? bridge;

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtrMsgSend(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtrMsgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool BoolMsgSend(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void VoidMsgSend(IntPtr receiver, IntPtr selector);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void VoidMsgSendPtrPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void VoidMsgSendPtrPtrDouble(
        IntPtr receiver,
        IntPtr selector,
        IntPtr arg1,
        IntPtr arg2,
        double arg3);

    /// <inheritdoc/>
    public bool IsSupported
    {
        get
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(16, 2))
            {
                return false;
            }

            var b = GetBridge();
            return b is not null && BoolMsgSend(b.Handle, Selector.GetHandle("isEnabled"));
        }
    }

    /// <inheritdoc/>
    public string? Start(string name, IReadOnlyDictionary<string, string> state)
    {
        if (!UIDevice.CurrentDevice.CheckSystemVersion(16, 2))
        {
            return null;
        }

        try
        {
            var b = GetBridge();
            if (b is null)
            {
                return null;
            }

            using var nameStr = new NSString(name ?? string.Empty);
            using var jsonStr = new NSString(StateSerializer.ToJson(state));

            var resultHandle = IntPtrMsgSend(
                b.Handle,
                Selector.GetHandle("startWithName:jsonState:"),
                nameStr.Handle,
                jsonStr.Handle);

            if (resultHandle == IntPtr.Zero)
            {
                return null;
            }

            var id = Runtime.GetNSObject<NSString>(resultHandle)?.ToString();
            return string.IsNullOrEmpty(id) ? null : id;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[iOSActivityKit] Start failed: {ex.Message}");
            return null;
        }
    }

    /// <inheritdoc/>
    public void Update(string name, IReadOnlyDictionary<string, string> state)
    {
        if (!UIDevice.CurrentDevice.CheckSystemVersion(16, 2))
        {
            return;
        }

        try
        {
            var b = GetBridge();
            if (b is null)
            {
                return;
            }

            using var nameStr = new NSString(name ?? string.Empty);
            using var jsonStr = new NSString(StateSerializer.ToJson(state));

            VoidMsgSendPtrPtr(
                b.Handle,
                Selector.GetHandle("updateWithName:jsonState:"),
                nameStr.Handle,
                jsonStr.Handle);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[iOSActivityKit] Update failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public void End(
        string name,
        IReadOnlyDictionary<string, string>? finalState = null,
        TimeSpan dismissAfter = default)
    {
        if (!UIDevice.CurrentDevice.CheckSystemVersion(16, 2))
        {
            return;
        }

        try
        {
            var b = GetBridge();
            if (b is null)
            {
                return;
            }

            var json = finalState is null ? string.Empty : StateSerializer.ToJson(finalState);
            using var nameStr = new NSString(name ?? string.Empty);
            using var jsonStr = new NSString(json);

            VoidMsgSendPtrPtrDouble(
                b.Handle,
                Selector.GetHandle("endWithName:jsonState:dismissAfter:"),
                nameStr.Handle,
                jsonStr.Handle,
                dismissAfter.TotalSeconds);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[iOSActivityKit] End failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public void EndAll()
    {
        if (!UIDevice.CurrentDevice.CheckSystemVersion(16, 2))
        {
            return;
        }

        try
        {
            var b = GetBridge();
            if (b is null)
            {
                return;
            }

            VoidMsgSend(b.Handle, Selector.GetHandle("endAll"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[iOSActivityKit] EndAll failed: {ex.Message}");
        }
    }

    private NSObject? GetBridge()
    {
        if (bridge is not null)
        {
            return bridge;
        }

        var cls = Class.GetHandle("LiveActivityBridge");
        if (cls == IntPtr.Zero)
        {
            System.Diagnostics.Debug.WriteLine(
                "[iOSActivityKit] LiveActivityBridge class not found. " +
                "Is the iOSActivityKitBridge.xcframework referenced as a <NativeReference>?");
            return null;
        }

        var sharedHandle = IntPtrMsgSend(cls, Selector.GetHandle("shared"));
        if (sharedHandle == IntPtr.Zero)
        {
            return null;
        }

        bridge = Runtime.GetNSObject(sharedHandle);
        return bridge;
    }
}
