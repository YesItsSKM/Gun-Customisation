# Asset Store Listing Draft

This is working commercial copy, not a submission-ready claim sheet. Text marked
**verify before publishing** must not appear on the live listing until the licensed
Unity validation pass is complete.

## Product identity

**Working title:** Modular Weapon Customisation Framework

**Subtitle:** A data-driven framework for weapon attachments, loadouts, stats,
preview, persistence, authoring, and validation.

**Category candidate:** Tools / Game Toolkits

**Version:** 1.0.0 (after the release checklist passes)

## Short description

Build weapon customisation flows without coupling your project to a complete FPS
controller. Define weapons, attachment slots, compatibility, and stat effects with
ScriptableObjects; then install, replace, remove, preview, save, and restore each
weapon's independent runtime loadout.

## Full description

Modular Weapon Customisation Framework is a focused foundation for projects that
need configurable weapons but already have—or plan to build—their own shooting,
inventory, animation, and input systems.

Author reusable weapon, attachment, slot, and stat definitions in the Inspector.
At runtime, each spawned weapon owns an independent loadout. Attachments are
validated against weapon and slot compatibility rules, instantiated at named
sockets, and reflected in a deterministic effective-stat snapshot.

The package includes editor setup and validation tools, stable-ID JSON loadout
persistence, an input-independent preview controller, an event-driven uGUI
reference panel, documented APIs, and Edit Mode coverage for the critical rules.

### Highlights

- ScriptableObject definitions for weapons, attachments, slots, and stats.
- Explicit allow/deny compatibility with inspectable failure results.
- Install, replace, and remove attachment operations.
- Per-instance loadouts; shared definitions are never mutated at runtime.
- Additive, multiplicative, and priority-based override stat modifiers.
- Socket-driven attachment visual lifecycle.
- Safe current-weapon spawning and replacement.
- Stable-ID JSON save/load with catalog validation.
- Input-independent preview, orbit, pitch, and zoom controller.
- Event-driven uGUI reference interface.
- Setup wizard, compact property drawers, and project validator.
- Source code, API documentation, troubleshooting guide, and changelog.
- A redistributable primitive demo generator with two example weapons and four
  attachments. **Verify before publishing.**

### Designed to integrate, not take over

The framework deliberately does not implement firing, damage, ammunition,
inventory, character control, animation, recoil, networking, audio, or VFX. These
systems vary greatly between games; the documented events and APIs are intended to
connect the customisation layer to the project's chosen implementations.

### Included content

- Runtime core, persistence, preview, and spawning assemblies.
- Editor authoring, validation, demo-generation, and package-export tools.
- uGUI sample implementation.
- Primitive sample data and scene generated inside Unity. **Verify before
  publishing.**
- Edit Mode tests and buyer-facing documentation.

No production gun models, animations, audio, or VFX are included. The sample uses
Unity primitives so buyers can replace the visuals with their own licensed assets.

## Technical information

- Baseline Unity version: Unity 2022.3 LTS.
- Language: C#.
- UI sample dependency: Unity uGUI.
- Render-pipeline coupling: core runtime code has none; generated primitive sample
  materials select an available Built-in, URP, or HDRP-compatible shader.
- Input: core preview is input-system agnostic. The generated demo includes an
  optional legacy mouse-input adapter.
- Persistence: versioned JSON records and project-authored stable-ID catalogs; no
  PlayerPrefs dependency.
- Assembly definitions and namespaces are included to reduce integration conflicts.
- **Verify before publishing:** clean import, Unity-run Edit Mode tests, demo
  behaviour, platform claims, and every advertised Unity/render-pipeline version.

## Buyer fit

Good fit for:

- Teams adding attachment customisation to an existing FPS, TPS, RPG, survival,
  loadout, or inventory stack.
- Developers who want data-driven definitions without adopting an entire combat
  controller.
- Projects that need independent weapon instances and serializable attachment
  choices.

Not intended as:

