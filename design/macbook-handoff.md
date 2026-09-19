# MacBook build and qualification handoff

Mac packaging is intentionally deferred to the later MacBook session. No DMG generated on another platform counts as Mac installation or native execution evidence.

## Source and tools

Transfer the complete Git checkout at the final Windows source revision, including `global.json`, `Directory.Packages.props`, every `packages.lock.json`, and the committed iconset. Record `git rev-parse HEAD` and `git status --short` before building. Artifact `build-info.json` includes both the commit identity and source hashes, so dirty-source artifacts can be identified.

Required: macOS 14+, .NET SDK **10.0.303**, Xcode command-line tools (`xcode-select -p`), PowerShell 7, Git, `codesign`, `plutil`, `iconutil`, and `hdiutil`. Inno Setup is not required on Mac. There are no server credentials or certificate purchases.

Before restore, record:

```sh
uname -m
sw_vers
dotnet --info
xcode-select -p
git rev-parse HEAD
git status --short
```

## Build and package

Run from the repository root:

```sh
pwsh -NoProfile -File scripts/Build.ps1 -Configuration Release
bash scripts/package-macos.sh osx-arm64 0.1.5
bash scripts/package-macos.sh osx-x64 0.1.5
```

Expected outputs are `artifacts/installers/Margin-0.1.5-osx-arm64.dmg` and `Margin-0.1.5-osx-x64.dmg`, each with a SHA-256 sidecar. The DMG contains Margin.app and an Applications shortcut. Runtime files, dependency notices, licenses, build metadata, and file checksums are inside the bundle. The scripts sign Mach-O binaries and the bundle ad-hoc, verify the signature, create the DMG, and verify its image structure. Packaging does not remove quarantine or alter Gatekeeper policy.

If locked restore fails, inspect the exact SDK/runtime graph discrepancy. Do not silently upgrade Avalonia or regenerate locks to a different package set. The Windows development verification record must not be copied as Mac evidence.

## Qualification checklist

1. Mount the correct architecture DMG, inspect its content, drag the app to Applications, and launch through Finder.
2. Verify empty launch, Retina layout, native title/menu behavior, Command-O/S/Shift-S/F/E/C/A, standard window close, and separate document windows.
3. Open `.md` and `.markdown` through Finder, Open, drag/drop, and paths with spaces/non-ASCII characters. Every route must start in rendered Read mode.
4. Exercise all 18 skins and Follow system, then preview/cancel/close the picker. Typography, selection, dirty state, caret, editor instance, and undo remain intact.
5. Test native IME, mixed RTL/LTR text, cross-paragraph selection, accessibility with VoiceOver, focus, Find, outline, typography, long lines, nested lists, tables, code, and local/explicit remote images.
6. Save/reopen UTF-8 with/without BOM and BOM-marked UTF-16/UTF-32, preserving mixed newlines. Test external writers, deleted/locked/unavailable files and Save as. Never risk a personal document during failure injection; use temporary fixtures.
7. Test dirty-window Save/Discard/Cancel and a failed Save. Confirm ordinary updates/removal preserve settings and unrelated documents.
8. Run packaged performance sampling and record hardware, macOS, build revision, architecture, sample counts and p95 results against the canonical budgets.

An Apple Silicon build running on Apple Silicon is native evidence. An Intel build run under Rosetta is emulated evidence. Native Intel qualification requires an Intel Mac. Record each artifact independently as built, installed, launched, and tested natively or under emulation.
