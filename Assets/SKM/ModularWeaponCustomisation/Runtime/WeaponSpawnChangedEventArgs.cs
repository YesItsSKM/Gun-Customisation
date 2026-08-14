using System;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Describes a successful current-weapon replacement or clear.</summary>
    public sealed class WeaponSpawnChangedEventArgs : EventArgs
    {
        public WeaponSpawnChangedEventArgs(
            WeaponRuntime previousWeapon,
            WeaponRuntime currentWeapon)
        {
            PreviousWeapon = previousWeapon;
            CurrentWeapon = currentWeapon;
        }

        public WeaponRuntime PreviousWeapon { get; }
        public WeaponRuntime CurrentWeapon { get; }
    }
}
