using System.Collections;
using System;
using UnityEngine;

public class LEDDisplayLogic : MonoBehaviour
{
    [HideInInspector] public TextMesh Mesh;
    private string Text = string.Empty;
    private GameObject NextButton;
    private GameObject PreviewButton;
    private bool AbleToChangeText = true;
    private string[] TrackName;
    private float TrackTime;
    private AudioPlayerLogic AudioPlayerScript;

    private Coroutine setTrackTime;
    public static string CurrentArtsistName;

    void Start()
    {
        AudioPlayerScript = FindObjectOfType<AudioPlayerLogic>();
        NextButton = GameObject.Find("LED_next");
        PreviewButton = GameObject.Find("LED_preview");
        Mesh = this.GetComponent<TextMesh>();
    }

    public IEnumerator OnNewMessage(string _msg)
    {
        AbleToChangeText = false;

        NextButton.SetActive(false);
        PreviewButton.SetActive(false);

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

        Mesh.text = Text;
        Mesh.fontSize = lastsize;

        NextButton.SetActive(true);
        PreviewButton.SetActive(true);

        AbleToChangeText = true;

        yield return null;
    }

    public void SetLEDTrackName(string _name, float _length)
    {
        TrackName = _name.Split('-');
        TrackTime = _length;

        CurrentArtsistName = TrackName[0].ToLower();

        if(GameManager.GameStarted == true) GameObject.FindObjectOfType<ArtistNamePanel>().ShowNewArtistName();

        if(setTrackTime == null)
        {
            setTrackTime = StartCoroutine(SetTrackTime(_length));
        }
        else
        {
            StopCoroutine(setTrackTime);
            setTrackTime = StartCoroutine(SetTrackTime(_length));
        }
    }

    private IEnumerator SetTrackTime(float _length)
    {
        int min = Mathf.FloorToInt(TrackTime / 60);
        int sec = Mathf.RoundToInt(TrackTime % 60);
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

            if((min == 0 && sec == 1) || AudioPlayerScript.Source.isPlaying == false)
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
                Text = $"{TrackName[0]}\n{TrackName[1]}\n-{min}:{sec}";
            }
            else
            {
                Text = $"{TrackName[0]}\n{TrackName[1]}\n-{min}:0{sec}";
            }

            if(AbleToChangeText) Mesh.text = Text;

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

        AudioPlayerScript.OnNextTrack();

        yield return null;
    }
}
