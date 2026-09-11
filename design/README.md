# Margin design and implementation

Revision 0.2 · 7 September 2026

Margin opens Markdown in rendered Read mode. Editing and saving are explicit actions. Reading preferences never change document bytes.

- [Canonical specification](canonical-design.md): accepted Paper shell, native reader/editor behavior, SkinManager resources, all 18 skins, installer requirements, and performance gates.
- [Brand decision](brand.md): Margin, its tagline, and compatibility with earlier MDPlayer installations.
- [Asset placement plan](asset-plan.md): confirmed toolbar branding and compact controls, the working surface inventory, and remaining asset decisions.
- [Comfy asset prompts](prompts/margin-asset-prompts.md): model-neutral prompts for exploring brand, platform, and interface assets; visual direction remains open.
- [Verification record](verification.md): measured evidence and outstanding qualification work.
- [Implementation record](implementation-status.md): implementation status and limitations.
- [MacBook handoff](macbook-handoff.md): exact prerequisites, build commands, expected artifacts, and native/emulated test distinctions.

The repository now contains an Avalonia application, file/document core, native renderer, six Margin skins, tests, benchmarks, and packaging scripts. Source presence and successful compilation do not imply every acceptance gate has passed.

The earlier interactive HTML design remains a preliminary design reference. Paper, Slate, and Studio now describe skin families on one canonical workspace; choosing a skin does not open panels or change reading typography.
