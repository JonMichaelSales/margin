# Margin implementation record

Updated 19 September 2026. Personal test version 0.1.5.

## Implemented

- Six-project C# / Avalonia solution, SDK 10.0.303, runtime 10.0.11, central package versions, and dependency locks for all four target runtimes.
- DI integration with AvaloniaSkinManager 2.0.1; twelve supplied palettes plus six complete Margin Paper/Slate/Studio definitions; custom picker with temporary preview, Apply, Cancel, and Follow system; isolated preference storage.
- Named dynamic brush resources, Margin button templates, Fluent template aliases, editor syntax/selection resources, source color audit, and palette rollback after failed preview notification.
- Native Markdig block reader with bounded visible-layout caching, whole-document selection, copy, Unicode navigation, first-strong RTL paragraph direction, outline, Find, local images and explicit remote-image requests.
- Semantic pipe-table rows and cells with native grid drawing, rich inline styles, links and images; cell-level automation peers; keyboard link traversal; exact rendered-text/source mappings retained through formatting and long-block chunking.
- Encoded-size, format-header, decoded-dimension and pixel-count checks before image decode. Image caches survive same-document preview revisions so typing does not repeatedly reload images.
- Staged first-section display for large documents followed by revision-checked background indexing. Select all waits for complete text. Long paragraphs are split at grapheme boundaries to bound individual native layouts.
- Typography controls, character-based reflow anchors, optional panels and focus mode; lazy AvaloniaEdit, Read/Edit/Split, source formatting in undo transactions, source Find, and span-based block navigation.
- Explicit save/save-as, BOM/encoding/newline preservation, external-change checks, sibling temporary writes and replacement, dirty guards, and preservation of edits after save failures or file deletion.
- Command-line, Open, drag/drop, and platform file-activation routes; one document per window; no document autosave or history.
- Per-user Inno Setup packaging for x64/ARM64; self-contained publishing, icon assets, notices and hashes; macOS bundle/DMG scripts and later-session handoff.
- Windows Default Apps capabilities for `.md` and `.markdown`, an unchecked setup option that opens Margin's system association page, and an in-app Default app command. Windows retains the explicit user choice.

## Verification so far

The Release build has zero warnings/errors and **49 automated tests pass**. The tests include actual Skia compositor drawing, semantic table rendering and cell accessibility, exact bidirectional source mapping, rich table content, keyboard link traversal, Windows default-app routing, PNG/JPEG/GIF/BMP/WebP header safety, all 18 palettes across Read/Edit/Split, editor identity/undo/selection preservation, continuous reader selection, encoding round trips, settings isolation, failed theme rollback, deferred whole-document selection, and migration of all six saved application skin names to Margin.

Margin 0.1.4 x64 was built from a clean source commit, checksum-verified, installed for the current user, and launched natively with the canonical design. Its installed application and renderer binaries match the publish output; Add/Remove Programs, the Start Menu shortcut, and the quoted Markdown handler were verified. The earlier 0.1.2 x64 upgrade/uninstall/reinstall cycle passed while preserving settings, fixture bytes, and default associations, and a 0.1.2 ARM64 installer was built. No Windows ARM64 execution or Mac execution has been performed. See [verification.md](verification.md).

## Required work still open

These are acceptance gaps, not silently relaxed requirements:

1. Accessibility exposes document text, persistent block/heading peers, semantic table/cell peers, and keyboard-reachable links. Native Narrator/VoiceOver reading and text-range behavior, IME composition, and the full RTL interaction matrix remain unqualified.
2. Format-header and decoded-size limits are implemented for PNG, JPEG, GIF, BMP and WebP. Complete image-rich stress tests and native supported-format behavior still need qualification.
3. Verify every control state, dialogs, focus, disabled controls, system high contrast, all included palettes, and narrow/ultrawide/scaled layouts against actual rendered contrast and interaction requirements.
4. Complete native Open/drop/Explorer/Finder entry-point coverage, dirty-window interaction tests, real disk-full/unavailable-storage scenarios, concurrent external writers and multi-window behavior.
5. Measure packaged startup/open/scroll/typing/reflow/memory distributions against the canonical targets. Parser-only results are diagnostic evidence, not acceptance results.
6. Complete Windows installation on clean machines without a separately installed .NET runtime and native ARM64 execution. Produce both Mac DMGs and record native versus emulated testing during the later MacBook session.

The native renderer implementation milestone is complete in source and headless compositor tests. Overall release qualification remains open until the platform-specific work above is complete.
