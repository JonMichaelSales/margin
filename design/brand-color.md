# Margin brand color

8 September 2026. The user selected periwinkle in place of teal for the application and icon identity.

| Role | Color |
|---|---|
| Primary brand and operating-system icon tile | `#6674CC` |
| Light-mode interactive accent | `#5261B5` |
| Dark-mode interactive accent | `#ADB8FF` |
| Light-mode selected surface | `#E5E7F7` |
| Dark-mode selected surface | `#303653` |
| Warm paper | `#FCFBF7` |

Paper Light and Paper Dark carry this default direction. Slate, Studio, and the package's included skins retain their separate identities. Semantic success remains green; warnings and errors retain their meanings. Neutral interface icons use text resources, and selected/accent icons use `AccentBlueBrush`, whose package key remains unchanged. Typography is independent of the palette.

Generated brand concepts use periwinkle. Black interface-icon concepts are shape references for eventual resource-bound vectors, not fixed black production assets. Generation may approximate specified colors; final exports need exact palette matching, optical sizing, and alpha cleanup. Text and installer metadata are typeset separately.

The teal first pass is retained in `generated/2026-09-08-pass-01`. The periwinkle review set is in `generated/2026-09-08-pass-02`. These concepts do not replace the application's production icon files automatically.
