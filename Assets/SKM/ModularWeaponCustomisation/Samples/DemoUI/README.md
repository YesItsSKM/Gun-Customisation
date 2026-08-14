# Demo UI Sample

`WeaponCustomisationPanel` is a small uGUI reference implementation. It lists compatible attachments from a `WeaponCatalog`, installs or removes the selected option, and refreshes effective stats through runtime events.

## Scene setup

1. Create a Canvas with weapon and slot Dropdowns, attachment-list content
   transform, inactive Button template, remove Button, and Text labels.
2. Add `WeaponCustomisationPanel` to the panel root.
3. Assign either an initialized `WeaponRuntime`, or a `WeaponSpawner` for
   automatic rebinding, plus a `WeaponCatalog`.
4. Wire the controls and labels in the inspector.
5. Put a Vertical Layout Group on the attachment-list content if you want automatic button layout.

This sample uses Unity uGUI to keep it available in Unity 2022.3 without TextMesh Pro coupling. The runtime core itself has no UI dependency.

`LegacyMousePreviewInput` is an optional adapter for the generated desktop demo:
click a weapon to preview, drag to orbit, scroll to zoom, and press Escape to
return. It polls Unity's old Input Manager only; Input System projects should call
the preview controller API from their own actions.
