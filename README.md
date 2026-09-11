# Margin

**A little room to read.**

A native C# / Avalonia Markdown reader and explicit editor. Personal testing release **0.1.2**. The application is implemented and under qualification; the complete acceptance matrix is not yet passed.

Files open in rendered **Read** mode. Choose **Edit** or **Split** to change the buffer, then **Save** deliberately. Appearance and reading preferences are stored separately from documents. There is no autosave, recent-file history, document recovery, or session restoration.

The Paper workspace includes an outline, Find, focus mode, and optional typography controls. Margin's own Appearance dialog provides six Paper/Slate/Studio skins plus the twelve AvaloniaSkinManager palettes, with preview, Apply, Cancel, and Follow system. Reading typography remains independent of the selected skin.

## Build and run

Install .NET SDK **10.0.303** and PowerShell 7. Dependencies and all four runtime graphs are pinned in central versions and package locks. The self-contained runtime baseline is **10.0.11**.

For Visual Studio, use **Visual Studio 2026 (18.0 or later)** and open `MDPlayer.slnx`; set `MDPlayer.Desktop` as the startup project. Visual Studio 2022 cannot load this pinned SDK: it requires MSBuild 18.0 or later, while VS2022 provides MSBuild 17.x. This can appear as “Microsoft.NET.Sdk could not be found” even when the SDK is installed. Workload updates and VS2022 repairs do not resolve the version mismatch. See [Microsoft's SDK and Visual Studio compatibility table](https://learn.microsoft.com/en-us/dotnet/core/porting/versioning-sdk-msbuild-vs).

The command-line build below works without Visual Studio. Keep `global.json` and the `net10.0` target together; supporting VS2022 would require a deliberate framework/SDK retarget and requalification.

```powershell
./scripts/Build.ps1
dotnet run --project src/MDPlayer.Desktop --no-restore
dotnet run --project src/MDPlayer.Desktop --no-restore -- "C:/Documents/My notes.md"
```

The test suite uses both core tests and Avalonia headless tests with real Skia text/compositor rendering. Passing it does not establish native screen-reader, IME, installation, or performance qualification.

## Personal test installers

On Windows, install the exact Inno Setup compiler recorded in `packaging/windows/toolchain.json`, then run:

```powershell
./scripts/Package-Windows.ps1 -Architecture x64
./scripts/Package-Windows.ps1 -Architecture arm64
```

Installers are written to `artifacts/installers`, with SHA-256 sidecars. They install per user, include .NET, and register Margin as an available Markdown handler without changing the default handler. Ordinary uninstall preserves settings and Markdown documents. The application must be closed before updating or uninstalling.

Mac app bundles and DMGs will be built and exercised in the later MacBook session. See [the Mac handoff](design/macbook-handoff.md). The packaging script requires macOS and uses local/ad-hoc signing.

## Project boundaries

| Project | Responsibility |
|---|---|
| `MDPlayer.Core` | Document revisions, explicit save state, preferences, Unicode navigation |
| `MDPlayer.Infrastructure` | Encoding-preserving files, conflict checks, atomic replacement, isolated settings |
| `MDPlayer.Rendering` | Markdig structure, native block drawing, selection, accessibility model, image loading |
| `MDPlayer.Desktop` | Windows, AvaloniaEdit, custom appearance picker, SkinManager integration, platform activation |
| `MDPlayer.Tests` | Document, theme, renderer, workspace, settings, failure-path tests |
| `MDPlayer.Benchmarks` | Parser timing harness; not an end-to-end UI benchmark |

Application colors resolve SkinManager resources. Palette literals are confined to the six skin definitions; `scripts/Test-Colors.ps1` audits application source. Upstream license notices are included in published artifacts.

Read [the canonical design](design/canonical-design.md), [implementation status](design/implementation-status.md), and [verification record](design/verification.md) before interpreting this as a finished release. The native renderer includes semantic tables, exact inline source mapping, document-wide selection, accessible structure, images and keyboard links. Remaining work includes native accessibility and IME qualification, complete visual/contrast testing, platform installation coverage, and packaged performance measurements.
