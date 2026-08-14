# Getting Started

## 1. Define reusable stat and slot assets

Create assets from `Assets > Create > SKM > Modular Weapon Customisation`:

- `Weapon Stat` for each project-specific statistic.
- `Attachment Slot` for each attachment category.

Stable IDs are used for validation and future persistence. Treat an ID as immutable after releasing content that references it.

For a guided weapon or attachment setup, open `Tools > SKM > Modular Weapon Customisation > Setup Wizard`. Generated socket placeholders must still be positioned at the correct mount points on the prefab.

## 2. Create attachment definitions

Create a `Weapon Attachment` asset and assign:

- A unique stable ID and display name.
- The slot it occupies.
- An optional visual prefab.
- Stat modifiers.
- Optional compatible/incompatible weapon lists.

An empty compatible-weapon list means every weapon supporting the slot is accepted. The incompatible list always wins.

## 3. Create a weapon definition

Create a `Weapon` asset and assign:

- A unique stable ID and display name.
- A weapon prefab.
- Base stat values.
- Supported slots.
- Optional starting attachments.

Each base stat and supported slot should appear only once.

## 4. Configure the weapon prefab

1. Add `WeaponRuntime` to the prefab root and assign the matching weapon definition.
2. Add one `AttachmentSocket` for every supported slot.
3. Position and rotate each socket where its visual attachment should be placed.
4. Leave the socket's Visual Root empty to use the socket transform itself, or assign a child transform.

Attachment prefabs should use a zero local position/rotation and unit scale at their intended mount pivot.

## 5. Change a loadout at runtime

```csharp
using SKM.ModularWeaponCustomisation;

public sealed class AttachmentButtonExample : MonoBehaviour
{
    [SerializeField] private WeaponRuntime weapon;
    [SerializeField] private WeaponAttachmentDefinition attachment;

    public void Install()
    {
        if (!weapon.TryInstall(attachment, out AttachmentInstallFailure failure))
        {
            Debug.LogError($"Attachment was rejected: {failure}", weapon);
        }
    }
}
```

Use `TryRemove` with a slot definition to remove the current attachment. Subscribe to `LoadoutChanged` and `StatsChanged` instead of polling in `Update`.

## 6. Spawn selectable weapons (optional)

Add `WeaponSpawner` to a scene coordinator and call `TrySpawn` with a weapon definition. The replacement is instantiated and initialized before the old current weapon is destroyed. Listen to `CurrentWeaponChanged` to rebind UI or preview code.

## 7. Add inspection/preview (optional)

1. Add `WeaponPreviewController` to a scene coordinator object.
2. Assign a preview anchor positioned where the weapon should be displayed.
3. Optionally assign the viewer/camera as Zoom Reference so normalized zoom moves
   toward the viewer independently of the weapon's display rotation.
4. Call `TryBeginPreview(weapon)` and `TryEndPreview()` from your interaction layer.
5. Feed pointer, stick, or touch deltas to `Rotate`, and a normalized value to `SetZoom`.

The controller deliberately does not choose an input package. It restores the weapon's captured parent, sibling index, and local pose when preview ends or the controller is disabled.

## 8. Save and restore loadouts (optional)

The persistence assembly is optional and stores stable IDs rather than Unity object references:

1. Create a `Weapon Catalog` asset and add every weapon and attachment that may be restored.
2. Call `WeaponLoadoutJson.Serialize(weapon.Loadout)` to create portable JSON.
3. Call `WeaponLoadoutJson.TryApply(json, weapon, catalog, out error)` to validate and restore it.

The API intentionally does not choose a storage backend. Save the returned JSON with your own profile, cloud-save, file, or platform service. Keep released IDs stable so old saves continue to resolve.

## 9. Validate before Play Mode

Run `Tools > SKM > Modular Weapon Customisation > Validator`. Resolve every error before packaging. Warnings should either be fixed or intentionally documented.

## Modifier rules

For each stat, calculation is deterministic:

1. Sum all `Add` modifiers with the base value.
2. Multiply the result by every `Multiply` value.
3. If overrides exist, the highest-priority `Override` replaces the result. A later modifier wins a priority tie.
4. Clamp the final value to the stat definition's range.

For example, a multiply value of `0.9` reduces a result by 10%; `1.1` increases it by 10%.
