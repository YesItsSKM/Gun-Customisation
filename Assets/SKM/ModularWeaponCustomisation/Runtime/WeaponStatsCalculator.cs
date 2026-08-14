using System;
using System.Collections.Generic;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>
    /// Calculates effective stats without mutating authored ScriptableObject data.
    /// Add modifiers are summed, multiply modifiers are multiplied, and the
    /// highest-priority override wins. A later override wins a priority tie.
    /// </summary>
    public static class WeaponStatsCalculator
    {
        private struct ModifierAccumulator
        {
            public float Additive;
            public float Multiplier;
            public bool HasOverride;
            public float OverrideValue;
            public int OverridePriority;
            public int OverrideSequence;
        }

        public static WeaponStatsSnapshot Calculate(
            IReadOnlyList<WeaponStatValue> baseStats,
            IEnumerable<WeaponStatModifier> modifiers)
        {
            var definitionsInOrder = new List<WeaponStatDefinition>();
            var baseValues = new Dictionary<WeaponStatDefinition, float>();
            var accumulatedModifiers =
                new Dictionary<WeaponStatDefinition, ModifierAccumulator>();

            if (baseStats != null)
            {
                foreach (WeaponStatValue baseStat in baseStats)
                {
                    WeaponStatDefinition definition = baseStat.Definition;
                    if (definition == null)
                    {
                        continue;
                    }

                    EnsureValidDefinition(definition);
                    EnsureFinite(baseStat.Value, definition, "base value");

                    if (!baseValues.ContainsKey(definition))
                    {
                        definitionsInOrder.Add(definition);
                    }

                    baseValues[definition] = baseStat.Value;
                }
            }

            int sequence = 0;
            if (modifiers != null)
            {
                foreach (WeaponStatModifier modifier in modifiers)
                {
                    WeaponStatDefinition definition = modifier.Definition;
                    if (definition == null)
                    {
                        sequence++;
                        continue;
                    }

                    EnsureValidDefinition(definition);
                    EnsureFinite(modifier.Value, definition, "modifier");

                    if (!baseValues.ContainsKey(definition) &&
                        !accumulatedModifiers.ContainsKey(definition))
                    {
                        definitionsInOrder.Add(definition);
                    }

                    if (!accumulatedModifiers.TryGetValue(
                            definition,
                            out ModifierAccumulator accumulator))
                    {
                        accumulator.Multiplier = 1f;
                        accumulator.OverridePriority = int.MinValue;
                        accumulator.OverrideSequence = -1;
                    }

                    switch (modifier.Operation)
                    {
                        case WeaponStatModifierOperation.Add:
                            accumulator.Additive += modifier.Value;
                            break;

                        case WeaponStatModifierOperation.Multiply:
                            accumulator.Multiplier *= modifier.Value;
                            break;

                        case WeaponStatModifierOperation.Override:
                            bool higherPriority = modifier.Priority > accumulator.OverridePriority;
                            bool laterTie = modifier.Priority == accumulator.OverridePriority &&
                                            sequence > accumulator.OverrideSequence;
                            if (!accumulator.HasOverride || higherPriority || laterTie)
                            {
                                accumulator.HasOverride = true;
                                accumulator.OverrideValue = modifier.Value;
                                accumulator.OverridePriority = modifier.Priority;
                                accumulator.OverrideSequence = sequence;
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException(
                                nameof(modifiers),
                                modifier.Operation,
                                "Unsupported weapon stat modifier operation.");
                    }

                    accumulatedModifiers[definition] = accumulator;
                    sequence++;
                }
            }

            var calculatedValues = new List<WeaponStatValue>(definitionsInOrder.Count);
            foreach (WeaponStatDefinition definition in definitionsInOrder)
            {
                baseValues.TryGetValue(definition, out float baseValue);

                float calculatedValue = baseValue;
                if (accumulatedModifiers.TryGetValue(
                        definition,
                        out ModifierAccumulator accumulator))
                {
                    calculatedValue = (baseValue + accumulator.Additive) * accumulator.Multiplier;
                    if (accumulator.HasOverride)
                    {
                        calculatedValue = accumulator.OverrideValue;
                    }
                }

                EnsureFinite(calculatedValue, definition, "calculated value");
                calculatedValues.Add(
                    new WeaponStatValue(definition, definition.Clamp(calculatedValue)));
            }

            return new WeaponStatsSnapshot(calculatedValues);
        }

        private static void EnsureFinite(
            float value,
            WeaponStatDefinition definition,
            string valueKind)
        {
            if (!float.IsNaN(value) && !float.IsInfinity(value))
            {
                return;
            }

            throw new ArgumentException(
                $"The {valueKind} for stat '{definition.Id}' must be finite.");
        }

        private static void EnsureValidDefinition(WeaponStatDefinition definition)
        {
            if (definition.IsValid)
            {
                return;
            }

            throw new ArgumentException(
                $"Stat definition '{definition.name}' has an invalid ID or range.");
        }
    }
}
