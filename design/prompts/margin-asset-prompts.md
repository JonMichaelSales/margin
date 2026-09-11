# Margin — Comfy asset prompt pack

Version 2 · 8 September 2026 · 44 self-contained prompts.

These prompts are for concept exploration across image models. No final symbol, wordmark, icon style, or production method has been approved. The starting direction is **page and margin**, a restrained editorial identity, warm off-white, and deep periwinkle. Prompts 02/03 and 07 provide alternatives.

## How to compare
- Copy a prompt unchanged between models; use the listed aspect ratio as a composition target.
- If your workflow has a negative-prompt field, use the supplied negative prompt. Otherwise use the positive prompt alone; it already contains important exclusions.
- Try a few seeds per model and save the model, prompt, seed, and generation settings with each result.
- Use a plain background for clean comparison. Do not ask the model to draw a transparency checkerboard; actual alpha and clean edges are production/export work.
- Once a symbol is selected, use that same image as a reference in reference-capable workflows for the app/file icons and compositions. Matching prompts alone do not ensure matching symbols.
- Compare symbols at small displayed sizes as well as full resolution. “Vector-style” output remains a raster image; approved icons will need vector reconstruction or a suitable consistent library.
- Hex colors communicate intent. Production exports will be color-matched; in-app icon colors will come from SkinManager resources.
- Generate one asset per image. Interface icons here are exploration prompts, not a requirement to generate every production icon separately.

## Reuse and non-generated content
| Surface | Prompt(s) / treatment |
|---|---|
| Windows executable, installer executable, taskbar, Start Menu, desktop shortcut, Add/Remove Programs | Reuse the approved primary app icon from 04 |
| Mac application and Dock | 05, adapted from the same approved symbol |
| Associated Markdown files | 10 |
| Main toolbar | Approved symbol from 01, 02, or 03; no wordmark or tagline beside the document title |
| Welcome screen and About dialog | Reuse approved symbol and wordmark; 09 explores composition |
| Installer header | Reuse approved brand assets; 08 explores composition; 11 is an optional background |
| DMG | 12 is an optional background; real app/folder icons and installation instruction are placed separately |
| Appearance skin tiles | Continue rendering live previews from the actual skin resources |
| Read/Edit/Split | Labels stay visible; optional icons have prompts below |
| Open/Save/Save as | Retain text alongside icons |
| Outline/Typography/Find/Focus/Appearance | May compact to icons with tooltips and accessible names |
| Tagline | Typeset exactly **A little room to read.** as live text |
| Product/version/publisher | Typeset **Margin**, the actual build version, and **Jon Sales** in their respective fields |
| Photos, splash art, onboarding illustrations, reading backgrounds | Not part of the current asset allocation |

Prompt 09 intentionally leaves the tagline out of the generated image. Interface states such as hover, pressed, disabled, selected, and focus are rendered through control templates and skin resources; they do not need separate generated pictures.

