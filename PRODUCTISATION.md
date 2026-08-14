# Productisation Status

## Product boundary

The commercial package is exactly:

`Assets/SKM/ModularWeaponCustomisation`

The legacy workshop, gun models, attachment models, textures, HDRIs, and fonts outside that root are not approved for redistribution. The included exporter is intentionally scoped to the product root.

## Implemented

- Data-driven weapon, attachment, slot, and stat definitions.
- Compatibility allow/deny rules and explicit failure values.
- Independent runtime loadouts with install, replacement, and removal.
- Socket-based visual lifecycle and deterministic stat snapshots.
- Safe current-weapon spawning/replacement.
- Input-independent preview/orbit/zoom with transform restoration.
- Optional stable-ID JSON persistence and catalog.
- Setup wizard, inspector drawers, validator, and scoped package exporter.
- Event-driven uGUI reference panel.
- Edit Mode test source for core, runtime visuals, spawning, preview, and persistence.
- Buyer-facing getting-started, architecture, API, requirements, troubleshooting, changelog, and third-party notice documents.
- Conservative Asset Store listing draft, pricing hypothesis, media plan, support
  boundaries, and version 1.0 release gate.

## Verification completed

- Every product C# assembly compiles against Unity 2022.3.62f2 managed assemblies.
- Static build result: zero compiler warnings and zero compiler errors.
- All product C# files use the `SKM.ModularWeaponCustomisation` namespace family.
- All assembly-definition JSON parses.
- No missing product `.meta` files or duplicate product GUIDs.
- No product-root reference to legacy scripts or content paths.
- Serialized scene/prefab/material/asset GUID audit resolves every non-built-in
  reference against project or package metadata.
- A project-local migration command can generate new definitions and prefab variants
  without changing legacy source assets; its output stays outside the export root.
- A product-local builder is ready to generate a clean primitive sample after Unity
  licensing is restored.
- The package exporter refuses release until that clean demo scene exists and the
  framework validator reports no errors.

## Verification blocked by local state

Unity batch import and Edit Mode test execution stop before import because the Unity Licensing Client reports:

- Machine bindings do not match.
- Token is not present in cache.
- No valid Unity Editor license was found.

Activate/refresh the Unity 2022.3 license in Unity Hub, then rerun the isolated
validation project at `Temp/UnityPackageValidationCurrent`.

## Release blockers

- Execute the Edit Mode suite inside Unity and record all tests passing.
- Import in a clean Unity 2022.3 LTS project with no package-originated console warnings/errors.
- Create and visually verify a redistributable sample scene/prefabs.
- Either document redistribution rights for every legacy demo asset or replace/exclude it.
- Remove the now-unreferenced Microsoft Consolas folder from any redistributable
  demo export (all scene/UI references already use Afacad Flux).
- Verify the final package in every advertised render pipeline/version.
- Generate final screenshots/video, provide publisher metadata and support contact,
  choose the final version/price, and promote `STORE_LISTING_DRAFT.md` only after its
  marked claims are verified.
- Perform a final Asset Store guideline/licensing review immediately before submission.
