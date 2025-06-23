using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class DepthOfFieldManager : MonoBehaviour
{
    #region Private Variables

    private Volume postProcessVolume;
    private DepthOfField depthOfField;
    private float dofDistance_shallowFocus = 1.5f;
    private float dofDistance_deepFocus = 2.25f;

    #endregion

    void Start()
    {
        postProcessVolume = FindAnyObjectByType<Volume>();

        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out depthOfField))
        {
            depthOfField.active = true;
            depthOfField.mode.value = DepthOfFieldMode.Bokeh;
            depthOfField.focusDistance.value = dofDistance_deepFocus;
        }
    }

    public IEnumerator CR_BlurBackground(bool turnBlurOn = false, float blurLerpDuration = 0.3f)
    {
        float startValue = depthOfField.focusDistance.value;
        float endValue = turnBlurOn ? dofDistance_shallowFocus : dofDistance_deepFocus;

        float elapsedTime = 0f;

        while (elapsedTime < blurLerpDuration)
        {
            depthOfField.focusDistance.value = Mathf.Lerp(startValue, endValue, elapsedTime / blurLerpDuration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        depthOfField.focusDistance.value = endValue;

        yield return null;
    }

}
