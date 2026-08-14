# Modular Weapon Customisation Framework

This folder contains the reusable product code. It is intentionally separated from the legacy workshop scene and its unverified third-party art.

## Included foundation

- Data-driven weapon, attachment, slot, and stat definitions
- Explicit weapon/attachment compatibility checks
- Independent per-instance runtime loadouts
- Validated current-weapon spawning and replacement
- Install, replace, and remove operations
- Socket-based attachment visuals
- Deterministic stat calculation without ScriptableObject mutation
- Runtime loadout/stat events
- Input-independent weapon preview transitions, orbit, and zoom
- Optional stable-ID JSON persistence
- Event-driven uGUI customisation panel sample
- Project validation window
- Edit Mode tests for critical pure logic

Start with [Getting Started](Documentation/Getting Started.md), then read
[Architecture](Documentation/Architecture.md), [API Reference](Documentation/API%20Reference.md),
[Requirements](Documentation/Requirements.md), and
[Troubleshooting](Documentation/Troubleshooting.md).

The sample UI is documented in [Demo UI](Samples/DemoUI/README.md).

## Editor menu

Open `Tools > SKM > Modular Weapon Customisation > Setup Wizard` to create connected weapon/attachment definitions and configure weapon prefab sockets.

Open `Tools > SKM > Modular Weapon Customisation > Validator` to scan all framework definitions and prefabs for authoring errors.

The publisher-only `Export Product Package...` command exports this product root and excludes the legacy workshop content.

`Create Redistributable Primitive Demo` generates a self-contained sample scene,
two weapons, four attachments, materials, definitions, and UI without referencing
the legacy art.

## Status

The reusable core is under active productisation. The current workshop scene is not yet a redistributable sample and is not part of this product root.
