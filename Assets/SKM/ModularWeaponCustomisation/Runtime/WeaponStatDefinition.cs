using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Defines one extensible weapon statistic and its valid range.</summary>
    [CreateAssetMenu(
        fileName = "WeaponStat_New",
        menuName = "SKM/Modular Weapon Customisation/Weapon Stat")]
    public sealed class WeaponStatDefinition : ScriptableObject
    {
        [SerializeField] private string id = "weapon-stat";
        [SerializeField] private string displayName = "Weapon Stat";
        [SerializeField] private string unit = string.Empty;
        [SerializeField] private float minimumValue = 0f;
        [SerializeField] private float maximumValue = 100f;
        [SerializeField] private bool higherIsBetter = true;

        public string Id => id;
        public string DisplayName => displayName;
        public string Unit => unit;
        public float MinimumValue => minimumValue;
        public float MaximumValue => maximumValue;
        public bool HigherIsBetter => higherIsBetter;
        public bool IsValid =>
            !string.IsNullOrWhiteSpace(id) &&
            IsFinite(minimumValue) &&
            IsFinite(maximumValue) &&
            maximumValue > minimumValue;

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, minimumValue, maximumValue);
        }

        public float Normalize(float value)
        {
            return Mathf.InverseLerp(minimumValue, maximumValue, value);
        }

        private void OnValidate()
        {
            id = id?.Trim();
            displayName = displayName?.Trim();
            unit = unit?.Trim();

            if (maximumValue <= minimumValue)
            {
                maximumValue = minimumValue + 1f;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
