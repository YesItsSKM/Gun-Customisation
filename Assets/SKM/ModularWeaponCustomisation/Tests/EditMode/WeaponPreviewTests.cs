using System.Collections.Generic;
using NUnit.Framework;
using SKM.ModularWeaponCustomisation.Preview;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Tests
{
    public sealed class WeaponPreviewTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void ImmediatePreview_RestoresParentSiblingAndLocalPose()
        {
            GameObject parent = CreateGameObject("Weapon Parent");
            parent.transform.SetPositionAndRotation(
                new Vector3(4f, 1f, -2f),
                Quaternion.Euler(0f, 35f, 0f));

            GameObject olderSibling = CreateGameObject("Older Sibling");
            olderSibling.transform.SetParent(parent.transform, false);
            GameObject weaponObject = CreateGameObject("Weapon");
            weaponObject.transform.SetParent(parent.transform, false);
            weaponObject.transform.localPosition = new Vector3(0.5f, 0.25f, -0.75f);
            weaponObject.transform.localRotation = Quaternion.Euler(5f, 10f, 15f);
            weaponObject.transform.localScale = new Vector3(1.1f, 0.9f, 1.2f);
            int siblingIndex = weaponObject.transform.GetSiblingIndex();
            Vector3 localPosition = weaponObject.transform.localPosition;
            Quaternion localRotation = weaponObject.transform.localRotation;
            Vector3 localScale = weaponObject.transform.localScale;
            weaponObject.SetActive(false);
            WeaponRuntime weapon = weaponObject.AddComponent<WeaponRuntime>();

            GameObject anchorObject = CreateGameObject("Preview Anchor");
            anchorObject.transform.SetPositionAndRotation(
                new Vector3(0f, 2f, 3f),
                Quaternion.Euler(0f, 180f, 0f));
            GameObject controllerObject = CreateGameObject("Preview Controller");
            WeaponPreviewController controller =
                controllerObject.AddComponent<WeaponPreviewController>();
            Configure(controller, anchorObject.transform, transitionDuration: 0f);

            Assert.That(controller.TryBeginPreview(weapon), Is.True);
            Assert.That(controller.State, Is.EqualTo(WeaponPreviewState.Active));
            Assert.That(weaponObject.transform.parent, Is.Null);
            Assert.That(
                Vector3.Distance(
                    weaponObject.transform.position,
                    anchorObject.transform.position),
                Is.LessThan(0.0001f));

            Assert.That(controller.TryEndPreview(), Is.True);

            Assert.That(controller.State, Is.EqualTo(WeaponPreviewState.Idle));
            Assert.That(weaponObject.transform.parent, Is.SameAs(parent.transform));
            Assert.That(weaponObject.transform.GetSiblingIndex(), Is.EqualTo(siblingIndex));
            Assert.That(
                Vector3.Distance(weaponObject.transform.localPosition, localPosition),
                Is.LessThan(0.0001f));
            Assert.That(
                Quaternion.Angle(weaponObject.transform.localRotation, localRotation),
                Is.LessThan(0.0001f));
            Assert.That(
                Vector3.Distance(weaponObject.transform.localScale, localScale),
                Is.LessThan(0.0001f));
        }

        [Test]
        public void Preview_RejectsASecondWeaponUntilTheSessionEnds()
        {
            WeaponRuntime first = CreateInactiveRuntime("First");
            WeaponRuntime second = CreateInactiveRuntime("Second");
            Transform anchor = CreateGameObject("Anchor").transform;
            WeaponPreviewController controller =
                CreateGameObject("Controller").AddComponent<WeaponPreviewController>();
            Configure(controller, anchor, transitionDuration: 0f);

            Assert.That(controller.TryBeginPreview(first), Is.True);
            Assert.That(controller.TryBeginPreview(second), Is.False);
            Assert.That(controller.CurrentWeapon, Is.SameAs(first));

            Assert.That(controller.RestoreImmediately(), Is.True);
            Assert.That(controller.TryBeginPreview(second), Is.True);
        }

        private static void Configure(
            WeaponPreviewController controller,
            Transform anchor,
            float transitionDuration)
        {
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("previewAnchor").objectReferenceValue = anchor;
            serialized.FindProperty("zoomDistanceLimits").vector2Value = Vector2.zero;
            serialized.FindProperty("transitionDuration").floatValue = transitionDuration;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private GameObject CreateGameObject(string name)
        {
            var instance = new GameObject(name);
            createdObjects.Add(instance);
            return instance;
        }

        private WeaponRuntime CreateInactiveRuntime(string name)
        {
            GameObject instance = CreateGameObject(name);
            instance.SetActive(false);
            return instance.AddComponent<WeaponRuntime>();
        }
    }
}
