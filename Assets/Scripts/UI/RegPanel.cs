using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using System;

public class RegPanel : MonoBehaviour
{
    private GameObject _loginBlock;
    private GameObject _regBlock;
    private List<Transform> _childrenTransform = new List<Transform>();

    private Coroutine C_ErrorText;
    private Coroutine C_LoginValid;
    private Coroutine C_UIFadeEffect;

    private UserInterface _UI;
    private Network _network;

    private event Action<string> OnFadeOutEvent;

    private void Start()
    {
        SetChildrenTransformList(transform);
        foreach(var objectTransform in _childrenTransform)
        {
            SetElemetColor(objectTransform, 0);
        }

        OnFadeOutEvent += OnFadeOut;

        _network = FindObjectOfType<Network>();
        _UI = FindObjectOfType<UserInterface>();
        
        Color RedColor = Color.red;
        RedColor.a = 0;

        _loginBlock = GameObject.Find("LoginBlock");
        _regBlock = GameObject.Find("RegBlock");
        
        var ErrorTextList = transform.GetComponentsInChildren<Text>().Where(x => x.name.Contains("ErrorText")).ToList();    
        var InputFieldList = transform.GetComponentsInChildren<InputField>().ToList();

        for(int i = 0; i < ErrorTextList.Count; i++)
            ErrorTextList[i].color = RedColor;  

        for(int i = 0; i < InputFieldList.Count; i++)
            InputFieldList[i].GetComponent<Shadow>().effectColor = RedColor;

        _regBlock.SetActive(false);
    }

