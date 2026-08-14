using System;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    public enum WeaponStatModifierOperation
    {
        Add = 0,
        Multiply = 1,
        Override = 2
    }

    /// <summary>Describes one deterministic modification to a weapon statistic.</summary>
    [Serializable]
    public struct WeaponStatModifier
    {
        [SerializeField] private WeaponStatDefinition definition;
        [SerializeField] private WeaponStatModifierOperation operation;
        [SerializeField] private float value;
        [SerializeField] private int priority;

        public WeaponStatDefinition Definition => definition;
        public WeaponStatModifierOperation Operation => operation;
        public float Value => value;
        public int Priority => priority;

        public WeaponStatModifier(
            WeaponStatDefinition definition,
            WeaponStatModifierOperation operation,
            float value,
            int priority = 0)
        {
            this.definition = definition;
            this.operation = operation;
            this.value = value;
            this.priority = priority;
        }
    }
}
