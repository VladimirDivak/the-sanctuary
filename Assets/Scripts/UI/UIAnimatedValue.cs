using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class UIAnimatedValue : MonoBehaviour
{
    Text valueText;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
        valueText = GetComponent<Text>();
    }

    public void ShowValue(string value)
    {
        if(!gameObject.activeSelf) gameObject.SetActive(true);

        valueText.text = value;
        animator.Play("Show");
    }

    public void ShowValue(float value, float colorPosition)
    {
        if(!gameObject.activeSelf) gameObject.SetActive(true);

        valueText.text = value.ToString();
        valueText.color = Color.Lerp(Color.red, Color.green, colorPosition);

        animator.Play("Show");
    }

    public void Hide() => gameObject.SetActive(false);
}