## Index
- [01-symbol-page-margin — Brand symbol — page and margin concept](#01-symbol-page-margin)
- [02-symbol-monogram — Alternate brand symbol — M monogram](#02-symbol-monogram)
- [03-symbol-reading-marker — Alternate brand symbol — reading marker](#03-symbol-reading-marker)
- [04-windows-app-icon — Primary app icon — Windows composition](#04-windows-app-icon)
- [05-macos-app-icon — App icon — macOS composition](#05-macos-app-icon)
- [06-wordmark-serif — Wordmark — editorial serif exploration](#06-wordmark-serif)
- [07-wordmark-sans — Wordmark — humanist sans-serif exploration](#07-wordmark-sans)
- [08-brand-lockup-horizontal — Horizontal brand composition](#08-brand-lockup-horizontal)
- [09-brand-lockup-stacked — Stacked welcome/About brand composition](#09-brand-lockup-stacked)
- [10-markdown-document-icon — Associated Markdown document icon](#10-markdown-document-icon)
- [11-installer-header-background — Optional installer header background](#11-installer-header-background)
- [12-dmg-background — Optional Mac DMG installation background](#12-dmg-background)
- [13-open — Open](#13-open)
- [14-save — Save](#14-save)
- [15-save-as — Save as](#15-save-as)
- [16-read — Read mode](#16-read)
- [17-edit — Edit mode](#17-edit)
- [18-split — Split view](#18-split)
- [19-outline — Document outline](#19-outline)
- [20-typography — Typography](#20-typography)
- [21-find — Find](#21-find)
- [22-focus — Enter focus mode](#22-focus)
- [23-exit-focus — Exit focus mode](#23-exit-focus)
- [24-appearance — Appearance](#24-appearance)
- [25-undo — Undo](#25-undo)
- [26-redo — Redo](#26-redo)
- [27-bold — Bold](#27-bold)
- [28-italic — Italic](#28-italic)
- [29-heading — Heading](#29-heading)
- [30-quote — Block quote](#30-quote)
- [31-list — Bulleted list](#31-list)
- [32-code — Code formatting](#32-code)
- [33-link — Insert link](#33-link)
- [34-close — Close](#34-close)
- [35-previous — Previous](#35-previous)
- [36-next — Next](#36-next)
- [37-expand — Expand](#37-expand)
- [38-collapse — Collapse](#38-collapse)
- [39-information — Information](#39-information)
- [40-warning — Warning](#40-warning)
- [41-error — Error](#41-error)
- [42-success — Success](#42-success)
- [43-unsaved — Unsaved changes](#43-unsaved)
- [44-external-change — External file change](#44-external-change)

<a id="01-symbol-page-margin"></a>

## 01-symbol-page-margin — Brand symbol — page and margin concept

**Composition:** 1:1. **Used for:** Toolbar; Brand identity exploration.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Design one standalone brand symbol. A single upright page silhouette with a distinctive vertical margin line slightly inset from its left edge, leaving a generous quiet reading area. A small intentional opening or offset in the page outline makes the silhouette recognizable. Reduce the idea to a few broad shapes; avoid detailed text lines, folded corners, and a download arrow. Render the entire mark in solid black on a plain white background, with approximately 20 percent clear space around it. It must remain distinguishable at a very small toolbar size. No tile, lettering, wordmark, caption, UI, or surrounding scene. Deliver one large isolated symbol.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Working direction, not a final approved logo. Use a monochrome symbol so it can later take its color from SkinManager.

<a id="02-symbol-monogram"></a>

## 02-symbol-monogram — Alternate brand symbol — M monogram

**Composition:** 1:1. **Used for:** Alternative identity exploration.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Design one original capital M monogram for Margin. Construct the M from two or three broad geometric strokes, incorporating a quiet vertical reading margin through negative space. Keep the letter immediately recognizable without a surrounding page, book, or download arrow. Render the whole mark in solid black on plain white, with approximately 20 percent clear space. Favor a memorable silhouette and optical balance at very small sizes. No tile, additional words, captions, UI, or surrounding scene. Deliver one isolated symbol.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Alternative to prompt 01, not an additional logo to ship.

<a id="03-symbol-reading-marker"></a>

## 03-symbol-reading-marker — Alternate brand symbol — reading marker

**Composition:** 1:1. **Used for:** Alternative identity exploration.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Design one original reading-marker symbol for Margin. Combine a simple bookmark shape and a restrained page-edge gesture into a single compact silhouette. Evoke a place kept in a book and the quiet space beside a paragraph. Use only a few broad shapes and one deliberate negative-space opening. Solid black mark on plain white, approximately 20 percent clear space. Keep it legible at very small sizes. No letters, words, hearts, stars, download arrows, tile, UI, or surrounding scene. Deliver one isolated symbol.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Alternative to prompt 01, not an additional logo to ship.

<a id="04-windows-app-icon"></a>

## 04-windows-app-icon — Primary app icon — Windows composition

**Composition:** 1:1. **Used for:** Windows executable; Taskbar; Start Menu; Desktop shortcut; Installer executable; Add/Remove Programs.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create one desktop application icon for Margin. Use a muted periwinkle rounded-square tile, with a warm off-white brand symbol centered inside it. The symbol is: A single upright page silhouette with a distinctive vertical margin line slightly inset from its left edge, leaving a generous quiet reading area. A small intentional opening or offset in the page outline makes the silhouette recognizable. Reduce the idea to a few broad shapes; avoid detailed text lines, folded corners, and a download arrow. The central symbol should occupy roughly two thirds of the tile height. Keep the overall silhouette bold, the interior uncluttered, and the margin line strong enough to survive reduction to a small taskbar icon. Place the entire tile within the canvas with a narrow plain-white surround; do not crop its corners. No app name, tagline, readable document text, device mockup, or surrounding UI.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Reuse one approved icon across these Windows surfaces. After selecting a symbol, use it as an image reference when the model/workflow supports references; do not accept a newly invented symbol in each export.

<a id="05-macos-app-icon"></a>

## 05-macos-app-icon — App icon — macOS composition

**Composition:** 1:1. **Used for:** Mac application; Dock; DMG application item.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create one macOS desktop application icon for Margin using a softly rounded-square muted periwinkle tile and a centered warm off-white symbol. The symbol is: A single upright page silhouette with a distinctive vertical margin line slightly inset from its left edge, leaving a generous quiet reading area. A small intentional opening or offset in the page outline makes the silhouette recognizable. Reduce the idea to a few broad shapes; avoid detailed text lines, folded corners, and a download arrow. Use the same restrained identity and bold proportions as a clean Windows version, with balanced inset spacing and a carefully resolved rounded-square silhouette suitable for a Dock icon. Flat colors, crisp geometry, no material effects. Show only the complete icon on a plain white surround, centered and uncropped. No words, screen, Dock mockup, wallpaper, or other application icons.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

An adaptation of the same identity, not a separate Mac logo. Final platform padding and masking are export decisions.

<a id="06-wordmark-serif"></a>

## 06-wordmark-serif — Wordmark — editorial serif exploration

**Composition:** 3:1. **Used for:** Welcome screen; About dialog; Installer brand composition.

**Positive prompt**

```text
Design a refined typographic wordmark containing exactly the single word "Margin", capital M followed by lowercase argin. Calm contemporary editorial serif lettering, moderate stroke contrast, open counters, sturdy small details, thoughtful optical spacing, approachable rather than ornate. Muted periwinkle lettering on a flat warm off-white background. Center the complete word with generous clear space on all sides. Straight-on flat graphic. No symbol, tagline, border, book illustration, mockup, extra letters, or additional text.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration only. Final wordmark should be rebuilt using a chosen licensed typeface or custom lettering; generated lettering is not a font specification.

<a id="07-wordmark-sans"></a>

## 07-wordmark-sans — Wordmark — humanist sans-serif exploration

**Composition:** 3:1. **Used for:** Alternative wordmark exploration.

**Positive prompt**

```text
Design a refined typographic wordmark containing exactly the single word "Margin", capital M followed by lowercase argin. Warm humanist sans-serif lettering with open counters, a clear distinctive lowercase g, restrained medium weight, balanced optical spacing, and friendly precision. Muted periwinkle lettering on a flat warm off-white background. Center the whole word with generous clear space. Straight-on flat graphic. No symbol, tagline, border, book illustration, mockup, extra letters, or additional text.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Alternative to prompt 06, not a second production wordmark.

<a id="08-brand-lockup-horizontal"></a>

## 08-brand-lockup-horizontal — Horizontal brand composition

**Composition:** 4:1. **Used for:** Installer header branding; About branding.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create a single horizontal brand composition. Place a compact muted periwinkle page-and-margin symbol on the left and the exact word "Margin" on the right in restrained contemporary editorial serif lettering. Symbol description: A single upright page silhouette with a distinctive vertical margin line slightly inset from its left edge, leaving a generous quiet reading area. A small intentional opening or offset in the page outline makes the silhouette recognizable. Reduce the idea to a few broad shapes; avoid detailed text lines, folded corners, and a download arrow. Align their optical centers, leave a comfortable gap, and maintain generous outer clear space. Use a flat warm off-white background. Show the complete composition straight on. No tagline, other text, border, software controls, mockup, or illustration.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Layout exploration. In the final application, compose the approved symbol and wordmark assets instead of generating new variants.

<a id="09-brand-lockup-stacked"></a>

## 09-brand-lockup-stacked — Stacked welcome/About brand composition

**Composition:** 1:1. **Used for:** Welcome screen; About dialog.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create a spacious centered vertical brand composition for Margin. At the top place one modest periwinkle page-and-margin symbol; below it place the exact word "Margin" in restrained contemporary editorial serif lettering. Symbol description: A single upright page silhouette with a distinctive vertical margin line slightly inset from its left edge, leaving a generous quiet reading area. A small intentional opening or offset in the page outline makes the silhouette recognizable. Reduce the idea to a few broad shapes; avoid detailed text lines, folded corners, and a download arrow. Keep both elements in the upper two thirds of the canvas and leave a clearly empty lower area for a tagline to be typeset later. Use a flat warm off-white background. Quiet proportions, ample clear space, no enclosing tile. No buttons, UI mockup, scenery, extra lettering, or tagline rendered into the image.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Typeset the exact tagline "A little room to read." separately in Avalonia. This is a composition reference, not a new decorative welcome illustration.

<a id="10-markdown-document-icon"></a>

## 10-markdown-document-icon — Associated Markdown document icon

**Composition:** 1:1. **Used for:** .md files; .markdown files.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create one operating-system file icon for a Markdown document associated with Margin. Use a tall warm off-white page silhouette with one subtle folded top-right corner, a crisp deep periwinkle outline, and a prominent periwinkle vertical margin line near the left edge. Include a small simple page-and-margin brand identifier near the lower portion, clearly subordinate to the document silhouette. Maintain large quiet areas, broad readable shapes, and a clean outer contour. Isolate the complete document on plain white with generous surrounding space. No rounded-square application tile, extension label, words, readable document text, download arrow, or mockup.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

File icon must remain visibly different from the app launcher. Use the selected brand symbol as reference for the small identifier; simplify or omit tiny details in final small-size variants.

<a id="11-installer-header-background"></a>

## 11-installer-header-background — Optional installer header background

**Composition:** 5:1. **Used for:** Windows installer header.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create a very restrained wide background plate for a desktop application's installer header. Flat warm off-white field. Place one short muted periwinkle vertical accent near the far left edge, echoing a page margin, with ample breathing room. Leave almost the entire remaining width completely empty and visually even for a separately placed logo, application name, and installation heading. Flat graphic, clean edges, no gradients, texture, illustration, symbols, words, icons, buttons, screenshot, or frame.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Optional background concept only. Installer copy, logo, and tagline are composed separately. Exact dimensions follow the chosen installer layout.

<a id="12-dmg-background"></a>

## 12-dmg-background — Optional Mac DMG installation background

**Composition:** 3:2. **Used for:** Mac DMG window.

**Positive prompt**

```text
Calm editorial identity for Margin, a fast desktop Markdown reader with deliberate editing. Communicate clarity, breathing room, precision, and a comfortable place to read. Simple intentional geometry, generous negative space, balanced proportions, crisp edges. Restrained warm off-white and muted periwinkle, approximately #FCFBF7 and #6674CC. Flat vector-style artwork, viewed straight on. Create a minimal background plate for a macOS application installation disk image. Flat warm off-white background with a quiet muted periwinkle vertical margin accent close to the far left edge. Reserve an empty top area for a separately typeset title. In the middle, reserve two spacious empty areas at equal height, left and right, for real application and Applications-folder icons to be overlaid later. Between the two areas, draw one small clear periwinkle arrow pointing from left to right. Leave an empty lower area for a short installation instruction. No actual app icon, folder icon, logo, words, window frame, cursor, screenshot, photo, gradient, or texture.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Optional layout concept. Finder items must be real interactive items. Add the instruction as separate typeset content: “Drag Margin to Applications.”

<a id="13-open"></a>

## 13-open — Open

**Composition:** 1:1. **Used for:** Open controls.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: An open folder with a clear upward-tilted front flap. The folder alone communicates opening a file; no arrow or letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="14-save"></a>

## 14-save — Save

**Composition:** 1:1. **Used for:** Save controls.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A simple recognizable floppy-disk silhouette in outline, with one broad upper inset and one broad lower panel. No letters or tiny mechanical details.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="15-save-as"></a>

## 15-save-as — Save as

**Composition:** 1:1. **Used for:** Save as controls.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A simple floppy-disk outline with a small pencil overlapping its lower-right corner, communicating saving under a new name. Both shapes must be readable, with no extra badge or letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="16-read"></a>

## 16-read — Read mode

**Composition:** 1:1. **Used for:** Optional Read mode icon beside its label.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: An open book with two clean page outlines and a central crease. No page text or letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="17-edit"></a>

## 17-edit — Edit mode

**Composition:** 1:1. **Used for:** Edit controls.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A single diagonal pencil with a clearly pointed nib. No page, handwriting, or letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="18-split"></a>

## 18-split — Split view

**Composition:** 1:1. **Used for:** Optional Split mode icon beside its label.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A rectangular workspace divided by one vertical line into two equally sized panes. No text, miniature controls, or arrows.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="19-outline"></a>

## 19-outline — Document outline

**Composition:** 1:1. **Used for:** Outline control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Three horizontal text-outline rows, each with a short leading marker; the lower two rows are progressively indented to suggest heading hierarchy. No actual letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="20-typography"></a>

## 20-typography — Typography

**Composition:** 1:1. **Used for:** Typography control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Exactly the characters Aa, a capital A followed by a lowercase a, in clear sturdy contemporary serif lettering. Align their baselines and use balanced spacing. These two characters are the entire icon.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="21-find"></a>

## 21-find — Find

**Composition:** 1:1. **Used for:** Find control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One clear magnifying glass with a round open lens and a short diagonal handle. No object inside the lens and no letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="22-focus"></a>

## 22-focus — Enter focus mode

**Composition:** 1:1. **Used for:** Focus control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Four inward-facing right-angle corner brackets arranged around an empty center, suggesting attention concentrated on the reading area. No arrows or central object.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="23-exit-focus"></a>

## 23-exit-focus — Exit focus mode

**Composition:** 1:1. **Used for:** Exit focus control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Four outward-facing right-angle corner brackets arranged to suggest expanding back into the surrounding workspace. Keep the same weight and proportions as the focus icon. No arrows or central object.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="24-appearance"></a>

## 24-appearance — Appearance

**Composition:** 1:1. **Used for:** Appearance control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One small artist's palette outline, with a simple thumb opening and three spacious circular paint wells. Keep all strokes black; no brush, color, sparkle, or letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="25-undo"></a>

## 25-undo — Undo

**Composition:** 1:1. **Used for:** Undo control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A single broad curved arrow turning left, with a clear arrowhead and open interior. No letters or enclosing circle.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="26-redo"></a>

## 26-redo — Redo

**Composition:** 1:1. **Used for:** Redo control.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A single broad curved arrow turning right, with a clear arrowhead and open interior. Mirror the structure of an undo icon. No letters or enclosing circle.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="27-bold"></a>

## 27-bold — Bold

**Composition:** 1:1. **Used for:** Bold formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Exactly one capital B in a sturdy bold serif letterform with clearly open counters. The B is the entire icon; no other letters or enclosing shape.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="28-italic"></a>

## 28-italic — Italic

**Composition:** 1:1. **Used for:** Italic formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Exactly one capital I in a clearly italic serif letterform, with short visible top and bottom serifs. The I is the entire icon; no other letters or enclosing shape.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="29-heading"></a>

## 29-heading — Heading

**Composition:** 1:1. **Used for:** Heading formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Exactly one capital H in a sturdy clear letterform with a strong horizontal crossbar. The H is the entire icon; no extra number, letters, or enclosing shape.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="30-quote"></a>

## 30-quote — Block quote

**Composition:** 1:1. **Used for:** Quote formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: A large, simple pair of opening double quotation marks, balanced and unmistakable. The punctuation is the entire icon; no page outline or additional content.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="31-list"></a>

## 31-list — Bulleted list

**Composition:** 1:1. **Used for:** List formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Three aligned round bullets beside three clean horizontal lines, with generous spacing and equal line weight. No letters or enclosing page.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="32-code"></a>

## 32-code — Code formatting

**Composition:** 1:1. **Used for:** Code formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Exactly the punctuation sequence </>, represented as a left angle bracket, a short diagonal slash, and a right angle bracket. Clear sturdy strokes and spacious separation; no letters or enclosing box.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="33-link"></a>

## 33-link — Insert link

**Composition:** 1:1. **Used for:** Link formatting.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Two simple interlocking oval chain links oriented diagonally. Show the overlap clearly with clean breaks in the outlines. No globe, arrow, or letters.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="34-close"></a>

## 34-close — Close

**Composition:** 1:1. **Used for:** Panel and dialog close controls.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: Two equal diagonal strokes crossing at the center to form a clear X. No surrounding circle or square.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="35-previous"></a>

## 35-previous — Previous

**Composition:** 1:1. **Used for:** Previous Find match.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One simple left-pointing chevron, two equal strokes meeting cleanly. No enclosing shape or second chevron.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="36-next"></a>

## 36-next — Next

**Composition:** 1:1. **Used for:** Next Find match.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One simple right-pointing chevron, two equal strokes meeting cleanly. No enclosing shape or second chevron.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="37-expand"></a>

## 37-expand — Expand

**Composition:** 1:1. **Used for:** Collapsed expandable items.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One simple right-pointing disclosure triangle or chevron, visually balanced and compact. No enclosing shape or extra marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="38-collapse"></a>

## 38-collapse — Collapse

**Composition:** 1:1. **Used for:** Expanded items.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One simple downward-pointing disclosure triangle or chevron, visually balanced and compact. Match the expand icon's proportions. No enclosing shape or extra marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="39-information"></a>

## 39-information — Information

**Composition:** 1:1. **Used for:** Informational dialogs.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One outlined circle containing exactly a lowercase i made from a clear dot and short stem. Keep the interior spacious. No other marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="40-warning"></a>

## 40-warning — Warning

**Composition:** 1:1. **Used for:** Warnings.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One outlined triangle with gently resolved corners containing exactly an exclamation mark. Keep the punctuation clear and the interior spacious. No other marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="41-error"></a>

## 41-error — Error

**Composition:** 1:1. **Used for:** Errors.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One outlined circle containing a clear X. Maintain a generous gap between the X and the circle. No other marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="42-success"></a>

## 42-success — Success

**Composition:** 1:1. **Used for:** Success messages.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One outlined circle containing a clear check mark. Maintain a generous gap between the check and the circle. No other marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="43-unsaved"></a>

## 43-unsaved — Unsaved changes

**Composition:** 1:1. **Used for:** Unsaved indicator beside explanatory text.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One small perfectly round solid dot centered in the canvas. Crisp silhouette, no outline ring, text, shadow, or other marks.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.

<a id="44-external-change"></a>

## 44-external-change — External file change

**Composition:** 1:1. **Used for:** External change notification.

**Positive prompt**

```text
Create exactly one isolated desktop interface icon for Margin, a calm editorial Markdown reader. Monochrome solid-black strokes on a plain white background. Flat vector-style drawing, straight-on view, consistent medium stroke weight, gently rounded line caps and joins, simple geometry, open shapes, no tiny details. Center the complete icon inside a square canvas with roughly 20 percent clear space on every side. Design it to remain clear when reduced to 20–24 pixels. No colored tile, border, shadow, gradient, texture, mockup, surrounding interface, extra symbols, captions, or labels.

Subject: One simple page outline with a small circular refresh arrow overlapping the lower-right corner, indicating that a file has changed outside the application. No page text, letters, download arrow, or extra badges.
```

**Optional negative prompt**

```text
photograph, photorealism, 3D render, perspective mockup, metallic material, glass, glow, heavy shadows, gradients, paper texture, distressed texture, watermark, signature, stock-logo presentation, contact sheet, multiple alternatives in one image, decorative border, unrelated objects, tiny details, blurry edges
```

Exploration/reference asset. Final UI icons should be coherent vectors or geometry bound to named SkinManager brushes, rather than individually shipped generated bitmaps.
