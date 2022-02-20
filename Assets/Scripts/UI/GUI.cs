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
    [SerializeField] UICrosshair _crosshair;
    [SerializeField] UIScoreboard _scoreboard;
    [SerializeField] UITimer _timer;
    [SerializeField] UIAnimatedValue _accuracy;
    [SerializeField] UIAnimatedValue _points;
    [SerializeField] UIBestIdicators _bestIndicators;

    [SerializeField] GameObject PopUpPanel;
    [SerializeField] Sprite[] PopUpIcons;
    [SerializeField] GameObject PointCamElement;

    [HideInInspector] GameObject PopUpIcon;
    [HideInInspector] GameObject PopUpContainer;

    public static UICrosshair crosshair;
    public static UIScoreboard scoreboard;
    public static UITimer timer;
    public static UIAnimatedValue accuracy;
    public static UIAnimatedValue points;
    public static UIBestIdicators bestIndicators;

    void Awake()
    {
        crosshair = _crosshair;
        scoreboard = _scoreboard;
        timer = _timer;
        points = _points;
        accuracy = _accuracy;
        bestIndicators = _bestIndicators;
    }

    public static void ShowCrosshair(bool value)
    {
        crosshair.SetActive(value);
    }

    public static void ShowScoreboard(bool value)
    {
        if(value == false) scoreboard.Reset();
        scoreboard.gameObject.SetActive(value);
    }

    public static void ShowTimer(bool value)
    {
        if(value == false) timer.Reset();
        timer.gameObject.SetActive(value);
    }
}
