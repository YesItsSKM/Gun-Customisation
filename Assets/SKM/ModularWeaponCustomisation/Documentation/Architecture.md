# Architecture

## Ownership

- `WeaponDefinition`, `WeaponAttachmentDefinition`, `AttachmentSlotDefinition`, and `WeaponStatDefinition` are immutable authored data shared across instances.
- `WeaponRuntime` owns one scene instance.
- `WeaponSpawner` owns the optional current-instance replacement lifecycle.
- `WeaponLoadout` owns that instance's mutable attachment selections.
- `WeaponStatsSnapshot` is a newly calculated immutable view of effective values.
- `AttachmentSocket` owns only presentation placement; it does not own loadout state.

ScriptableObject assets are never changed when an attachment is installed or removed.

## Operation flow

1. A caller requests `WeaponRuntime.TryInstall`.
2. `WeaponCompatibility` checks the weapon, attachment, slot, and allow/deny lists.
3. `WeaponRuntime` confirms the prefab has the required visual socket.
4. `WeaponLoadout` replaces the attachment stored for that slot.
5. The runtime replaces the tracked visual instance at the socket.
6. `WeaponStatsCalculator` creates a new effective-stat snapshot.
7. `LoadoutChanged` and `StatsChanged` notify presentation code.

Removing follows the same path and restores stats by recalculating from the weapon's base values and remaining attachments. No inverse modifier arithmetic is used.

`WeaponSpawner` validates and initializes a replacement before publishing the change event and releasing the previous instance. A rejected definition therefore leaves the current weapon untouched.

## Compatibility

An attachment is installable only when:

- The weapon, attachment, and slot definitions exist.
- The weapon lists the attachment's slot as supported.
- The attachment does not explicitly deny the weapon.
- An explicit compatible list is empty or contains the weapon.
- The instantiated weapon prefab has one socket for the slot.

Failures are returned as `AttachmentInstallFailure`; expected validation failures do not use exceptions.

## Extensibility

Stats and slots are ScriptableObject identities rather than hard-coded enums. Projects can add new statistics and attachment categories without modifying framework source.

Runtime UI, input, persistence, inspection presentation, and weapon gameplay consume the core through public methods/events. They remain separate concerns so projects can replace the sample implementations.

`WeaponPreviewController` moves one `WeaponRuntime` to an authored preview anchor, exposes input-agnostic orbit/zoom methods, and restores its original parent, sibling position, and local transform when preview ends. It does not poll a particular input system or require a render pipeline.

Persistence lookup is deliberately fail-closed: duplicate stable IDs are reported by the validator and are not resolved by `WeaponCatalog`.

## Assembly boundaries

- `SKM.ModularWeaponCustomisation.Runtime`: reusable runtime API.
- `SKM.ModularWeaponCustomisation.Preview`: optional, input-independent inspection presentation.
- `SKM.ModularWeaponCustomisation.Persistence`: optional stable-ID JSON reference implementation.
- `SKM.ModularWeaponCustomisation.Editor`: authoring validation; excluded from builds.
- `SKM.ModularWeaponCustomisation.Tests.EditMode`: critical logic tests; excluded from builds.
