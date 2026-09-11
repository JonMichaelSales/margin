# Margin canonical design specification

**Brand decision, 7 September 2026:** Margin. Tagline: **A little room to read.** The visual direction remains the Paper workspace, generous whitespace, and a restrained accent line.

Revision 0.2 · 7 September 2026 · Accepted implementation contract

Margin is a local C#/.NET Avalonia Markdown reader with deliberate editing. Every file opens as a rendered document. Editing changes an in-memory source buffer; only Save or Save as writes that buffer to disk. Switching to Read never saves or discards changes.

This revision incorporates the accepted SkinManager and installer plan. Implementation evidence is maintained separately in [verification.md](verification.md); requirements below are not claims that every acceptance check has passed.

## 1. Product behavior

- One document per window. Additional files open additional windows. An empty launch shows Open and drag/drop guidance.
- Read, Edit, and Split are explicit modes with a persistent unsaved indicator. A narrow Split workspace provides Read/Edit mode controls when two useful columns cannot fit.
- The Paper shell is canonical: quiet toolbar, centered reading measure, optional outline, optional typography inspector, restrained status footer.
- No automatic document saves, recovery copies, recent-file list, session restoration, or document-content persistence in v1.
- Settings contain appearance, future-document reading defaults, editor typography, and window geometry. They are stored in Margin's application settings directory, outside document folders and Mac app bundles.
- Reading adjustments are window-specific. “Use as default” explicitly changes future-document preferences.
- Personal testing is the first distribution scope. Manual updates, unsigned Windows installers, and ad-hoc Mac signing are sufficient for that scope. Public stores, certificate purchases, notarization, and automatic updates are deferred.

## 2. Visual language and layout

The document is the primary surface. The toolbar places the Margin symbol only beside the document title, without repeating the wordmark or tagline. Chrome uses Inter at 14 DIP. Read, Edit, Split, Open, and saving actions retain clear text labels. Secondary actions (Outline, Typography, Find, Focus, Appearance) may collapse from icon-and-text controls to icons with tooltips when space is limited. The default reading face is Georgia at 18 DIP, line height 1.65, paragraph gap 0.8 em, and approximately 68 characters per line.

Window defaults are 1280 × 860 DIP, bounded by the active screen's work area. Minimum workspace size is 720 × 480 DIP. The outline uses approximately 208 DIP; the typography panel approximately 272 DIP. Secondary toolbar actions can compact before wrapping remaining controls at small widths. Exact fit thresholds remain an implementation decision. Split requires enough room for both source and preview. Focus hides optional panels while retaining an obvious Exit focus action.

Control spacing uses a 4-DIP rhythm, generally 8–24 DIP gaps. Buttons have a 34-DIP minimum height, rounded corners, visible hover and keyboard focus states, and distinguishable disabled text. Compact icon controls retain accessible action names and tooltips available to pointer and keyboard users. Their meaning and selected state remain discoverable without relying only on color. Window geometry is restored only inside an available display area. See [asset placement](asset-plan.md) for the brand and interface asset workshop record; new artwork and its generation methods remain undecided.

Paper, Slate, and Studio are appearance families, not separate application layouts:

| Family | Light | Dark | Character |
|---|---|---|---|
| Paper | Margin Paper Light | Margin Paper Dark | Warm neutral surfaces and periwinkle accents |
| Slate | Margin Slate Light | Margin Slate Dark | Cool neutral surfaces and blue accents |
| Studio | Margin Studio Light | Margin Studio Dark | Soft neutral surfaces and violet accents |

Changing a skin must preserve reading/editor typography, panel arrangement, document bytes, dirty state, undo history, caret, selection, and reading position.

## 3. SkinManager integration

Pin AvaloniaSkinManager 2.0.1. Create one service provider, register `AddThemeManagerServices()`, resolve `ISkinManager`, load all 12 package skins, explicitly register the six application skins, then restore appearance before displaying the first window. The original catalog remains intact. Application copies retain palette/font metadata and omit `ControlThemeUris` and `StyleUris`, so package controls cannot replace Margin templates.

`IThemeSelectionStore` is replaced with Margin's preferences adapter. The package's shared AvaloniaThemeManager settings folder is never used to persist Margin choices. The name-based `ApplySkin` overload is not used for previews because it persists selection. Object-based application is checked through `SkinChanged` and resource validation, since the package can catch internal failures.

