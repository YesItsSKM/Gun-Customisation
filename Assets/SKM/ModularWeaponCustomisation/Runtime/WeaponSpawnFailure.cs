namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Explains why a weapon definition could not be instantiated.</summary>
    public enum WeaponSpawnFailure
    {
        None = 0,
        MissingWeaponDefinition = 1,
        MissingPrefab = 2,
        MissingRuntime = 3,
        InitializationFailed = 4
    }
}
