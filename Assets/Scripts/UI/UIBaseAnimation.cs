using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBaseAnimation : MonoBehaviour
{
    protected Coroutine c_fadeAnimation;
    protected Color _color;

    public void StartFadeAnimation(bool isStart)
    {
        if(c_fadeAnimation != null)
        {
            StopCoroutine(c_fadeAnimation);
            c_fadeAnimation = null;
        }

        c_fadeAnimation = StartCoroutine(FadeAnimationRoutine(isStart));
    }

    IEnumerator FadeAnimationRoutine(bool isStart)
    {
        float lerpProgress = 0;
        float lerpTime = 2;

        float startValue = _color.a;
        float endValue = 1;

        Color newColor = _color;

        if(isStart) endValue = 0;

        while(lerpProgress < 1)
        {
            newColor.a = Mathf.Lerp(startValue, endValue, lerpProgress);
            _color = newColor;

            lerpProgress += lerpTime * Time.deltaTime;

            yield return null;
        }

        newColor.a = endValue;
        _color = newColor;

        yield break;
    }
}
