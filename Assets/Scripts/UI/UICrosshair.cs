using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UICrosshair : MonoBehaviour
{
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Show()
    {
        _animator.SetBool("Show", true); 
    }

    public void Hide()
    {
        _animator.SetBool("Show", false);
    }
}
