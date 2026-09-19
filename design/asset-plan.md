# Margin asset placement plan

Workshop record · 7 September 2026. This describes asset roles and placement. It does not authorize or claim completion of new artwork, a new icon library, or responsive toolbar implementation.

## Confirmed toolbar decisions

1. Show the Margin symbol only beside the document title. The reading toolbar does not repeat the Margin wordmark or tagline.
2. Secondary actions may collapse from icon-and-text buttons to icons with tooltips when space is limited. The secondary group is Outline, Typography, Find, Focus, and Appearance.

Read / Edit / Split retain their mode labels. Open and saving actions retain text in the working layout. Compact secondary controls keep the same action, accessible name, keyboard access, visible focus, and selected state. Tooltips provide the action name and an existing shortcut where available; keyboard users must be able to discover the same information. Compact presentation does not hide the unsaved indicator or make the return from focus mode ambiguous.

Exact fit thresholds and overflow behavior will be established during layout implementation. No pixel breakpoint or icon style has been selected in this workshop.

## Working asset allocation

The following allocation carries forward the workshop proposal. Only the two toolbar decisions above have received explicit answers; the remaining rows are the working baseline for the asset review.

| Surface | Asset or content role | Existing material / required work |
|---|---|---|
| Windows executable, taskbar, Start Menu, desktop shortcut | Primary Margin application icon | Current teal Markdown document icon is reused; dedicated Margin artwork remains undecided |
| Mac application and Dock | Platform-adapted primary application icon | Existing PNG iconset; final Margin treatment and Mac output remain pending |
| Windows installer executable | Primary application icon | Uses the current application icon |
| Windows installer screens | Symbol, name, tagline in a header; installation instructions | Standard wizard currently; branded header allocation proposed |
| Add/Remove Programs | Primary icon; Margin name; separate version and publisher fields | Margin 0.1.5 uses the primary icon, clean product name, separate version, and publisher fields |
| Associated Markdown files | Distinct document-file icon with a small Margin identifier | Currently uses the application icon; distinct asset missing |
| Mac DMG | App icon, Applications shortcut, short installation instruction | Packaging structure exists; background/layout artwork is undecided |
| Main toolbar | Symbol only beside document title | Confirmed; current `m↓` text mark is preliminary |
| Empty Open screen | Symbol, Margin wordmark, tagline, Open action | Name/tagline text exist; dedicated symbol/wordmark artwork missing |
| About dialog | Symbol/wordmark, tagline, version, publisher, licenses, support link | Proposed surface; dedicated dialog and asset placement remain pending |
| Appearance picker | Live skin previews and current-selection indicators | Generated from actual resources; no static preview thumbnails needed |
| Status and confirmation dialogs | Small status symbol where useful, with explanatory text | Dedicated status icon family remains undecided |

Support/about links need real destinations before inclusion. An icon reference in installation metadata is distinct from the executable actually being present; the missing installation directory found during the inventory is a separate issue, outside this asset-planning step.

## Interface asset roles

| Group | Working presentation |
|---|---|
| Read / Edit / Split | Labeled segmented mode controls |
| Open / Save / Save as | Icon with text |
| Outline / Typography / Find / Focus / Appearance | Icon with text normally; compact icons with tooltips permitted |
| Undo / Redo / Bold / Italic / Heading / Quote / List / Code / Link | Consistent compact action icons or typographic symbols; tooltips and accessible names |
| Close / Previous / Next / Expand / Collapse | Utility icons |
| Unsaved / external change / warning / error / success | Status indicators paired with explanatory text |

Application interface assets must use SkinManager resources for their colors. Primary operating-system application/file icons are separate exported assets; their visual treatment will be decided in the next stage. Images supplied by Markdown documents remain content, not application branding.

## Decisions reserved for the next stage

- Margin symbol concept and relationship to the wordmark.
- Icon shapes, stroke/fill treatment, detail, and small-size variants.
- Relationship between platform app icons, document icons, and in-app symbols.
- Installer header and DMG presentation style, including whether they need dedicated background artwork.
- Asset sources, libraries, licenses, generation methods, editable masters, and export formats.

Splash screens, decorative reading backgrounds, photographs, and onboarding illustrations are outside the proposed initial asset set. No artwork is generated or replaced by this record.
