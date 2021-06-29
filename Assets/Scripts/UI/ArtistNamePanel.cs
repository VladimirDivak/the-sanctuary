using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//  класс описывает поведение элемента UI,
//  который отображает, чей трек сейчас играет на фоне
//
//  проблема кода - все анимации затухания того или иного
//  элемента UI прописаны вручную для каждого, по
//  этому одна из главных целей в дальнейшем для UI -
//  это написание одной-единственной логики для реализации
//  этого эффекта

public class ArtistNamePanel : MonoBehaviour
{
    Text _artistNameText;
    Image _artistNameLogo;
    
    Coroutine C_ShowArtistNamePanel;
    LEDDisplayLogic _displayLogicScript;
    
    [SerializeField] public Sprite[] Logos;

    void Start()
    {
        _displayLogicScript = GameObject.FindObjectOfType<LEDDisplayLogic>();

        _artistNameText = GameObject.Find("ArtistNameText").GetComponent<Text>();
        var textColor = _artistNameText.color;
        textColor.a = 0;
        _artistNameText.color = textColor;

        _artistNameLogo = GameObject.Find("ArtistNameImg").GetComponent<Image>();
        var artistSpriteColor = _artistNameLogo.color;
        artistSpriteColor.a = 0;
        _artistNameLogo.color = artistSpriteColor;
        _artistNameLogo.sprite = Logos[0];
    }

    public void ShowNewArtistName()
    {
        if(C_ShowArtistNamePanel != null)
        {
            StopCoroutine(C_ShowArtistNamePanel);
            C_ShowArtistNamePanel = null;
        }

        C_ShowArtistNamePanel = StartCoroutine(ShowArtistNamePanel());
    }

    IEnumerator ShowArtistNamePanel()
    {
        yield return null;

        var _artistImage = _artistNameLogo.GetComponentInChildren<Image>();
        var _artistImageColor = _artistImage.color;

        var _artistTextColor = _artistNameText.color;

        if(_artistImageColor.a != 0 && _artistTextColor.a != 0)
        {
            while(_artistImageColor.a > 0.1f && _artistTextColor.a > 0.1f)
            {
                _artistImageColor.a = Mathf.Lerp(_artistImageColor.a, 0, 3 * Time.deltaTime);
                _artistImage.color = _artistImageColor;

                _artistTextColor.a = Mathf.Lerp(_artistTextColor.a, 0, 3 * Time.deltaTime);
                _artistNameText.color = _artistTextColor;

                yield return null;
            }

            _artistImageColor.a = 0;
            _artistImage.color = _artistImageColor;

            _artistTextColor.a = 0;
            _artistNameText.color = _artistTextColor;
        }

        switch(LEDDisplayLogic.CurrentArtsistName)
        {
            case "clvne":
                _artistImage.sprite = Logos[0];
                break;

            case "maxim maxay":
                _artistImage.sprite = Logos[1];
                break;

            case "vee":
                _artistImage.sprite = Logos[2];
                break;

            case "goldman":
                _artistImage.sprite = Logos[3];
                break;
        }

        while(_artistImageColor.a < 0.99f)
        {
            _artistImageColor.a = Mathf.Lerp(_artistImageColor.a, 1, 1.2f * Time.deltaTime);
            _artistImage.color = _artistImageColor;

            _artistTextColor.a = Mathf.Lerp(_artistTextColor.a, 1, 1.2f * Time.deltaTime);
            _artistNameText.color = _artistTextColor;

            yield return null;
        }

        _artistImageColor.a = 1;
        _artistImage.color = _artistImageColor;

        _artistTextColor.a = 1;
        _artistNameText.color = _artistTextColor;

        yield break;
    }
}
