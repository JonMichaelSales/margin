# Margin verification record

Recorded through 11 September 2026. This record distinguishes implemented source, automated tests, native execution, and installed packages. Earlier browser/prototype checks do not count as native application evidence.

## Current build checks

`scripts/Build.ps1 -Configuration Release` completed with a locked restore, zero build warnings/errors, **48 passed tests**, and a successful fixed-color audit. Six migration cases verify that previously saved MDPlayer skin selections become their corresponding Margin selections without rewriting the settings file or changing reading/window preferences.

Test output: `tests/MDPlayer.Tests/TestResults/MDPlayer.trx` (generated locally, excluded from Git). Tests exercise the real Avalonia/Skia compositor in a headless platform, including scrolling and font changes that previously exposed a render invalidation crash. That crash was fixed by deferring measure invalidation out of the render callback.

Coverage includes six encoding/BOM variants, mixed newlines, external modification and deletion, injected replacement failure, Save as encoding retention, saving an older revision, corrupt/settings round trips, all 18 palettes, Read/Edit/Split editor preservation, document-wide selection, emoji/combining navigation, persistent accessibility peers, large-block chunking, staged indexing, and failed-preview resource rollback.

Renderer milestone coverage now includes native semantic table cells, rich inline/link/image table content, table-cell automation peers, exact rendered-text/source mapping through Markdown delimiters and 8K grapheme-safe chunks, source-based navigation in both Split directions, keyboard link traversal, and pre-decode PNG/JPEG/GIF/BMP/WebP dimension limits. A headless Skia frame exercises the table drawing path. A native Windows development build opened the canonical design after these changes; native screen-reader and IME behavior remain separate qualification work.

Palette contrast tests cover primary/secondary/link colors of the six Margin skins. They do not establish complete rendered contrast for every supplied palette or control state.

## Native Windows evidence

- Native Windows x64 development build opened the canonical design in rendered Read mode and remained running after the compositor fix.
- Version 0.1.0 x64 self-contained installer was built and installed successfully under the current user's LocalAppData. Installed application launched to the empty Open screen.
- The native Open dialog appeared. The Windows UI helper could not reliably target its editable field, so choosing a file through that dialog remains unverified. The user closed the application/dialog to permit lifecycle tests.
- MDPlayer 0.1.1 x64 and ARM64 installers finished building. Its x64 upgrade/uninstall/reinstall passed; native UI automation was stopped by the user before final installed interaction checks.
- Margin 0.1.2 x64 and ARM64 installers built successfully. The x64 upgrade from MDPlayer 0.1.1, uninstall, and reinstall all passed. Settings and document fixture SHA-256 values and default associations were unchanged. The legacy apphost was removed; existing handler commands resolve to `Margin.exe`; installed product metadata is Margin; the Margin Start Menu shortcut points to the new executable.
- Rename evidence is in `artifacts/qualification/margin-0.1.2-windows-lifecycle.json` and corresponding versioned installer logs. The earlier `windows-lifecycle.json` remains the MDPlayer 0.1.0 → 0.1.1 record.
- The installed Margin UI has not been exercised natively. No screen-reader, IME, complete control-state contrast, or packaged performance qualification is implied by these installer checks.

Qualification fixtures live under `artifacts/qualification`, outside personal document folders. Installer lifecycle tests compare fixture/settings hashes and default association values before and after each operation.

## Artifact matrix

| Target | Built | Installed | Native qualification |
|---|---|---|---|
| Windows x64 | Margin 0.1.2 yes | Margin 0.1.2 yes; upgrade/uninstall/reinstall passed | Earlier MDPlayer launch/read smoke evidence; Margin native interaction and full matrix open |
| Windows ARM64 | Margin 0.1.2 yes | No | No ARM64 hardware execution |
| Mac Intel | No | No | Later Mac session; Rosetta is not native Intel evidence |
| Mac Apple Silicon | No | No | Later MacBook session |

Artifacts include version, runtime, source commit/dirty state, source hashes, dependency notices, bundled runtime licenses, and SHA-256 checksums. No certificate purchase, public signing, notarization, or automatic update infrastructure is included.

## Performance

Hardware observed: Intel Core i9-13900H, 14 cores / 20 logical processors, Windows x64, .NET 10.0.11. The earlier parser-only run used 30 samples at each size and measured UTF-16 source characters, not file bytes:

| Source characters | Parser p50 | Parser p95 |
|---|---:|---:|
| 100,032 | 18.46 ms | 25.76 ms |
| 1,000,032 | 87.23 ms | 146.45 ms |
| 10,000,032 | 2,383.90 ms | 2,933.15 ms |

Those measurements preceded staged loading and bounded paragraph chunks. The 10 MB-scale result motivated showing the first section before complete parsing. It does not prove that the first readable viewport meets one second.

No packaged p95 launch/open/frame/typing/reflow or memory target has passed qualification. Individual process memory observations are not representative sample distributions.

## Outstanding acceptance

See [implementation-status.md](implementation-status.md) for the remaining native accessibility, image stress, contrast, platform and performance qualification. The source and Windows test packages are preliminary; the complete accepted implementation plan is not yet finished.
