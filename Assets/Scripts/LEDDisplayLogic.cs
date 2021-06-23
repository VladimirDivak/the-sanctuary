using System.Collections;
using System;
using UnityEngine;

//  данный класс работает с цифровым табло,
//  на котором отображается информация о
//  текущем треке, а также имеется логика
//  переключения треков на следующий и предыдущий

public class LEDDisplayLogic : MonoBehaviour
{
    [HideInInspector] public TextMesh Mesh;
    private string _text = string.Empty;
    private GameObject _nextButton;
    private GameObject _previewButton;
    private bool _ableToChangeText = true;
    private string[] _trackName;
    private float _trackTime;
    private AudioPlayerLogic _audioPlayerScript;

    private Coroutine _setTrackTime;
    public static string CurrentArtsistName;

    void Start()
    {
        _audioPlayerScript = FindObjectOfType<AudioPlayerLogic>();
        _nextButton = GameObject.Find("LED_next");
        _previewButton = GameObject.Find("LED_preview");
        Mesh = this.GetComponent<TextMesh>();
    }
    
    //  изначально подключение и отключение игроков
    //  от игры сопровождалось бегущей строкой на табло
    public IEnumerator OnNewMessage(string _msg)
    {
        _ableToChangeText = false;

        _nextButton.SetActive(false);
        _previewButton.SetActive(false);

        int lastsize = Mesh.fontSize;
        Mesh.fontSize = 115;
        Mesh.text = string.Empty;
        _msg += "            ";
        var chararray = _msg.ToCharArray();
        int count = 0;

        while(count < chararray.Length)
        {
            if(count <= 11)
            {
                Mesh.text += chararray[count];
            }
            else
            {
                var meshtext = Mesh.text.ToCharArray();
                for(int i = 0; i < meshtext.Length; i++)
                {
                    if( i != meshtext.Length - 1)
                    {
                        try
                        {
                            meshtext[i] = meshtext [i + 1];
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else
                    {
                        meshtext[i] = chararray[count];
                    }
                }
                Mesh.text = new string(meshtext);
            }
            count++;
            yield return new WaitForSecondsRealtime(0.15f);
        }

        Mesh.text = _text;
        Mesh.fontSize = lastsize;

        _nextButton.SetActive(true);
        _previewButton.SetActive(true);

        _ableToChangeText = true;

        yield return null;
    }

    public void SetLEDTrackName(string _name, float _length)
    {
        _trackName = _name.Split('-');
        _trackTime = _length;

        CurrentArtsistName = _trackName[0].ToLower();

        if(GameManager.GameStarted == true) GameObject.FindObjectOfType<ArtistNamePanel>().ShowNewArtistName();

        if(_setTrackTime == null)
            _setTrackTime = StartCoroutine(SetTrackTime(_length));
        else
        {
            StopCoroutine(_setTrackTime);
            _setTrackTime = StartCoroutine(SetTrackTime(_length));
        }
    }

    //  данный перечислитель высчитывает оставшееся время
    //  не совсем коррктно из-за отсутствия верного учёта
    //  погрешности в миллисекундах
    private IEnumerator SetTrackTime(float _length)
    {
        int min = Mathf.FloorToInt(_trackTime / 60);
        int sec = Mathf.RoundToInt(_trackTime % 60);
        float DynamicLength = _length;

        if(sec > 60)
        {
            min++;
            sec -= 60;
        }

        DateTime StartTrackDate = DateTime.Now;

        while(true)
        {
            yield return null;

            if((min == 0 && sec == 1) || _audioPlayerScript.Source.isPlaying == false)
            {
                break;
            }

            if(sec < 0)
            {
                min--;
                sec = 59;
            }
            if(sec >= 10)
            {
                _text = $"{_trackName[0]}\n{_trackName[1]}\n-{min}:{sec}";
            }
            else
            {
                _text = $"{_trackName[0]}\n{_trackName[1]}\n-{min}:0{sec}";
            }

            if(_ableToChangeText) Mesh.text = _text;

            if(DynamicLength >= 1)
            {
                yield return new WaitForSecondsRealtime(1);
                sec--;
                DynamicLength -= 1.0f;
            }
            else
            {
                yield return new WaitForSecondsRealtime(DynamicLength);
                DynamicLength = 0;
            }
        }

        _audioPlayerScript.OnNextTrack();

        yield return null;
    }
}
