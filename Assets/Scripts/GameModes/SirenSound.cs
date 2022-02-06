using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class SirenSound : MonoBehaviour
{
    [SerializeField]
    GameObject sirenLights;

    [SerializeField]
    AudioClip middleSound;
    [SerializeField]
    AudioClip highSound;
    [SerializeField]
    AudioClip endSound;

    private event Action _onStartTimerEnded;
    private event Action _onEndTimerEnded;
    AudioSource _source;

    void Start()
    {
        sirenLights.SetActive(false);
        _source = GetComponent<AudioSource>();
    }

    public async void PlayStartGameSounds(Action DoAfter)
    {
        _onStartTimerEnded += DoAfter;

        _source.clip = middleSound;
        _source.Play();
        await Task.Delay(1000);
        _source.Play();
        await Task.Delay(1000);
        _source.Play();
        await Task.Delay(1000);
        _source.clip = highSound;
        _source.Play();

        _onStartTimerEnded?.Invoke();
        _onStartTimerEnded -= DoAfter;
    }

    public async void PlayEndGameSounds(Action DoAfter)
    {
        _onEndTimerEnded += DoAfter;

        sirenLights.SetActive(true);
        _source.clip = endSound;
        _source.Play();

        await Task.Delay(5000);
        sirenLights.SetActive(false);

        _onEndTimerEnded?.Invoke();
        _onEndTimerEnded -= DoAfter;
    }
}
