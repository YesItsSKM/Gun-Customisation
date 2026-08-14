# Troubleshooting

## An attachment is rejected

Read the returned `AttachmentInstallFailure`:

- `UnsupportedSlot`: add the attachment's slot to the weapon definition.
- `IncompatibleWeapon`: review the attachment's compatible/incompatible lists.
- `MissingSocket`: add an `AttachmentSocket` for that slot to the instantiated weapon prefab.
- `RuntimeNotInitialized`: assign a definition and call `Initialize`, or enable initialization on Awake.

Run the validator to catch the corresponding authoring problem before Play Mode.

Non-finite or overflowed stat calculations throw an `ArgumentException` because they indicate invalid authoring data rather than a recoverable selection failure.

## The attachment is offset or rotated incorrectly

Position the weapon prefab's socket at the mount point. Author the attachment prefab so its intended mount pivot is local position/rotation zero with unit local scale. A socket's optional Visual Root is the actual parent used for the visual instance.

## Stats do not return to their base values

Do not mutate definition assets or apply inverse arithmetic. Use `WeaponRuntime.TryInstall` and `TryRemove`; the runtime recalculates from the authored base stats and every currently installed attachment.

## Old save JSON no longer resolves

Stable IDs are persistence contracts. Restore the released ID or migrate the save data in your storage layer. Do not rename IDs after shipping content without a migration plan.

## Preview does not start

Assign a preview anchor and make sure the controller is idle. `TryBeginPreview` intentionally rejects a second weapon until the current session ends.

## A model prefab cannot be configured by the wizard

Imported model prefabs are read-only. Create a regular prefab or prefab variant, then run the setup wizard with that asset.

## A sample UI button does nothing

Check that the panel is bound to an initialized runtime and catalog, the button template is inactive in the scene, and the catalog contains the attachment. The validator reports ambiguous or empty catalog entries.

## Compilation fails in a project without sample/test packages

The framework core does not require uGUI or Unity Test Framework, but the included Demo UI and Edit Mode test assemblies do. Install `com.unity.ugui` and `com.unity.test-framework`, or omit those optional folders when exporting a core-only integration.
