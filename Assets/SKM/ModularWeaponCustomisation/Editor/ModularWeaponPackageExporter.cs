using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Editor
{
    /// <summary>Exports only the commercial product root.</summary>
    public static class ModularWeaponPackageExporter
    {
        public const string ProductRoot =
            "Assets/SKM/ModularWeaponCustomisation";
        private const string DemoScenePath =
            ProductRoot +
            "/Samples/GeneratedDemo/Scenes/Modular Weapon Customisation Demo.unity";

        [MenuItem(
            "Tools/SKM/Modular Weapon Customisation/Export Product Package...")]
        public static void Export()
        {
            if (!AssetDatabase.IsValidFolder(ProductRoot))
            {
                EditorUtility.DisplayDialog(
                    "Product root missing",
                    $"Could not find '{ProductRoot}'.",
                    "Close");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DemoScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Demo scene missing",
                    "Generate and verify the redistributable primitive demo before " +
                    "exporting the commercial package.",
                    "Close");
                return;
            }

            bool passed = ModularWeaponValidationWindow.ValidateProject(
                out int errors,
                out int warnings);
            if (!passed)
            {
                EditorUtility.DisplayDialog(
                    "Validation failed",
                    $"Resolve {errors} validation error(s) before exporting. " +
                    "The validator will open with the current results.",
                    "Open Validator");
                ModularWeaponValidationWindow.OpenAndScan();
                return;
            }

            bool shouldContinue = EditorUtility.DisplayDialog(
                "Export product package",
                $"Validation passed with {warnings} warning(s). This " +
                "operation includes only the SKM product root; the legacy workshop " +
                "and its unverified art are excluded.",
                "Choose Destination",
                "Cancel");
            if (!shouldContinue)
            {
                return;
            }

            string destination = EditorUtility.SaveFilePanel(
                "Export Modular Weapon Customisation Framework",
                string.Empty,
                "SKM-Modular-Weapon-Customisation.unitypackage",
                "unitypackage");
            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }

            try
            {
                AssetDatabase.ExportPackage(
                    ProductRoot,
                    destination,
                    ExportPackageOptions.Recurse);

                long bytes = File.Exists(destination)
                    ? new FileInfo(destination).Length
                    : 0L;
                Debug.Log(
                    $"Exported Modular Weapon Customisation package to " +
                    $"'{destination}' ({bytes:N0} bytes).");
                EditorUtility.RevealInFinder(destination);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Export failed",
                    exception.Message,
                    "Close");
            }
        }
    }
}
