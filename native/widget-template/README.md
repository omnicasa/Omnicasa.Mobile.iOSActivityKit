# Widget extension template

A Live Activity's UI **must** live in an iOS *widget extension* (`.appex`) — MAUI
cannot produce one, so every consuming app supplies its own. This folder is a
minimal, ready-to-customize template.

| File | What it is | Do you edit it? |
|------|------------|-----------------|
| `GenericActivityAttributes.swift` | The shared state contract. **Must stay byte-identical** to the copy compiled into `iOSActivityKitBridge.xcframework`. | ❌ Never |
| `SampleLiveActivity.swift` | Your Lock Screen + Dynamic Island SwiftUI. | ✅ This is your UI |
| `SampleWidgetBundle.swift` | `@main` entry point listing the widgets. | Rename only |
| `Info.plist` | Widget-extension Info.plist. | As needed |

## Create the extension in Xcode

You build the extension once in Xcode; the MAUI build then embeds its output.
This mirrors the proven `AdditionalAppExtensions` approach.

1. **New Xcode project** (or target) → *Widget Extension*. **Uncheck**
   "Include Live Activity" (we provide our own files) and **uncheck**
   "Include Configuration App Intent". Name it e.g. `MyWidgetExtension`.
2. Delete the auto-generated `*.swift` files and **add the four files from this
   folder** to the target.
3. Set the extension's **Deployment Target to iOS 16.2** (Dynamic Island needs
   16.1, `update`/`end` need 16.2).
4. Give the extension a bundle id **nested under your app's**, e.g.
   app `com.acme.myapp` → extension `com.acme.myapp.MyWidgetExtension`.
5. Build the extension target (Product → Build) for both *Any iOS Device* and a
   simulator. Note the `Build/Products/...` output path — the MAUI `.csproj`
   points `<AdditionalAppExtensions>` at it.

## Required app-side Info.plist key

In your **MAUI app's** `Platforms/iOS/Info.plist` add:

```xml
<key>NSSupportsLiveActivities</key>
<true/>
```

(Optionally `NSSupportsLiveActivitiesFrequentUpdates` for high-frequency updates.)

## Wire it into the MAUI `.csproj`

```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
  <!-- The native control bridge (objc) the NuGet talks to -->
  <NativeReference Include="path/to/iOSActivityKitBridge.xcframework">
    <Kind>Framework</Kind>
    <SmartLink>true</SmartLink>
  </NativeReference>
</ItemGroup>

<!-- Embed the Xcode-built widget extension -->
<ItemGroup Condition="'$(Configuration)|$(TargetFramework)'=='Debug|net9.0-ios'">
  <AdditionalAppExtensions Include="path/to/MyWidgetExtension/xcode/project/dir">
    <Name>MyWidgetExtension</Name>
    <BuildOutput>DerivedData/MyWidgetExtension/Build/Products/Debug-iphonesimulator</BuildOutput>
  </AdditionalAppExtensions>
</ItemGroup>
<ItemGroup Condition="'$(Configuration)|$(TargetFramework)'=='Release|net9.0-ios'">
  <AdditionalAppExtensions Include="path/to/MyWidgetExtension/xcode/project/dir">
    <Name>MyWidgetExtension</Name>
    <BuildOutput Condition="!$(RuntimeIdentifier.Contains('simulator'))">DerivedData/MyWidgetExtension/Build/Products/Release-iphoneos</BuildOutput>
    <BuildOutput Condition="$(RuntimeIdentifier.Contains('simulator'))">DerivedData/MyWidgetExtension/Build/Products/Release-iphonesimulator</BuildOutput>
  </AdditionalAppExtensions>
</ItemGroup>
```

See the repository root `README.md` for the C# usage.