    void SetChildrenTransformList(Transform objectTransform)
    {
        if(!objectTransform.name.Contains("ErrorText"))
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

        if(!objectTransform.name.Contains("ErrorText"))
        {
            if(objectTransform.name == "LoginPanel")
                alphaParam = Mathf.Clamp(newAlpha, 0, 0.66f);

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

    private void OnFadeOut(string buttonName)
    {
        switch(buttonName)
        {
            case "confirm":
            _regBlock.SetActive(false);
            _loginBlock.SetActive(false);

            FindObjectOfType<UserInterface>().BallCustomizePanel.SetActive(true);
            FindObjectOfType<BallCustomize>().SetFadeEffect();
            break;

            case "signin":
            _regBlock.SetActive(true);
             _loginBlock.SetActive(false);
            break;

            case "back":
            _regBlock.SetActive(false);
            _loginBlock.SetActive(true);
            break;
        }
        OnFadeOutEvent -= OnFadeOut;
        if(buttonName == "confirm")
            GameObject.Find("LoginPanel").SetActive(false);
    }

    public void OnLoginButtonPressed()
    {
        bool AllIsCorrect = false;
        Dictionary<string, string> TextElements = new Dictionary<string, string>();

        var _loginField = GetComponentsInChildren<InputField>().Last(x => x.name.Contains("_Login"));
        var _loginErrorText = GetComponentsInChildren<Text>().Last(x => x.name.Contains("_Login"));

        var _passwordField = GetComponentsInChildren<InputField>().Last(x => x.name.Contains("_Password"));
        var _passwordErrorText = GetComponentsInChildren<Text>().Last(x => x.name.Contains("_Password"));

        string ErrorText = string.Empty;


        // chek a login field
        if(_loginField.text.Length <= 3)
        {
            if(_loginField.text.Length != 0) ErrorText = "login is small";
            else ErrorText = "login is empty";

            TextElements.Add(_loginErrorText.name, ErrorText);
        }

        // chek a password field
        if(_passwordField.text == string.Empty)
        {
            ErrorText = "password is empty";

            TextElements.Add(_passwordErrorText.name, ErrorText);
        }

        if(TextElements.Count == 0) AllIsCorrect = true;

        if(AllIsCorrect)
        {
            _network.SendServerData("ServerAuthorzation", false, _loginField.text, _passwordField.text);

            if(C_LoginValid != null) StopCoroutine(C_LoginValid);
            C_LoginValid = StartCoroutine(ChekValdiation(_loginErrorText));
        }
        else
        {
            if(C_ErrorText != null) StopCoroutine(C_ErrorText);
            C_ErrorText = StartCoroutine(ShowErrorText(TextElements));
        }
    }

    public void OnConfirmPressed()
    {
        bool AllIsCorrect = false;
        Dictionary<string, string> TextElements = new Dictionary<string, string>();

        var _newLoginField = GetComponentsInChildren<InputField>().Last(x => x.name.Contains("_NewLogin"));
        var _newLoginErrorText = GetComponentsInChildren<Text>().Last(x => x.name.Contains("_NewLogin"));

        var _newPasswordField = GetComponentsInChildren<InputField>().Last(x => x.name.Contains("_NewPassword"));
        var _newPasswordErrorText = GetComponentsInChildren<Text>().Last(x => x.name.Contains("_NewPassword"));

        var _newPasswordFieldAgain = GetComponentsInChildren<InputField>().Last(x => x.name.Contains("_PassConfirm"));
        var _newPasswordErrorTextAgain = GetComponentsInChildren<Text>().Last(x => x.name.Contains("_PassConfirm"));

        string ErrorText = string.Empty;


        // chek a login field
        if(_newLoginField.text.Length <= 3)
        {
            if(_newLoginField.text.Length != 0) ErrorText = "login is small";
            else ErrorText = "login is empty";

            TextElements.Add(_newLoginErrorText.name, ErrorText);
        }

        // chek a password field
        if(_newPasswordField.text == string.Empty)
        {
            ErrorText = "password is empty";

            TextElements.Add(_newPasswordErrorText.name, ErrorText);
            TextElements.Add(_newPasswordErrorTextAgain.name, ErrorText);
        }

        // chek a confirm password field
        if(!string.Equals(_newPasswordFieldAgain.text, _newPasswordField.text))
        {
            ErrorText = "password mismatch";

            TextElements.Add(_newPasswordErrorTextAgain.name, ErrorText);
        }

        if(TextElements.Count == 0) AllIsCorrect = true;

        if(AllIsCorrect)
        {
            _network.SendServerData("ServerLoginChecker", _newLoginField.text);

            if(C_LoginValid != null) StopCoroutine(C_LoginValid);
            C_LoginValid = StartCoroutine(ChekValdiation(_newLoginErrorText));
        }
        else
        {
            if(C_ErrorText != null) StopCoroutine(C_ErrorText);
            C_ErrorText = StartCoroutine(ShowErrorText(TextElements));
        }
    }

    public void OnSignInPressed()
    {
        OnFadeOutEvent += OnFadeOut;
        SetFadeEffect("signin");
    }
    public void OnBackPressed()
    {
        OnFadeOutEvent += OnFadeOut;
        SetFadeEffect("back");
    }

    private IEnumerator ShowErrorText(Dictionary<string, string> ErrorTextObjects)
    {
        List<Shadow> ActiveShadows = new List<Shadow>();
        List<Text> ActiveErrorsText = new List<Text>();

        List<Text> ErrorsText = transform.GetComponentsInChildren<Text>().ToList();

        float LerpProgress = 0;
        Color RedColor = Color.red;
        RedColor.a = 1;

        yield return null;

        foreach(var TextName in ErrorTextObjects.Keys)
        {
            var Item = ErrorsText.Where(x => x.name == TextName).ToList().Last();
            Item.text = ErrorTextObjects[TextName].ToUpper();
            Item.color = RedColor;
            ActiveErrorsText.Add(Item);

            var FieldKey = TextName.Split('_');

            var FieldShadow = GetComponentsInChildren<InputField>().Where(x => x.name.Contains(FieldKey[1])).ToList().Last().GetComponent<Shadow>();
            FieldShadow.effectColor = RedColor;
            ActiveShadows.Add(FieldShadow);
        }

        yield return new WaitForSeconds(2);

        while(LerpProgress <= 1)
        {
            for(int i = 0; i < ActiveErrorsText.Count; i++)
            {
                RedColor.a = Mathf.Lerp(1, 0, LerpProgress);
                ActiveErrorsText[i].color = RedColor;
                ActiveShadows[i].effectColor = RedColor;
            }

            LerpProgress += 1.6f * Time.deltaTime;

            yield return null;
        }
    }

    private IEnumerator ChekValdiation(Text ErrorTextField)
    {
        yield return new WaitForSeconds(0.5f);
        
        if(ErrorTextField.name.Contains("_Login"))
        {
            if(Network.accountData.login != null)
            {
                foreach(var ball in GameObject.FindGameObjectsWithTag("Ball"))
                {
                    var material = ball.GetComponent<Renderer>().material;

                    Color baseColor;
                    Color linesColor;

                    ColorUtility.TryParseHtmlString(Network.accountData.baseColor, out baseColor);
                    ColorUtility.TryParseHtmlString(Network.accountData.linesColor, out linesColor);

                    material.SetColor(BallCustomize._baseColorID, baseColor);
                    material.SetColor(BallCustomize._linesColorID, linesColor);

                    if(Network.accountData.usePattern)
                    {
                        material.SetTexture(BallCustomize._patternTextureID,
                        BallCustomize._patternsStatic.Where(x => x.name == Network.accountData.patternName).ToList().FirstOrDefault());
                    }
                }

                FindObjectOfType<GUI>().SetFade(5f);
            }
            else
            {
                if(C_ErrorText != null) StopCoroutine(C_ErrorText);
                C_ErrorText = StartCoroutine(ShowErrorText(new Dictionary<string, string>()
                {
                    {ErrorTextField.name, "wrong login"}
                }));
            }
        }
        else
        {
            if(!Network.accountRegistrationState)
            {
                if(C_ErrorText != null) StopCoroutine(C_ErrorText);
                C_ErrorText = StartCoroutine(ShowErrorText(new Dictionary<string, string>()
                {
                    {ErrorTextField.name, "login is exists"}
                }));

                Network.accountRegistrationState = true;
            }
            else
            {
                var _newLoginField = GetComponentsInChildren<InputField>().Where(x => x.name.Contains("_NewLogin")).ToList().Last();
                var _newPasswordField = GetComponentsInChildren<InputField>().Where(x => x.name.Contains("_NewPassword")).ToList().Last();

                Network.accountData.login = _newLoginField.text;
                Network.accountData.password = _newPasswordField.text;


                OnFadeOutEvent += OnFadeOut;
                SetFadeEffect("confirm");
            }

        }
        yield return null;
    }

    public void SetFadeEffect(string buttonName)
    {
        if(C_UIFadeEffect != null)
        {
            StopCoroutine(C_UIFadeEffect);
            C_UIFadeEffect = null;
        }
        C_UIFadeEffect = StartCoroutine(FadeEffect(buttonName));
    }

    private IEnumerator FadeEffect(string buttonName)
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

                lerpProc += 10 * Time.deltaTime;
                yield return null;
            }

            foreach(var item in _childrenTransform)
                SetElemetColor(item, 0);

            yield return new WaitForSeconds(0.1f);

            OnFadeOutEvent?.Invoke(buttonName);
            lerpProc = 0;
        }

        while(lerpProc < 1)
        {
            alpha = Mathf.Lerp(0, 1, lerpProc);
            foreach(var item in _childrenTransform)
                SetElemetColor(item, alpha);

            lerpProc += 10 * Time.deltaTime;
            yield return null;
        }

        foreach(var item in _childrenTransform)
            SetElemetColor(item, 1);

        yield break;
    }
}
