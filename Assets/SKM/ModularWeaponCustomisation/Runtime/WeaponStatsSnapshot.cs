using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>An immutable set of calculated weapon statistics.</summary>
    public sealed class WeaponStatsSnapshot
    {
        private static readonly WeaponStatsSnapshot EmptySnapshot =
            new WeaponStatsSnapshot(new List<WeaponStatValue>());

        private readonly ReadOnlyCollection<WeaponStatValue> entries;
        private readonly Dictionary<WeaponStatDefinition, float> values;

        internal WeaponStatsSnapshot(IList<WeaponStatValue> calculatedEntries)
        {
            var entryCopy = new List<WeaponStatValue>(calculatedEntries);
            entries = entryCopy.AsReadOnly();
            values = new Dictionary<WeaponStatDefinition, float>(entryCopy.Count);

            foreach (WeaponStatValue entry in entryCopy)
            {
                if (entry.Definition != null)
                {
                    values[entry.Definition] = entry.Value;
                }
            }
        }

        public static WeaponStatsSnapshot Empty => EmptySnapshot;

        public IReadOnlyList<WeaponStatValue> Entries => entries;

        public bool TryGetValue(WeaponStatDefinition definition, out float value)
        {
            if (definition == null)
            {
                value = default;
                return false;
            }

            return values.TryGetValue(definition, out value);
        }

        public float GetValueOrDefault(WeaponStatDefinition definition, float defaultValue = 0f)
        {
            return TryGetValue(definition, out float value) ? value : defaultValue;
        }
    }
}
