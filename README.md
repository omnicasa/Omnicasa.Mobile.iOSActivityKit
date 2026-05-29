# iOSActivityKit

[![CI](https://github.com/omnicasa/Omnicasa.Mobile.iOSActivityKit/actions/workflows/ci.yml/badge.svg)](https://github.com/omnicasa/Omnicasa.Mobile.iOSActivityKit/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Omnicasa.Mobile.iOSActivityKit.svg?logo=nuget)](https://www.nuget.org/packages/Omnicasa.Mobile.iOSActivityKit)
[![NuGet downloads](https://img.shields.io/nuget/dt/Omnicasa.Mobile.iOSActivityKit.svg)](https://www.nuget.org/packages/Omnicasa.Mobile.iOSActivityKit)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Use iOS **ActivityKit Live Activities** and the **Dynamic Island** from a
**.NET MAUI / .NET for iOS** app — something the MAUI/.NET-iOS SDK doesn't
support out of the box.

It works by pairing a tiny `@objc` Swift control bridge (shipped prebuilt as an
`.xcframework`) with a thin C# layer that calls it over `objc_msgSend`. State is
a generic `string`→`string` dictionary, so **one prebuilt bridge works for any
app** — you only write your activity's SwiftUI.

```csharp
var id = liveActivity.Start("sync", new Dictionary<string, string>
{
    ["progress"] = "0.0",
    ["phase"]    = "Preparing",
});

liveActivity.Update("sync", new Dictionary<string, string>
{
    ["progress"] = "0.42",
    ["phase"]    = "Downloading",
    ["message"]  = "12 MB of 30 MB",
});

liveActivity.End("sync",
    finalState: new Dictionary<string, string> { ["progress"] = "1.0", ["phase"] = "Done" },
    dismissAfter: TimeSpan.FromSeconds(5));
```

On Android / Windows / Mac Catalyst every call is a safe no-op, so you never
need to wrap calls in platform checks.

---

## How it fits together

```
  ┌──────────────── your MAUI app (C#) ─────────────────┐
  │  ILiveActivityService  ──►  LiveActivityServiceiOS   │
  │                              (objc_msgSend)          │
  └───────────────────────────────┬─────────────────────┘
                                   │  calls @objc LiveActivityBridge.shared
            ┌──────────────────────▼───────────────────────┐
            │  iOSActivityKitBridge.xcframework (prebuilt)  │  ← <NativeReference>
            │  start / update / end  on  ActivityKit        │
            └──────────────────────┬───────────────────────┘
                                   │  ActivityKit matches by type name
            ┌──────────────────────▼───────────────────────┐
            │  YOUR widget extension (.appex, SwiftUI)      │  ← <AdditionalAppExtensions>
            │  renders Lock Screen + Dynamic Island         │
            └───────────────────────────────────────────────┘
```

Both the bridge and your widget compile the **same**
`GenericActivityAttributes` type (name `"GenericActivityAttributes"`, a
`ContentState` holding `data: [String: String]`). ActivityKit pairs a running
activity with its UI by that type, which is why the prebuilt bridge and your
hand-written widget find each other without you editing any Swift in the bridge.

## Repository layout

| Path | Contents |
|------|----------|
| `src/iOSActivityKit/` | The C# library → NuGet package `Omnicasa.Mobile.iOSActivityKit`. |
| `native/shared/GenericActivityAttributes.swift` | The shared state contract (compiled into both the bridge and your widget). |
| `native/bridge/` | The `@objc` control bridge + `build.sh` that produces `iOSActivityKitBridge.xcframework`. A prebuilt copy lives in `native/bridge/output/`. |
| `native/widget-template/` | A copy-in widget extension you customize (your SwiftUI lives here). |

## Setup

### 1. Add the NuGet C# layer

```xml
<PackageReference Include="Omnicasa.Mobile.iOSActivityKit" Version="1.0.0" />
```

Register it (MAUI DI):

```csharp
using iOSActivityKit;

builder.Services.AddLiveActivities();   // singleton ILiveActivityService
```

…then inject `ILiveActivityService`. Without DI, use `LiveActivity.Current`.

### 2. Reference the native bridge

Build it once (or use the prebuilt `native/bridge/output/iOSActivityKitBridge.xcframework`):

```bash
./native/bridge/build.sh
```

Then in your **iOS** `.csproj`:

```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
  <NativeReference Include="path/to/iOSActivityKitBridge.xcframework">
    <Kind>Framework</Kind>
    <SmartLink>true</SmartLink>
  </NativeReference>
</ItemGroup>
```

### 3. Add your widget extension

The Live Activity UI must live in a widget extension. Copy
`native/widget-template/`, customize `SampleLiveActivity.swift`, and embed it —
full step-by-step in [`native/widget-template/README.md`](native/widget-template/README.md).
Don't forget `NSSupportsLiveActivities` = `true` in your app's iOS `Info.plist`.

## API

```csharp
public interface ILiveActivityService
{
    bool    IsSupported { get; }
    string? Start (string name, IReadOnlyDictionary<string,string> state);
    void    Update(string name, IReadOnlyDictionary<string,string> state);
    void    End   (string name, IReadOnlyDictionary<string,string>? finalState = null,
                   TimeSpan dismissAfter = default);
    void    EndAll();
}
```

- **`name`** groups updates/ends and lets your SwiftUI switch layouts via
  `context.attributes.name`. Run several distinct activities by using different
  names.
- **`state`** values are plain strings — format numbers/dates yourself and parse
  them in SwiftUI (`Double(context.state.data["progress"] ?? "")`).

## Requirements

- iOS **16.2+** at runtime (Dynamic Island 16.1, `update`/`end` 16.2). Older
  devices and other platforms degrade to no-ops.
- .NET 9 (`net9.0-ios`). The package also exposes a `net9.0` no-op target so it
  restores cleanly for Android/Windows heads of a MAUI app.
- Xcode (to build the widget extension and, if you rebuild it, the bridge).

## License

MIT — see [LICENSE](LICENSE).
