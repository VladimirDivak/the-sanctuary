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
    private static GameObject _registrationPanel;
    private static GameObject _roomsPanel;
    private GameObject _currentPointCamElement;
    private GameObject _popUpText;
    private static GameObject _thirdPointLineText;
    private static GameObject _artistNamePanel;

    public static event Action OnFadeOut;

    private Coroutine _fadeRoutine;

    [SerializeField]
    public TMP_Text PlayerNickname;
    private static List<TMP_Text> _playersNicknames = new List<TMP_Text>();
    private Dictionary<string, Vector2> _playnerNicknamesPositions = new Dictionary<string, Vector2>();

    [SerializeField]
    public GameObject PopUpPanel;
    [SerializeField]
    private GameObject FadeTextureObject;
    private Image _fadeTexture;

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
        _fadeTexture = FadeTextureObject.GetComponent<Image>();
        SetFade(0.2f);

        _roomsPanel = GameObject.Find("RoomsPanel");
        _roomsPanel.SetActive(false);

        _thirdPointLineText = GameObject.Find("ThirdPointLineText");
        _thirdPointLineText.GetComponent<Text>().text = "";

        PopUpContainer = GameObject.Find("PopupContainer");

        _artistNamePanel = GameObject.Find("ArtistNamePanel");

        _crosshair = GameObject.Find("Crosshair");
        _crosshair.SetActive(false);

        _registrationPanel = GameObject.Find("LoginPanel");
        _registrationPanel.SetActive(false);
    }

    public static void SetThrowDataText(string data)
    {
        _thirdPointLineText.GetComponent<Text>().text = data;
    }

    public static void ShowGameUI(bool itsTrue)
    {
        _crosshair.SetActive(itsTrue);
        _artistNamePanel.SetActive(itsTrue);
        for(int i = 0; i < _playersNicknames.Count; i++)
        {
            _playersNicknames[i].gameObject.SetActive(itsTrue);
        }
    }

    public void ShowPopUpMessage(string _message, Color _color, PopUpMessageType _icon)
    {
        var UpperString = _message.ToUpper();
        StartCoroutine(PopUpMessage(UpperString, _color, _icon));
    }

    public static void ShowGameRoomsPanel(bool itsTrue)
    {
        _roomsPanel.SetActive(itsTrue);
    }

    public void SetFade(float time)
    {
        if(_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        _fadeRoutine = StartCoroutine(LerpFadeAnimation(time));
    }

    public static void ShowRegistrationPanel(bool itsTrue)
    {
        _registrationPanel.SetActive(itsTrue);
    }

    private IEnumerator LerpFadeAnimation(float time)
    {
        yield return null;

        float lerpProc = 0;
        Color newColor = _fadeTexture.color;
        float alphaState = _fadeTexture.color.a;

        if(newColor.a < 1)
        {
            while(lerpProc < 1)
            {
                newColor.a = Mathf.Lerp(alphaState, 1, lerpProc);
                _fadeTexture.color = newColor;
                lerpProc += time * Time.deltaTime;

                yield return null;
            }

            lerpProc = 0;

            newColor.a = 1;
            _fadeTexture.color = newColor;
            alphaState = _fadeTexture.color.a;

            OnFadeOut?.Invoke();
            yield return new WaitForSeconds(1);
        }

        while(lerpProc < 1)
        {
            newColor.a = Mathf.Lerp(alphaState, 0, lerpProc);
            _fadeTexture.color = newColor;
            lerpProc += time * Time.deltaTime;

            yield return null;
        }

        newColor.a = 0;
        _fadeTexture.color = newColor;

        yield break;
    }

    private IEnumerator PopUpMessage(string _message, Color _color, PopUpMessageType _icon)
    {
        var PopUp = Instantiate(PopUpPanel, Vector3.zero, Quaternion.identity);
        PopUp.SetActive(true);
        
        PopUpIcon = PopUp.transform.Find("PopupIcon").gameObject;
        _popUpText = PopUp.transform.Find("PopupText").gameObject;

        var _popuppanel = PopUp;

        var rectPopUp = PopUp.GetComponent<RectTransform>();
        rectPopUp.SetParent(PopUpContainer.transform);

        var panelImage = _popuppanel.GetComponent<Image>();
        var panelImageColor = panelImage.color;

        panelImageColor.a = 0;
        panelImage.color = panelImageColor;

        var panelText = _popUpText.GetComponent<Text>();
        var panelTextColor = panelText.color;
        panelText.text = _message;

        panelTextColor = _color;
        panelTextColor.a = 0;
        panelText.color = panelTextColor;

        var PopupIconImage = PopUpIcon.GetComponent<Image>();

        switch(_icon)
        {
            case PopUpMessageType.Info:
                PopupIconImage.sprite = PopUpIcons[0];
                break;
            case PopUpMessageType.Message:
                PopupIconImage.sprite = PopUpIcons[1];
                break;
            case PopUpMessageType.Error:
                PopupIconImage.sprite = PopUpIcons[2];
                break;
        }

        var PopupIconImageColor = PopupIconImage.color;
        PopupIconImageColor = _color;
        PopupIconImageColor.a = 0;

        while(panelImage.color.a < 0.6f && panelText.color.a < 0.9f)
        {
            panelImageColor.a = Mathf.Lerp(panelImageColor.a, 0.65f, 2 * Time.deltaTime);
            panelImage.color = panelImageColor;

            panelTextColor.a = Mathf.Lerp(panelTextColor.a, 1f, 2 * Time.deltaTime);
            panelText.color = panelTextColor;

            PopupIconImageColor.a = Mathf.Lerp(PopupIconImageColor.a, 1f, 2 * Time.deltaTime);
            PopupIconImage.color = PopupIconImageColor;

            yield return null;
        }

        yield return new WaitForSeconds(3);

        while(panelImage.color.a > 0.02f && panelText.color.a > 0.02f)
        {
            panelImageColor.a = Mathf.Lerp(panelImageColor.a, 0f, 2 * Time.deltaTime);
            panelImage.color = panelImageColor;

            panelTextColor.a = Mathf.Lerp(panelTextColor.a, 0f, 2 * Time.deltaTime);
            panelText.color = panelTextColor;

            PopupIconImageColor.a = Mathf.Lerp(PopupIconImageColor.a, 0f, 2 * Time.deltaTime);
            PopupIconImage.color = PopupIconImageColor;

            yield return null;
        }

        GameObject.Destroy(PopUp);

        yield return null;
    }

    public void ShowThirdPointLineText(bool _show)
    {
        if(_show) _thirdPointLineText.SetActive(true);
        else _thirdPointLineText.SetActive(false);
    }

    public void AddPlayerNickname(string name)
    {
        var data = Instantiate(PlayerNickname, Vector3.zero, Quaternion.identity);
        data.text = name;
        data.GetComponent<RectTransform>().SetParent(this.transform);

        _playersNicknames.Add(data);
        _playnerNicknamesPositions.Add(name, Vector2.zero);
        if(_playersNicknames.Count == 1)
        {
            StartCoroutine(SetNicknamesPositions());
        }
    }

    public void RemovePlayerNickname(string name)
    {
        var playerNicknameText = _playersNicknames.Find(x => x.text == name);
        Destroy(playerNicknameText);

        _playersNicknames.Remove(playerNicknameText);
        _playnerNicknamesPositions.Remove(name);
    }

    public void SetPlayerNicknamePosition(string name, Vector3 ballPosition)
    {
        _playnerNicknamesPositions[name] = ballPosition;
    }

    private IEnumerator SetNicknamesPositions()
    {
        while(_playersNicknames.Count != 0)
        {
            for(int i = 0; i < _playersNicknames.Count; i++)
            {
                var name = _playersNicknames[i].text;
                Vector2 newPosition =  _playnerNicknamesPositions[name];

                _playersNicknames[i].transform.position = newPosition;
            }
            yield return null;
        }
        yield break;
    }
}
