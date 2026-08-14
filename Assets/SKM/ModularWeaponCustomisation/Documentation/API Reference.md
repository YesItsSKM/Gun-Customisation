# API Reference

## Definitions

### `WeaponDefinition`

Authored weapon identity, visual prefab, base stats, supported slots, and starting attachments. Use the stable `Id` for persistence. `SupportsSlot` checks slot support by asset identity.

### `WeaponAttachmentDefinition`

Authored attachment identity, visual prefab, occupied slot, stat modifiers, and weapon allow/deny lists. An empty compatible list allows every weapon supporting the slot; the incompatible list always wins.

### `AttachmentSlotDefinition`

Extensible attachment category identity such as optic or muzzle. The framework does not hard-code categories in an enum.

### `WeaponStatDefinition`

Extensible stat identity with display metadata, range, and direction. `Clamp` and `Normalize` use the authored range.

## Per-instance runtime

### `WeaponRuntime`

Owns one weapon instance's loadout, visual attachment instances, and effective stats.

- `Initialize`: initializes or reinitializes an instance.
- `TryInstall`: validates, installs/replaces, refreshes the visual, and recalculates stats.
- `TryRemove`: clears a supported slot and recalculates.
- `TryGetSocket`: resolves the authored socket for a slot.
- `LoadoutChanged`: raised after a successful selection change.
- `StatsChanged`: raised after a new immutable effective-stat snapshot is available.

### `WeaponLoadout`

Mutable attachment selection state owned by one runtime instance. It can also be used independently in tests or headless game logic.

- `TryInstall`
- `TryRemove`
- `TryGetAttachment`
- `EnumerateAttachmentsInSlotOrder`
- `Changed`

### `WeaponStatsCalculator`

Pure deterministic calculator. It never writes to definitions. The result is a `WeaponStatsSnapshot`.

### `WeaponCompatibility`

Central slot and allow/deny evaluation used by runtime code, persistence, UI, and editor validation.

### `WeaponSpawner`

Optional one-current-weapon lifecycle coordinator.

- `TrySpawn`: initializes a replacement before releasing the old current instance.
- `Clear`: releases the current instance.
- `CurrentWeaponChanged`: rebind UI/preview consumers here.

## Preview

### `WeaponPreviewController`

Moves a weapon to a preview anchor without selecting an input API.

- `TryBeginPreview`
- `TryEndPreview`
- `Rotate`
- `SetOrbit`
- `SetZoom`
- `ResetView`
- `RestoreImmediately`
- `PreviewStarted`, `PreviewEnded`, and `StateChanged`

The controller restores the captured parent, sibling index, local position, rotation, and scale.
An optional zoom reference (normally the viewing camera) makes zoom direction
independent of the weapon anchor's display rotation.

## Persistence

### `WeaponCatalog`

Maps stable weapon and attachment IDs to authored assets. Run the project validator after editing a catalog.

### `WeaponLoadoutJson`

- `Serialize`: converts a loadout to schema-versioned stable-ID JSON.
- `TryResolve`: validates and resolves JSON without changing runtime state.
- `TryApply`: validates the complete selection and restores it to a matching initialized runtime.

The framework does not own storage or silently upgrade unknown schema versions.

## Failure values

`AttachmentInstallFailure` and `WeaponSpawnFailure` make expected validation failures explicit. Use them for user feedback and reserve exceptions for programmer/configuration defects such as non-finite stat values.
