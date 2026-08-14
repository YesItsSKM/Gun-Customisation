# Changelog

All notable framework changes will be documented here.

## [Unreleased]

### Added

- Reusable runtime assembly under the `SKM.ModularWeaponCustomisation` namespace.
- Data-driven weapon, attachment, slot, and stat definitions.
- Per-weapon runtime loadouts with compatibility failures.
- Socket-based visual installation, replacement, and removal.
- Deterministic additive, multiplicative, and override stat calculation.
- Immutable effective-stat snapshots and runtime change events.
- Editor validation window for definitions and prefabs.
- Edit Mode coverage for stat rules, compatibility, replacement, instance isolation,
  runtime visual lifecycle, and stat restoration.
- Optional stable-ID JSON loadout persistence with a project-authored catalog.
- Catalog validation and Edit Mode coverage for persistence resolution failures.
- Input-independent preview enter/exit transitions, constrained orbit, and zoom.
- Setup wizard for connected definition creation and guarded prefab/socket configuration.
- Event-driven uGUI reference panel for slot selection, install/remove, and stat display.
- Current-weapon spawner with failure reporting and safe replacement semantics.
- Compact inspector drawers for stat values and attachment modifiers.
- Non-destructive builder for a redistributable primitive demo with two weapons,
  four attachments, UI, preview controls, and a sample scene.

### Recovery

- Hardened legacy inspection transitions, null handling, camera switching, AudioListener ownership, and URP depth of field.
- Changed Build Settings to use the gun customisation demo scene.
- Removed a missing emission texture reference from the demo environment.
