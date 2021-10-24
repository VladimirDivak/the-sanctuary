using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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
    private static GameObject _crosshair;
    private GameObject _currentPointCamElement;

    [SerializeField]
    public GameObject PopUpPanel;
    [SerializeField]
    public Sprite[] PopUpIcons;
    [SerializeField]
    public GameObject PointCamElement;

    [HideInInspector] public GameObject PopUpIcon;
    [HideInInspector] public GameObject PopUpContainer;

    //  метод создаёт в углу интерфеса изображение с камеры, следующей
    //  за мячом в момент броска
    public void SetActivePointCam(bool isActive, Transform CurrentBall)
    {
        if(isActive)
        {
            RectTransform Rect;

            _currentPointCamElement = Instantiate(PointCamElement, new Vector2(Screen.width - 145, Screen.height - 145), Quaternion.identity);
            Rect = _currentPointCamElement.GetComponent<RectTransform>();
            Rect.SetParent(this.transform);
            FindObjectOfType<PointCam>().OnBallGrab(CurrentBall);
        }
        else
        {
            Destroy(_currentPointCamElement);
            _currentPointCamElement = null;
        }
    }

    void Start()
    {
        PopUpContainer = GameObject.Find("PopupContainer");

        _crosshair = GameObject.Find("Crosshair");
        _crosshair.SetActive(false);
    }

    public static void ShowGameUI(bool itsTrue)
    {
        _crosshair.SetActive(itsTrue);
    }
}
