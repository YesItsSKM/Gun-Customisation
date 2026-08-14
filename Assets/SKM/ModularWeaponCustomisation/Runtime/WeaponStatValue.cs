using System;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Pairs a stat definition with a numeric value.</summary>
    [Serializable]
    public struct WeaponStatValue
    {
        [SerializeField] private WeaponStatDefinition definition;
        [SerializeField] private float value;

        public WeaponStatDefinition Definition => definition;
        public float Value => value;

        public WeaponStatValue(WeaponStatDefinition definition, float value)
        {
            this.definition = definition;
            this.value = value;
        }
    }
}
