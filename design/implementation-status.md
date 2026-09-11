# Margin implementation record

Updated 7 September 2026. Personal test version 0.1.2.

## Implemented

- Six-project C# / Avalonia solution, SDK 10.0.303, runtime 10.0.11, central package versions, and dependency locks for all four target runtimes.
- DI integration with AvaloniaSkinManager 2.0.1; twelve supplied palettes plus six complete Margin Paper/Slate/Studio definitions; custom picker with temporary preview, Apply, Cancel, and Follow system; isolated preference storage.
- Named dynamic brush resources, Margin button templates, Fluent template aliases, editor syntax/selection resources, source color audit, and palette rollback after failed preview notification.
- Native Markdig block reader with bounded visible-layout caching, whole-document selection, copy, Unicode navigation, first-strong RTL paragraph direction, outline, Find, local images and explicit remote-image requests.
- Staged first-section display for large documents followed by revision-checked background indexing. Select all waits for complete text. Long paragraphs are split at grapheme boundaries to bound individual native layouts.
- Typography controls, character-based reflow anchors, optional panels and focus mode; lazy AvaloniaEdit, Read/Edit/Split, source formatting in undo transactions, source Find, and span-based block navigation.
- Explicit save/save-as, BOM/encoding/newline preservation, external-change checks, sibling temporary writes and replacement, dirty guards, and preservation of edits after save failures or file deletion.
- Command-line, Open, drag/drop, and platform file-activation routes; one document per window; no document autosave or history.
- Per-user Inno Setup packaging for x64/ARM64; self-contained publishing, icon assets, notices and hashes; macOS bundle/DMG scripts and later-session handoff.

## Verification so far

The Release build has zero warnings/errors and **40 automated tests pass**. The tests include actual Skia compositor drawing, all 18 palettes across Read/Edit/Split, editor identity/undo/selection preservation, continuous reader selection, encoding round trips, settings isolation, failed theme rollback, deferred whole-document selection, and migration of all six saved application skin names to Margin.

Earlier MDPlayer x64 builds were installed and launched natively. Margin 0.1.2 installers are built for Windows x64 and ARM64. Upgrade from MDPlayer 0.1.1 to Margin 0.1.2, ordinary uninstall, and reinstall passed on Windows x64, preserving settings, fixture bytes, and default associations. The renamed executable metadata, old-handler compatibility, and Start Menu shortcut were checked. The installed Margin UI has not been exercised natively. No Windows ARM64 execution or Mac execution has been performed. See [verification.md](verification.md).

## Required work still open

These are acceptance gaps, not silently relaxed requirements:

1. Tables currently use padded monospaced rows; implement semantic cell layout, cell-level accessibility, and rich inline/image content in cells.
2. Navigation uses block source spans. Exact inline mapping, formatted long-block mapping, bidirectional Split synchronization, and reading anchors across every image/width/edit transition need further work and validation.
3. Accessibility exposes document text and persistent block/heading peers. Native Narrator/VoiceOver reading and text-range behavior, IME composition, keyboard link traversal, and the full RTL interaction matrix remain unqualified.
4. Remote image permission and encoded-size limits are implemented. Complete decoded-dimension limits, image-rich stress tests and supported-format behavior need qualification.
5. Verify every control state, dialogs, focus, disabled controls, system high contrast, all included palettes, and narrow/ultrawide/scaled layouts against actual rendered contrast and interaction requirements.
6. Complete native Open/drop/Explorer/Finder entry-point coverage, dirty-window interaction tests, real disk-full/unavailable-storage scenarios, concurrent external writers and multi-window behavior.
7. Measure packaged startup/open/scroll/typing/reflow/memory distributions against the canonical targets. Parser-only results are diagnostic evidence, not acceptance results.
8. Complete Windows installation on clean machines without a separately installed .NET runtime and native ARM64 execution. Produce both Mac DMGs and record native versus emulated testing during the later MacBook session.

The renderer milestone and overall release qualification remain open until this work is complete.
