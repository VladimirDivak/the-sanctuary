using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class AccuracyValue : MonoBehaviour
{
    Text accuracyText;

    public void ShowAccuracyValue(float value, float colorPosition) {
        accuracyText = GetComponent<Text>();
        accuracyText.text = value.ToString();
        accuracyText.color = Color.Lerp(Color.red, Color.green, colorPosition);

        StartCoroutine(ChangeAlphaValueRoutine());
    }

    IEnumerator ChangeAlphaValueRoutine() {
        float startValue = accuracyText.color.a;
        float endValue = 0f;
        float progressValue = 0f;
        float progressTime = 1f;
        Color newColor = accuracyText.color;

        yield return new WaitForSeconds(1);

        while(progressValue < 1) {
            newColor.a = Mathf.Lerp(startValue, endValue, progressTime);
            accuracyText.color = newColor;

            progressValue += progressTime * Time.deltaTime;
            yield return null;
        }

        newColor.a = endValue;
        accuracyText.color = newColor;

        gameObject.SetActive(false);
    }

    private void OnDisable() {
        Color currentColor = accuracyText.color;
        currentColor.a = 1;
        accuracyText.color = currentColor;
    }
}