- A complete weapon firing or combat system.
- A gun-model or animation pack.
- A multiplayer replication solution.
- A no-code replacement for project-specific inventory and gameplay integration.

## Search keywords

weapon customisation, weapon customization, gun attachment, modular weapon,
loadout, attachments, ScriptableObject, weapon stats, weapon preview, save load,
JSON persistence, runtime equipment, FPS, TPS, RPG, inventory integration, editor
tools, validation

## Pricing hypothesis

**Recommended introductory list price:** USD 19.99.

**Target after release proof and broader compatibility testing:** USD 24.99.

This is intentionally below the current USD 29.99
[Modular Weapon System](https://marketplace.unity.com/packages/templates/systems/modular-weapon-system-319941),
which advertises a broader combat feature set plus five weapons, shooting modes,
audio, animation, VFX, and a polished demo. Two looser market anchors are the old
USD 4.99
[Modular Weapon Tool](https://assetstore.unity.com/packages/3d/props/guns/modular-weapon-tool-159229)
and the art-led USD 40
[BoZo: Stylized Modular Weapons](https://assetstore.unity.com/packages/3d/props/weapons/bozo-stylized-modular-weapons-295224).
They are not direct substitutes, but they support positioning this as a focused,
professionally documented code framework rather than a full FPS system or an art
pack. Recheck prices immediately before submission.

Do not discount at launch merely to compensate for missing release proof. Finish
the demo and verification first; use a temporary introductory discount only if it
fits the final publisher strategy.

## Store media plan

Required before submission:

1. Hero image showing two primitive weapon configurations and the product name.
2. Diagram: definitions -> runtime loadout -> socket visuals -> effective stats.
3. Inspector image showing a weapon definition and compatibility configuration.
4. Runtime image showing attachment selection and changing stat values.
5. Editor validator image with a clean result.
6. Persistence/API image showing stable IDs and the small integration surface.
7. Short video: author an attachment, install/replace/remove it, switch weapons,
   save a loadout, reload it, and show validation catching an error.

Use only the product's primitive demo in store media unless exact redistribution
and marketing rights are documented for any additional artwork.

## Support and maintenance copy

Publish only after supplying real contact details:

- Support email or support URL: **TBD by publisher**.
- Documentation location: included in package; optional public mirror **TBD**.
- Bug reports should include Unity version, render pipeline, reproduction steps,
  and the validator result.
- Support covers reproducible package defects and documented integration points.
- Custom game-specific combat, inventory, networking, and animation implementation
  is outside standard support.
- Do not promise a response-time SLA until there is a sustainable support process.

## Version 1.0 release gate

All items are mandatory:

- [ ] Unity license is active on the validation machine.
- [ ] Package imports into a clean Unity 2022.3 LTS project without
  package-originated errors or warnings.
- [ ] All 15 Edit Mode tests pass inside Unity and the result is retained.
- [ ] Primitive demo generation completes and the scene is visually reviewed.
- [ ] Install, replace, remove, weapon switch, preview, stat display, save, and load
  are manually exercised in the generated demo.
- [ ] Validator reports zero errors on the release project.
- [ ] Every advertised render pipeline and Unity version is tested; unsupported
  combinations are stated plainly.
- [ ] Exported `.unitypackage` is re-imported into a second empty project and
  retested.
- [ ] Package contents contain only `Assets/SKM/ModularWeaponCustomisation`.
- [ ] No legacy guns, workshop assets, Consolas files, or other unapproved content
  appears in the package or store media.
- [ ] Third-party notices and all license obligations are reviewed.
- [ ] Final title, publisher identity, support contact, screenshots, video, price,
  keywords, and compatibility fields are supplied.
- [ ] Current Unity Asset Store submission guidelines are reviewed immediately
  before upload.

## Claims policy

Until the release gate passes, use phrases such as "designed for" and "baseline"
instead of "fully compatible," "production ready," "all platforms," or "zero
setup." Never advertise the unverified legacy workshop art as included content.
