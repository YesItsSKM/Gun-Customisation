using UnityEngine;
using UnityEngine.UI;

public class GunStatsUIManager : MonoBehaviour
{
    [SerializeField] GameObject gunsStatsUICanvas;

    [Header("Stat UI Elements")]
    [SerializeField] private Slider accuracySlider;
    [SerializeField] private Slider rangeSlider;
    [SerializeField] private Slider fireRateSlider;
    [SerializeField] private Slider recoilSlider;
    [SerializeField] private Slider reloadTimeSlider;
    [SerializeField] private Slider ammoSlider;

    private void OnEnable()
    {
        GunEvents.OnGunInspected += ShowStatsUI;
        GunEvents.OnGunInspectionEnded += HideStatsUI;
    }

    private void OnDisable()
    {
        GunEvents.OnGunInspected -= ShowStatsUI;
        GunEvents.OnGunInspectionEnded -= HideStatsUI;
    }

    private void Start()
    {
        HideStatsUI();
    }

    void ShowStatsUI(GunData gunData)
    {
        gunsStatsUICanvas.gameObject.SetActive(true);

        accuracySlider.value = Mathf.InverseLerp(0f, 100f, gunData.accuracy);
        rangeSlider.value = Mathf.InverseLerp(0f, 2000f, gunData.range);
        fireRateSlider.value = Mathf.InverseLerp(0f, 1000f, gunData.fireRate);
        recoilSlider.value = Mathf.InverseLerp(0f, 100f, gunData.recoil);
        reloadTimeSlider.value = Mathf.InverseLerp(0f, 5f, gunData.reloadTime);
        ammoSlider.value = Mathf.InverseLerp(0f, 100f, gunData.ammo);
    }

    void HideStatsUI()
    {
        gunsStatsUICanvas.gameObject.SetActive(false);
    }
}
