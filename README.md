# Gun Customisation

Gun Customisation is being developed into a reusable **Modular Weapon Customisation Framework** for Unity. The current room is a recovery-stage demonstration project; it is not yet an Asset Store-ready package.

Current engineering and release status is tracked in [PRODUCTISATION.md](PRODUCTISATION.md).
Legacy licensing research is tracked separately in [ASSET_PROVENANCE.md](ASSET_PROVENANCE.md).

## Project baseline

- Unity 2022.3 LTS
- Universal Render Pipeline (URP) 14
- Desktop mouse and keyboard demo
- Main demo scene: `Assets/Scenes/GunCustomisationScene.unity`

The repository was created with Unity 2022.3.35f1. The next verified baseline will use the installed 2022.3.62f2 patch after the local Unity Editor licence is activated.

## Demo controls

- `C`: switch between room cameras
- Left click a weapon: enter inspection mode
- Left-drag: rotate the inspected weapon
- `Escape`: leave inspection mode
- Attachment arrows: cycle the selected attachment category

Attachment swapping and stat modifiers are prototype scaffolding at this stage. They are not yet the final runtime loadout system.

## Product direction

The intended V1 product focuses on:

- Data-driven weapon and attachment definitions
- Typed attachment slots and compatibility rules
- Per-weapon runtime loadouts
- Install, remove, and replace operations
- Deterministic effective-stat calculation
- Inspection/preview presentation
- Example runtime UI and optional persistence
- Authoring validation, tests, samples, and documentation

The workshop room will remain sample content. Runtime, editor tooling, tests, documentation, and samples will be separated before distribution.

## Current verification limitation

Repository code and serialized assets have been statically audited. Live import, compilation, and Play Mode verification are pending because the local Unity Editor licence is currently unavailable. Do not treat the current branch as release-ready until those checks pass with no package-originated errors or warnings.

## Commercial content notice

Models, textures, HDRIs, and fonts currently used by the demo are under provenance review. They must not be included in a commercial distribution until redistribution rights are documented and compatible third-party notices are added.
