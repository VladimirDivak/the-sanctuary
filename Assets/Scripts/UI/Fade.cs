using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour
{
    public static Fade Instance { get; private set; }
    public Task actionTask;

    private event Action _onFadeIn;
    private Image _fadeImage;

    void Start()
    {
        Instance = this;

        _fadeImage = GetComponent<Image>();
        Color fadeColor = Color.black;
        fadeColor.a = 0;
        _fadeImage.color = fadeColor;

        if(gameObject.activeSelf) gameObject.SetActive(false);
    }

    public async void ShowAfter(Action sender)
    {
        if(!gameObject.activeSelf) gameObject.SetActive(true);

        Color startColor = _fadeImage.color;
        Color endColor = new Color(Color.black.r, Color.black.g, Color.black.b, 1);
        float lerpProgress = 0;
        float lerpMultipler = 2;

        while(lerpProgress < 1)
        {
            _fadeImage.color = Color.Lerp(startColor, endColor, lerpProgress);
            lerpProgress += lerpMultipler * Time.deltaTime;
            await Task.Yield();
        }
        _fadeImage.color = endColor;

        _onFadeIn += sender;
        _onFadeIn?.Invoke();

        if(actionTask != null)
            while(!actionTask.IsCompleted) await Task.Yield();

        lerpProgress = 0;
        startColor = endColor;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while(lerpProgress < 1)
        {
            _fadeImage.color = Color.Lerp(startColor, endColor, lerpProgress);
            lerpProgress += lerpMultipler * Time.deltaTime;
            await Task.Yield();            
        }

        _fadeImage.color = endColor;
        lerpProgress = 0;

        _onFadeIn -= sender;
        actionTask = null;

        this.gameObject.SetActive(false);
        await Task.Yield();
    }
}
