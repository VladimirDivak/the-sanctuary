using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class AccuracyValue : MonoBehaviour
{
    Text accuracyText;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ShowAccuracyValue(float value, float colorPosition) {
        accuracyText = GetComponent<Text>();
        accuracyText.text = value.ToString();
        accuracyText.color = Color.Lerp(Color.red, Color.green, colorPosition);

        animator.Play("ShowAccuracy");
    }

    public void Hide() => gameObject.SetActive(false);
}