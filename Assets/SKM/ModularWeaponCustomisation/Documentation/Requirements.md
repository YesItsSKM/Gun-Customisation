# Requirements and Dependencies

## Supported baseline

- Unity 2022.3 LTS or newer.
- C# project compatibility level supported by Unity 2022.3.
- Built-in Render Pipeline, URP, and HDRP are supported by the framework code because it does not reference a render pipeline.

The first release must be imported and tested in its declared minimum Unity version before publishing.

## Package dependencies

- The runtime, preview, persistence, and editor assemblies use Unity core/editor APIs only.
- The Demo UI sample uses Unity uGUI (`com.unity.ugui`).
- The Edit Mode test assembly uses Unity Test Framework (`com.unity.test-framework`).

TextMesh Pro, Cinemachine, the Input System package, Addressables, and a render-pipeline package are not required by the framework.

## Input

`WeaponPreviewController` does not poll input. Call its public orbit, zoom, begin, and end methods from the old Input Manager, the Input System package, touch controls, or your own interaction layer.

## Serialization

The optional persistence example uses `JsonUtility` and stable authored IDs. It does not write files, PlayerPrefs, or cloud data. Your project chooses the storage backend.
