using System;

public static class GunEvents
{
    public static event Action<GunData> OnGunInspected;
    public static event Action OnGunInspectionEnded;

    public static void FireGunInspectionEvent(GunData data)
    {
        OnGunInspected?.Invoke(data);
    }

    public static void FireGunInspectionEndedEvent()
    {
        OnGunInspectionEnded?.Invoke();
    }
}
