using System.Collections;
using UnityEngine;
using TMPro;

//  логика работы кнопок главного меню
//  
//  кнопки увеличиваются при наведении на них
//  курсора и уменьшаются после перевода курсора
//  с их трансформа
//
//  при нажатии на кнопку запускается логика затухания

public class MenuButton : MonoBehaviour
{
    private UserInterface _ui;

    private Vector3 _initPosition;
    private Vector3 _transletePosition;

    private RectTransform _rectTransform;

    private float _transleteSpeed = 8f;
    private float _fadeSpeed = 2f;

    private TMP_Text _text;

    private Coroutine C_FadeOn;
    private Coroutine C_FadeOut;

    void Start()
    {
        _ui = GameObject.FindObjectOfType<UserInterface>();

        _rectTransform = transform.GetComponent<RectTransform>();
        _initPosition = _rectTransform.localPosition;
        _transletePosition = _initPosition;

        _text = GetComponent<TMP_Text>();
    }

    private void OnMouseEnter()
    {
        _transletePosition = _initPosition + new Vector3(0.3f, 0, 0);
    }
    
    private void OnMouseExit()
    {
        _transletePosition = _initPosition;
    }

    private void OnMouseDown()
    {
        if(_ui.AbleToMouseClick == true)
        {
            _ui.OnUIButtonPressed(name);
        }
    }

    private void Update()
    {
        _rectTransform.localPosition = Vector3.Lerp(_rectTransform.localPosition, _transletePosition, _transleteSpeed * Time.deltaTime);
    }

    public void FadeIn()
    {
        if(C_FadeOn != null)
        {
            StopCoroutine(C_FadeOn);
            C_FadeOn = null;
        }

        C_FadeOn = StartCoroutine(StartFadeIn());
    }

    public void FadeOut()
    {
        if(C_FadeOut != null)
        {
            StopCoroutine(C_FadeOut);
            C_FadeOut = null;
        }

        C_FadeOut = StartCoroutine(StartFadeOut());
    }

    private IEnumerator StartFadeIn()
    {
        yield return new WaitForSeconds(0.05f);

        Color MatColor = _text.color;

        float LerpProc = 0;

        while(LerpProc < 1)
        {
            MatColor.a = Mathf.Lerp(MatColor.a, 0, LerpProc);
            _text.color = MatColor;
            LerpProc += _fadeSpeed * Time.deltaTime;

            yield return null;
        }

        _rectTransform.localPosition = _initPosition;

        yield break;
    }

    private IEnumerator StartFadeOut()
    {
        yield return new WaitForSeconds(0.05f);

        Color MatColor = _text.color;

        float LerpProc = 0;

        while(LerpProc < 1)
        {
            MatColor.a = Mathf.Lerp(MatColor.a, 1, LerpProc);
            _text.color = MatColor;
            LerpProc += _fadeSpeed * Time.deltaTime;

            yield return null;
        }

        _rectTransform.localPosition = _initPosition;

        yield break;
    }
}