Every application brush comes from these named resources:

| Role | Resource |
|---|---|
| Document background | `BackgroundBrush` |
| Toolbar, outline, inspector, secondary surfaces | `BackgroundLightBrush` |
| Primary control surface | `PrimaryColorBrush` |
| Selected/secondary control surface | `SecondaryColorBrush` |
| Links, accent, focus | `AccentBlueBrush` |
| Primary text | `TextPrimaryBrush` |
| Secondary text | `TextSecondaryBrush` |
| Borders | `BorderBrush` |
| Semantic states | `ErrorBrush`, `WarningBrush`, `SuccessBrush` |

The accent resource retains its package name for periwinkle and violet palettes. AXAML uses DynamicResource bindings. Custom drawing resolves these same brushes and invalidates drawing caches after a skin change. Application aliases derive solely from package resources; for example, the accent-button foreground chooses a contrasting existing text/background color. Content images retain their original colors.

Literal application colors are allowed only in skin definitions and test fixtures. `scripts/Test-Colors.ps1` audits application C#/AXAML. This source audit supplements actual rendered contrast/state tests; it does not prove all inherited platform-template colors are correct.

The custom Appearance dialog has Follow system, an Margin group, an Included skins group, named preview tiles, a current-selection indicator, keyboard navigation, and Preview/Apply/Cancel behavior. Follow system maps to Paper Light/Dark. Cancel, Escape, and window close restore the original appearance. Apply persists only after successful application. A missing saved skin falls back to Follow system.

## 4. Screen contracts

| State | Required behavior |
|---|---|
| Empty | Open button, drag/drop target, no invented recent files or restored content |
| Read, clean | Rendered source, optional outline/Find/type controls, explicit Edit action |
| Typography | Font family, 12–32 DIP size, 1.2–2.2 line height, 0–1.8 em paragraph gap, 48–100 character measure, natural/justified alignment, optional first-line indentation |
| Edit | Lazy-created AvaloniaEdit, source formatting, undo/redo, separate editor preferences |
| Split | Source and native preview, source-span synchronization, debounced preview parsing |
| Read, dirty | Current unsaved buffer rendered; Save and unsaved indicator remain visible |
| Conflict/failure | Buffer preserved; clear error; reload or Save as available; no silent overwrite |
| Closing dirty | Save / Discard changes / Cancel; failed save prevents closing |
| Appearance | Temporary preview with Apply/Cancel and accessible skin names |
| Focus | Optional panels hidden; clear return to normal workspace |

Native Open, command-line paths, Explorer/Finder activation, and drag/drop feed the same Read-mode opening flow. Markdown links open new document windows. External HTTP/HTTPS/mailto links use the platform launcher; unsupported executable URI schemes are rejected. HTML is displayed as inert text and never executed.

## 5. Architecture

| Project | Responsibility |
|---|---|
| MDPlayer.Core | Document session, revisions, source buffer, reading/settings contracts, text navigation |
| MDPlayer.Infrastructure | Encoding-aware file I/O, conflict checks, atomic replacement, application preferences |
| MDPlayer.Rendering | Markdig parsing, revision-tagged blocks, source spans, native layout, selection, image loading |
| MDPlayer.Desktop | Avalonia windows, editor, appearance, dialogs, platform activation, composition root |
| MDPlayer.Tests | File safety, preferences, parser, skin integration, native headless rendering, workspace tests |
| MDPlayer.Benchmarks | Reproducible parser measurements; packaged UI benchmarks are tracked separately |

Dependencies are centrally pinned: SDK 10.0.303, Avalonia 11.3.13, AvaloniaSkinManager 2.0.1, AvaloniaEdit 11.4.1, Markdig 1.3.2, CommunityToolkit.Mvvm 8.4.2, and matching Avalonia.Headless tests. NuGet uses the public feed explicitly and dependency lock files. Tmds.DBus.Protocol is pinned to the compatible patched 0.21.3 after restore identified the vulnerable transitive 0.21.2 baseline.

### Reader

The reader uses native Avalonia text layout over Markdig blocks, not a browser. CommonMark, pipe tables, tasks, strikethrough, autolinks, nested lists, code, and safe literal HTML are represented in a document model. Parse work runs in the background; stale document/revision results cannot replace newer content. Editing preview parsing is debounced approximately 120 ms.

