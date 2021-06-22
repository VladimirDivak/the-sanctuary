using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtistNamePanel : MonoBehaviour
{
    Text ArtistNameText;
    Image ArtistNameLogo;
    
    Coroutine C_ShowArtistNamePanel;
    LEDDisplayLogic DisplayLogicScript;
    
    [SerializeField] public Sprite[] Logos;

    void Start()
    {
        DisplayLogicScript = GameObject.FindObjectOfType<LEDDisplayLogic>();

        ArtistNameText = GameObject.Find("ArtistNameText").GetComponent<Text>();
        var textColor = ArtistNameText.color;
        textColor.a = 0;
        ArtistNameText.color = textColor;

        ArtistNameLogo = GameObject.Find("ArtistNameImg").GetComponent<Image>();
        var artistSpriteColor = ArtistNameLogo.color;
        artistSpriteColor.a = 0;
        ArtistNameLogo.color = artistSpriteColor;
        ArtistNameLogo.sprite = Logos[0];
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

        var _artistImage = ArtistNameLogo.GetComponentInChildren<Image>();
        var _artistImageColor = _artistImage.color;

        var _artistTextColor = ArtistNameText.color;

        if(_artistImageColor.a != 0 && _artistTextColor.a != 0)
        {
            while(_artistImageColor.a > 0.1f && _artistTextColor.a > 0.1f)
            {
                _artistImageColor.a = Mathf.Lerp(_artistImageColor.a, 0, 3 * Time.deltaTime);
                _artistImage.color = _artistImageColor;

                _artistTextColor.a = Mathf.Lerp(_artistTextColor.a, 0, 3 * Time.deltaTime);
                ArtistNameText.color = _artistTextColor;

                yield return null;
            }

            _artistImageColor.a = 0;
            _artistImage.color = _artistImageColor;

            _artistTextColor.a = 0;
            ArtistNameText.color = _artistTextColor;
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
            ArtistNameText.color = _artistTextColor;

            yield return null;
        }

        _artistImageColor.a = 1;
        _artistImage.color = _artistImageColor;

        _artistTextColor.a = 1;
        ArtistNameText.color = _artistTextColor;

        yield break;
    }
}
