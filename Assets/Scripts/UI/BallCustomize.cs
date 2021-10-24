using System.Collections;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System;

//  скрипт поведения окна кастомизации игрового мяча  
//

public class BallCustomize : MonoBehaviour
{
    private Color _baseColor;
    private Color _linesColor;
    private Material _material;
    private Texture2D[] _patterns;

    [SerializeField]
    private Sprite[] PatternSprites;

    [SerializeField]
    private Texture2D CurrentPattern;

    public int UsePattern;

    [SerializeField]
    public GameObject PatternButton;

    private Image _baseColorTexture;
    private Image _linesColorTexture;

    private GameObject _colorPicker;
    private Texture2D _colorPickerTexture;

    private Coroutine C_ColorPickerSetColor;

    private GameObject _description;
    private GameObject _confirmButton;

    public static string baseColorID = "Color_da2856c20dc2474a80d2a860d0e7fa1b";
    public static string linesColorID = "Color_eddccc71b84c49ab919570d1ac6ad4e1";
    public static string usePatternID = "Boolean_eff8438e3cb14ec1b5cfd5378285884c";
    public static string patternTextureID = "Texture2D_bd683b01ffb44a2b800238c9447ea3de";
    public static string useNetworkFresnelID = "UseNetworkFresnel";

    private List<Transform> _childrenTransform = new List<Transform>();
    private event Action OnFadeOutEvent;
    private Coroutine C_UIFadeEffect;

    void Awake()
    {
        OnFadeOutEvent += OnFadeOut;

        SetChildrenTransformList(transform);
        foreach(var objectTransform in _childrenTransform)
        {
            SetElemetColor(objectTransform, 0);
        }
    }

    void Start()
    {
        _material = FindObjectOfType<ObjectSpawner>().MyBallMaterial;
        _patterns = FindObjectOfType<ObjectSpawner>().Patterns;

        _description = GameObject.Find("Description");
        _description.SetActive(false);

        _confirmButton = GameObject.Find("Confirm_Button");

        _baseColorTexture = GameObject.Find("BaseColor_Texture").GetComponent<Image>();
        _linesColorTexture = GameObject.Find("LinesColor_Texture").GetComponent<Image>();

        _baseColor = _material.GetColor(baseColorID);
        _baseColorTexture.color = _baseColor;

        _linesColor = _material.GetColor(linesColorID);
        _linesColorTexture.color = _linesColor;

        UsePattern = _material.GetInt(usePatternID);
        CurrentPattern = _material.GetTexture(patternTextureID) as Texture2D;

        _colorPicker = GameObject.Find("ColorPicker");
        _colorPickerTexture = _colorPicker.GetComponent<Image>().mainTexture as Texture2D;

        _colorPicker.GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);