Visible blocks and bounded offscreen preparation drive layout. Document-wide text/selection/source/accessibility models remain independent of realized drawing. Selection must cross paragraphs and support copy, keyboard movement, Unicode, combining marks, RTL, and long lines. Outline and Find navigate into the document. Typography, wrapping, and image reflow preserve logical reading anchors.

Local images resolve relative to the document. Remote images require an explicit action and are not persisted. Image loading must be asynchronous, bounded, cancellable when replacing/closing a document, and display errors without losing text.

The first renderer milestone must demonstrate continuous selection, Unicode/RTL, accessibility, source mapping, and large-document behavior. A compiling renderer or screenshot alone does not satisfy this gate. Remaining gaps must be recorded explicitly rather than replaced with a browser or omitted from acceptance.

### File lifecycle

The original source buffer is the authority for saving. Never serialize the AST back to Markdown. Preserve encoding, BOM, and exact existing newline sequences. Save as preserves source encoding independently of the destination conflict revision.

Capture a buffer revision, recheck the disk revision, write and flush a unique temporary sibling, recheck the destination, then use platform replacement. A failed write/replacement leaves the original and buffer available. A saved older revision must not clear newer edits. Concurrent writers, deleted files, locked destinations, full disks, permission failures, and unavailable storage are test cases. No portable file replacement API supplies a general compare-and-swap against uncooperative external writers; the final-check race must remain documented and tested as far as the platform permits.

## 6. Distribution

Publish self-contained Release builds for win-x64, win-arm64, osx-x64, and osx-arm64. Start with ReadyToRun enabled, trimming disabled, and multi-file output. Every artifact has a version, architecture, source/commit identity, dependency notices/licenses, and SHA-256 checksums. No separately installed .NET runtime is required.

Windows targets Windows 11. Inno Setup 6.4.3 is pinned by compiler hash. Separate architecture installers default to per-user installation, provide Start Menu and optional desktop shortcuts, register available Markdown handlers without changing default associations, preserve settings during upgrades/ordinary uninstall, and never remove user Markdown files. A shared application mutex prevents replacing/uninstalling a running application; the installer does not force-close unsaved work.

Mac targets macOS 14 or later. Separate Intel and Apple Silicon DMGs contain Margin.app and an Applications shortcut. The app bundle includes its icon, versions, Markdown associations, native activation, and Command-key behavior. Personal builds use ad-hoc signing. Packaging never disables Gatekeeper or strips quarantine. Mac bundle/DMG creation and native verification take place in the later MacBook session.

## 7. Acceptance and performance

Qualify every file-opening entry point; all 18 skins in Read/Edit/Split and dialogs; Preview/Apply/Cancel/system appearance and settings isolation; buffer/undo/selection invariants; actual contrast and high contrast; continuous selection and accessibility; long lines, tables, code, images, Unicode/RTL/IME; save failure/concurrent editing/external writers; dirty-window close; narrow/standard/ultrawide/Retina/scaled layouts; and install/upgrade/uninstall/associations/paths with spaces/no-runtime operation for every target.

| Measure | Target |
|---|---|
| Cold launch p95 | ≤ 700 ms |
| Warm open 100 KB / 1 MB | ≤ 100 / 250 ms |
| 10 MB first readable viewport | ≤ 1 second |
| Scrolling p95 frame time at 60 Hz | ≤ 16.7 ms |
| Typography response / typing echo | ≤ 50 / 30 ms |
| Idle private memory / 1 MB document | ≤ 120 / 220 MB |

Measure packaged Release builds and record hardware, sample counts, first content versus completed parsing, and native versus emulated execution. Parser-only timing is not an open/startup/frame measurement. Theme changes must not reparse Markdown or recreate the editor.

## 8. Delivery gates

1. Revised design, solution, central versions, locks, build/test scripts.
2. Verified SkinManager initialization, 18 palettes, named resources, isolated settings, custom picker.
3. Native reader gate with real files, selection, Find/outline/type controls, accessibility and performance evidence.
4. Editor/Split/formatting/undo, safe saves, conflicts and dirty guards.
5. Windows x64/ARM64 installer builds, installation lifecycle qualification, actual architecture evidence.
6. MacBook handoff, Mac packages, platform checks, native/emulated qualification separately.

The verification record distinguishes source implemented, built, installed, headless tested, natively tested, and pending. The HTML design prototype's offline checks are design evidence only.
