using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

//  скрипт, описывающий логику работы всего (почти),
//  что связано с игровым инетрфесом
//  
//  сейчас я понимаю, что нужно было использовать
//  C#-интерфейсы, чтобы описать классы игровых окон,
//  а также создать здесть общий для всех элементов эффект
//  плавного затухания и появления элементов на экране

public enum PopUpMessageType
{
    Info,
    Message,
    Error
}

public class GUI : MonoBehaviour
{
    public static GUI Instance { get; private set; }

    private static GameObject _crosshair;
    private GameObject _currentPointCamElement;

    [SerializeField] UICrosshair crosshair;

    [SerializeField] public GameObject PopUpPanel;
    [SerializeField] public Sprite[] PopUpIcons;
    [SerializeField] public GameObject PointCamElement;

    [HideInInspector] public GameObject PopUpIcon;
    [HideInInspector] public GameObject PopUpContainer;

    public void ShowCrosshair()
    {
        crosshair.Show();
    }

    public void HideCrosshair()
    {
        crosshair.Hide();
    }

    void Start()
    {
        Instance = this;
    }
}