        for(int i = 0; i < PatternSprites.Length + 1; i++)
        {
            var Button = Instantiate(PatternButton, Vector2.zero, Quaternion.identity);
            var ButtonScript = Button.GetComponent<PatternButton>();

            ButtonScript._baseColor = Color.white;

            ButtonScript._selectedColor = Color.yellow;
            ButtonScript._selectedColor.a = 0.5f;

            Image ButtonImage = Button.GetComponent<Image>();
            RectTransform ButtonRect = Button.GetComponent<RectTransform>();
            ButtonRect.SetParent(GameObject.Find("Patterns_Panel").transform);
            ButtonRect.localScale = new Vector3(1, 1, 1);

            if(i != PatternSprites.Length)
            {
                Button.name = PatternSprites[i].name;
                ButtonImage.sprite = PatternSprites[i];

                if(CurrentPattern.name.Contains(Button.name) && UsePattern == 1) ButtonImage.color = Button.GetComponent<PatternButton>()._selectedColor;
            }
            else
            {
                Button.name = "None";
                if(UsePattern == 0) ButtonImage.color = ButtonImage.color = ButtonScript._selectedColor;
            }
        }
    }

    private void OnFadeOut()
    {
        Network.accountData.baseColor = "#" + ColorUtility.ToHtmlStringRGB(_baseColor);
        Network.accountData.linesColor = "#" + ColorUtility.ToHtmlStringRGB(_linesColor);

        if(UsePattern == 1)
        {
            Network.accountData.usePattern = true;
            Network.accountData.patternName = CurrentPattern.name;
        }
        else
        {
            Network.accountData.usePattern = false;
            Network.accountData.patternName = string.Empty;
        }

        FindObjectOfType<Network>().SendServerData("ServerRegistration", Network.accountData);
    }

    void SetChildrenTransformList(Transform objectTransform)
    {
        if(!objectTransform.name.Contains("Description"))
        {
            _childrenTransform.Add(objectTransform);
            if(objectTransform.childCount != 0)
            {
                for(int i = 0; i < objectTransform.childCount; i++)
                    SetChildrenTransformList(objectTransform.GetChild(i));
            }
        }
    }

    void SetElemetColor(Transform objectTransform, float newAlpha)
    {
        float alphaParam = newAlpha;

        if(!objectTransform.name.Contains("Description"))
        {
            if(objectTransform.name == "BallCustomizePanel")
                alphaParam = Mathf.Clamp(newAlpha, 0, 0.66f);
            else if(objectTransform.name == "ColorPicker")
               alphaParam = Mathf.Clamp(newAlpha, 0, 0.2f);

            Text text;
            Image image;
            Outline outline;

            Color newColor;

            objectTransform.TryGetComponent<Text>(out text);
            objectTransform.TryGetComponent<Image>(out image);
            objectTransform.TryGetComponent<Outline>(out outline);

            if(text != null)
            {
                newColor = text.color;
                newColor.a = alphaParam;
                text.color = newColor;
            }
            if(image != null)
            {
                newColor = image.color;
                newColor.a = alphaParam;
                image.color = newColor;
            }
            if(outline != null)
            {
                newColor = outline.effectColor;
                newColor.a = alphaParam;
                outline.effectColor = newColor;
            }
        }
    }

    public void SetColor(string element, Color newColor)
    {
        switch(element)
        {
            case "BaseColor":
                _material.SetColor(baseColorID, newColor);
                _baseColorTexture.color = newColor;
                _baseColor = newColor;
                break;
            case "LinesColor":
                _material.SetColor(linesColorID, newColor);
                _linesColorTexture.color = newColor;
                _linesColor = newColor;
                break;
        }
    }

    public void ResetColor(string element)
    {
        switch(element)
        {
            case "BaseColor":
                _material.SetColor(baseColorID, _baseColor);
                break;
            case "LinesColor":
                _material.SetColor(linesColorID, _linesColor);
                break;
        }
    }

    public void SetPattern(string PatternName)
    {
        var PatternsList = _patterns.Where(x => x.name.Contains(PatternName)).ToList();

        if(PatternsList.Count != 0)
        {
            _material.SetInt(usePatternID, 1);
            _material.SetTexture(patternTextureID, PatternsList.Last());
            UsePattern = 1;
            CurrentPattern = PatternsList.Last();
        }
        else
        {
            _material.SetInt(usePatternID, 0);
            UsePattern = 0;
        }
    }

    public void OnCustomPanelConfirmPressed()
    {
        SetFadeEffect();
    }

    public void ActiveColorPicker(string Element)
    {
        Image PickerImage = _colorPicker.GetComponent<Image>();
        Color PickerAlpha = PickerImage.color;

        string ButtonName = $"{Element}_Button/Text";
        Text ButtonText = GameObject.Find(ButtonName).GetComponent<Text>();

        if(PickerImage.color.a < 1)
        {
            PickerAlpha.a = 1;
            PickerImage.color = PickerAlpha;

            _description.SetActive(true);
            _confirmButton.SetActive(false);

            if(C_ColorPickerSetColor != null)
            {
                StopCoroutine(C_ColorPickerSetColor);
                C_ColorPickerSetColor = null;
            }
            C_ColorPickerSetColor = StartCoroutine(ColorPickerSetColor(Element));

            ButtonText.text = "apply".ToUpper();
        }
        else if(PickerImage.color.a == 1 && ButtonText.text == "APPLY")
        {
            PickerAlpha.a = 0.2f;
            PickerImage.color = PickerAlpha;

            _description.SetActive(false);
            _confirmButton.SetActive(true);

            StopCoroutine(C_ColorPickerSetColor);
            C_ColorPickerSetColor = null;

            ButtonText.text = "change".ToUpper();
        }
    }

    public string[] GetBallColors()
    {
        return new string[2]
        {
            baseColorID,
            linesColorID
        };
    }

    private IEnumerator ColorPickerSetColor(string Element)
    {
        yield return null;

        RectTransform Rect = _colorPicker.GetComponent<RectTransform>();

        while(true)
        {
            Vector2 delta;
            float width;
            float hight;
            float x;
            float y;
            int textureX;
            int textureY;
            Color color = new Color();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, Input.mousePosition, null, out delta);

            width = Rect.rect.width;
            hight = Rect.rect.height;

            delta += new Vector2(width * .5f, hight * .5f);

            x = Mathf.Clamp(delta.x / width, 0f, 1f);
            y = Mathf.Clamp(delta.y / hight, 0f, 1f);

            textureX = Mathf.RoundToInt(x * _colorPickerTexture.width);
            textureY = Mathf.RoundToInt(y * _colorPickerTexture.height);

            color = _colorPickerTexture.GetPixel(textureX, textureY);
            if(Input.GetMouseButton(0) && y < 1) SetColor(Element, color);
            yield return null;
        }
    }

    public void SetFadeEffect()
    {
        if(C_UIFadeEffect != null)
        {
            StopCoroutine(C_UIFadeEffect);
            C_UIFadeEffect = null;
        }
        C_UIFadeEffect = StartCoroutine(FadeEffect());
    }

    private IEnumerator FadeEffect()
    {
        float lerpProc = 0;
        Color baseColor = GetComponent<Image>().color;
        float alpha;

        if(baseColor.a > 0.6f)
        {
            while(lerpProc < 1)
            {
                alpha = Mathf.Lerp(1, 0, lerpProc);
                foreach(var item in _childrenTransform)
                    SetElemetColor(item, alpha);

                lerpProc += 8 * Time.deltaTime;
                yield return null;
            }

            foreach(var item in _childrenTransform)
                SetElemetColor(item, 0);

            yield return new WaitForSeconds(0.1f);

            OnFadeOutEvent?.Invoke();
            lerpProc = 0;
            yield break;
        }

        while(lerpProc < 1)
        {
            alpha = Mathf.Lerp(0, 1, lerpProc);
            foreach(var item in _childrenTransform)
                SetElemetColor(item, alpha);

            lerpProc += 8 * Time.deltaTime;
            yield return null;
        }

        foreach(var item in _childrenTransform)
            SetElemetColor(item, 1);

        yield break;
    }
}