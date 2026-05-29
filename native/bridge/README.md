# Native control bridge

`LiveActivityBridge.swift` exposes an `@objc` class (`LiveActivityBridge`) that
the C# layer calls over `objc_msgSend` to start/update/end Live Activities. It
is **app-agnostic** — it only deals with the generic `[String: String]` state
defined in `../shared/GenericActivityAttributes.swift`, so a single compiled
framework serves every consuming app.

## Build

```bash
./build.sh
```

Produces `output/iOSActivityKitBridge.xcframework` (device `arm64` +
simulator `arm64`). `build.sh` compiles both `GenericActivityAttributes.swift`
and `LiveActivityBridge.swift` into the framework.

> The module is named `iOSActivityKitBridge` while the `@objc` class is
> `LiveActivityBridge`. They must differ — a Swift module whose name equals a
> public class name breaks the generated `.swiftinterface`. C# looks the class
> up by its Objective-C runtime name (`LiveActivityBridge`), independent of the
> module/framework name.

## Objective-C surface (selectors the C# layer calls)

| Swift | Selector | C# `objc_msgSend` shape |
|-------|----------|--------------------------|
| `shared` | `shared` | `IntPtr (recv, sel)` |
| `isEnabled()` | `isEnabled` | `bool (recv, sel)` |
| `start(name:jsonState:)` | `startWithName:jsonState:` | `IntPtr (recv, sel, IntPtr, IntPtr)` |
| `update(name:jsonState:)` | `updateWithName:jsonState:` | `void (recv, sel, IntPtr, IntPtr)` |
| `end(name:jsonState:dismissAfter:)` | `endWithName:jsonState:dismissAfter:` | `void (recv, sel, IntPtr, IntPtr, double)` |
| `endAll()` | `endAll` | `void (recv, sel)` |

`jsonState` is a JSON object of string→string pairs (the C# dictionary,
serialized). Min deployment target: **iOS 16.2**.
