using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthOfFieldManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Focus Distances")]
    [SerializeField, Min(0f)] private float shallowFocusDistance = 1.35f;
    [SerializeField, Min(0f)] private float deepFocusDistance = 2f;

    private DepthOfField depthOfField;
    private Coroutine focusTransition;

    public bool IsReady => depthOfField != null;

    private void Awake()
    {
        ResolveVolume();

        if (postProcessVolume == null)
        {
            Debug.LogError("Depth of field requires a Volume reference or a Volume tagged 'MainCameraVolume'.", this);
            return;
        }

        if (postProcessVolume.profile == null ||
            !postProcessVolume.profile.TryGet(out depthOfField))
        {
            Debug.LogError("The assigned Volume profile does not contain a Depth Of Field override.", postProcessVolume);
            return;
        }

        depthOfField.active = true;
        depthOfField.mode.value = DepthOfFieldMode.Bokeh;
        depthOfField.focusDistance.value = deepFocusDistance;
    }

    public void SetBackgroundBlur(bool enabled, float duration = 0.3f)
    {
        if (!IsReady)
        {
            return;
        }

        if (focusTransition != null)
        {
            StopCoroutine(focusTransition);
        }

        focusTransition = StartCoroutine(TransitionFocus(enabled, duration));
    }

    private void ResolveVolume()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = GetComponent<Volume>();
        }

        if (postProcessVolume != null)
        {
            return;
        }

        GameObject taggedVolume = GameObject.FindGameObjectWithTag("MainCameraVolume");
        if (taggedVolume != null)
        {
            postProcessVolume = taggedVolume.GetComponent<Volume>();
        }
    }

    private IEnumerator TransitionFocus(bool blurEnabled, float duration)
    {
        float startValue = depthOfField.focusDistance.value;
        float endValue = blurEnabled ? shallowFocusDistance : deepFocusDistance;

        if (duration <= 0f)
        {
            depthOfField.focusDistance.value = endValue;
            focusTransition = null;
            yield break;
        }

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            depthOfField.focusDistance.value = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        depthOfField.focusDistance.value = endValue;
        focusTransition = null;
    }
}
