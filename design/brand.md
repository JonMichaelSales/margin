# Margin

**A little room to read.**

Selected 7 September 2026. Margin replaces the preliminary MDPlayer product name.

The identity is calm and editorial: generous whitespace, readable typography, a simple page symbol, and a restrained accent line. The canonical Paper workspace and Paper/Slate/Studio palette families remain the visual foundation. Use the tagline exactly as written, including its final period.

The name appears in window titles, the opening screen, appearance dialog, application skin names, executable metadata, installers, shortcuts, and Mac bundle metadata. The Windows executable is `Margin.exe`; the Mac executable is `Margin` inside `Margin.app`. Release artifacts use `Margin-<version>-<runtime>`.

The reading toolbar uses the symbol only beside the document title. The wordmark and tagline belong on the welcome surface and other allocated brand surfaces. Secondary toolbar actions may collapse to icons with tooltips at limited widths. See the [asset placement plan](asset-plan.md) for confirmed decisions, the working allocation, and visual/generation choices still open.

## Compatibility with existing personal test installs

The Windows installer retains its original AppId and install directory, the existing Markdown ProgID, and the running-process mutex. Existing file choices are redirected to the renamed executable. The installer removes only its specifically named legacy executable files and shortcuts during upgrade. Settings and Markdown documents are retained.

Preferences continue using the existing private `MDPlayer` settings directory. The six old `MDPlayer Paper/Slate/Studio Light/Dark` selections are normalized to the corresponding `Margin` names on load without writing the settings file. Project folders, namespaces, and the solution filename remain technical identifiers under `MDPlayer`; they are not the displayed application name.

Mac packaging now uses `Margin.app` and bundle identifier `com.jonsales.margin`. Mac packages have not yet been distributed or tested; their build and qualification remain scheduled for the later MacBook session.
