# iOS 27: popups crash with `CALayerInvalidGeometry` because `UIScreen.ApplicationFrame` is NaN

## Description

On iOS 27, showing any Mopups popup with the default settings crashes the app:

```
*** Terminating app due to uncaught exception 'CALayerInvalidGeometry', reason: 'CALayer position contains NaN: [201 nan].
Layer: <CALayer: ...; delegate = <Microsoft_Maui_Platform_ContentView: ...>'
```

On iOS 27, `UIScreen.MainScreen.ApplicationFrame` returns NaN for its Y origin and height, from launch onwards. Measured on the iPhone 18 Pro simulator (iOS 27.0):

```
ApplicationFrame {{0, nan}, {402, nan}}   Bounds {{0, 0}, {402, 874}}
```

The same happens on an iPhone 15 Pro running iOS 27. On an iOS 26 simulator, `ApplicationFrame` is valid (`{{0, 54}, {402, 820}}`) and the popup shows correctly.

`PopupPageRenderer.ViewDidLayoutSubviews` (`Mopups.Platforms.iOS`) builds `PopupPage.SystemPadding` from that frame. The top is `ApplicationFrame.Top`, and the bottom is `ApplicationFrame.Bottom - Height - Top` plus the keyboard height. Both become NaN. `PopupNavigation.PushAsync` then copies `SystemPadding` into `page.Padding` when `HasSystemPadding` is true (the default). The popup content gets a NaN Y, and Core Animation throws.

## Steps to reproduce

1. Build this project with Xcode 27 (iOS 27 SDK) and run it on an iOS 27 simulator or device. An iOS 26 simulator does not reproduce it.
2. The main page shows `UIScreen.ApplicationFrame`, with NaN Y and height.
3. Tap **Show popup**.

**Expected:** a centred popup.
**Actual:** the app crashes with the `CALayerInvalidGeometry` exception above.

## Suggested fix

Compute the system padding from the safe-area insets of the popup's window (or its view) instead of `UIScreen.ApplicationFrame`, which has been deprecated since iOS 9.

## Related

`iOSMopups.AddAsync` only gives the `PopupWindow` a window scene when a connected scene is `ForegroundActive`; otherwise it creates the window without one. Under the UIScene lifecycle, which the iOS 27 SDK requires, a window without a scene is never shown. As a result, a popup pushed while the scene is still foreground-inactive, for example during launch, never appears. Using the first connected `UIWindowScene` whatever its activation state would avoid that.

## Environment

- Xcode 27, iOS 27 SDK
- iPhone 18 Pro simulator (iOS 27.0) and iPhone 15 Pro (iOS 27)
- .NET 10 (workload set 10.0.401.1, iOS workload 27.0.10722)
- Microsoft.Maui.Controls 10.0.x
- Mopups 1.3.4 (1.3.1 behaves the same)
- The app uses the UIScene lifecycle (`UIApplicationSceneManifest` and a `MauiUISceneDelegate`), as the iOS 27 SDK requires.
