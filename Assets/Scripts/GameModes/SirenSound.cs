using System.Collections;
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

    [SerializeField]
    public UnityEvent OnStartTimerEnded;

    AudioSource _source;

    void Start()
    {
        sirenLights.SetActive(false);
        _source = GetComponent<AudioSource>();
    }

    public void PlayStartGameSirenSound() => StartCoroutine(StartGameRoutine());
    public void PlayEndGameSirenSound() => StartCoroutine(EndGameRoutine());

    IEnumerator StartGameRoutine()
    {
        _source.clip = middleSound;
        _source.Play();
        yield return new WaitForSeconds(1);
        _source.Play();
        yield return new WaitForSeconds(1);
        _source.Play();
        yield return new WaitForSeconds(1);
        _source.clip = highSound;
        _source.Play();
        OnStartTimerEnded?.Invoke();
    }

    IEnumerator EndGameRoutine()
    {
        sirenLights.SetActive(true);
        _source.clip = endSound;
        _source.Play();

        yield return new WaitForSeconds(5);
        sirenLights.SetActive(false);
    }
}
