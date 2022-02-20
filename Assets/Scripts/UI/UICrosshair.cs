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

    public void SetActive(bool value)
    {
       if(value) _animator.Play("Show");
       else _animator.Play("Hide");
    }
}
